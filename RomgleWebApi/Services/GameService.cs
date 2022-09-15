using RomgleWebApi.Data.Extensions;
using RomgleWebApi.Data.Models;
using System;
using System.Runtime.InteropServices;

namespace RomgleWebApi.Services
{
    public class GameService
    {
        private readonly ItemsService _itemsService;
        private readonly DailiesService _dailiesService;
        private readonly PlayersService _playersService;


        public GameService(ItemsService itemsService, DailiesService dailiesService, PlayersService playersService)
        {
            _itemsService = itemsService;
            _dailiesService = dailiesService;
            _playersService = playersService;
        }
        
        public Task<GuessResult> CheckGuessAsync(string guessId, string playerId, Gamemode mode, bool reskinsExcluded) 
            => WithPlayerAsync(playerId, async player =>
        {
            Item guess = await _itemsService.GetAsync(guessId);
            GuessResult result = new GuessResult();
            if (player.HasActiveGame())
            {
                if (player.CurrentGame!.Mode != mode)
                {
                    throw new Exception("Given gamemode is not the same as current game");
                }
            }
            else
            {
                Item newItem;
                if (mode == Gamemode.Daily)
                {
                    Daily dailyItem = await _dailiesService.GetDailyItem();
                    newItem = await _itemsService.GetAsync(dailyItem.Id);
                }
                else newItem = await _itemsService.GetRandomItemAsync(reskinsExcluded);
                await StartNewGameAsync(player, newItem.Id, mode, reskinsExcluded);
            }
            //TODO: Guest

            player.CurrentGame!.GuessItemIds.Add(guessId);
            await _playersService.UpdateAsync(player.Id, player);

            if (player.CurrentGame.TargetItemId == guessId)
            {
                await UpdatePlayerScoreAsync(player, GameResult.Won);
                result.Status = GuessStatus.Guessed;
            }
            else if (player.IsOutOfTries())
            {
                result.targetItem = await _itemsService.GetAsync(player.CurrentGame.TargetItemId);
                result.Status = GuessStatus.Lost;
            }
            else
            {
                result.Status = GuessStatus.NotGuessed;
                result.Hints = await GetHintsAsync(player, guess);
            }
            return result;
        });

        public Task<int> GetTriesAsync(string playerId) => WithPlayer(playerId, player => player.CurrentGame?.GuessItemIds.Count() ?? 0);

        public Task<List<Item>> GetGuessesAsync(string playerId) => WithPlayerAsync(playerId, async player =>
        {
            List<Item> guesses = new List<Item>();
            if(player.CurrentGame != null)
            {
                foreach (string itemId in player.CurrentGame.GuessItemIds)
                {
                    guesses.Add(await _itemsService.GetAsync(itemId));
                }
            }
            return guesses;
        });

        public Task<List<Hints>> GetHintsAsync(string playerId) => WithPlayerAsync(playerId, async player =>
        {
            List<Hints> hints = new List<Hints>();
            List<Item> guesses = new List<Item>();
            if (player.CurrentGame != null)
            {
                foreach (string itemId in player.CurrentGame.GuessItemIds)
                {
                    guesses.Add(await _itemsService.GetAsync(itemId));
                }
                foreach (Item guess in guesses)
                {
                    hints.Add(await GetHintsAsync(player, guess));
                }
            }
            return hints;
        });

        public Task<Hints> GetHintsAsync(string playerId, string guessId) => WithPlayerAsync(playerId, async player =>
        {
            Item item = await _itemsService.GetAsync(guessId);
            return await GetHintsAsync(player, item);
        });

        public Task<string> GetTargetItemNameAsync(string playerId) => WithPlayerAsync(playerId, async player =>
        {
            Item target = await _itemsService.GetAsync(player.CurrentGame.TargetItemId);
            return target.Name;
        });

        public Task<Gamemode?> GetActiveGamemodeAsync(string playerId) => WithPlayer<Gamemode?>(playerId, player =>
        {
            if (player.HasActiveGame())
            {
                return player.CurrentGame!.Mode;
            }
            else return null;
        });

        //private methods

        private async Task UpdatePlayerScoreAsync(Player player, GameResult result)
        {
            if (player.CurrentGame == null) return;
            if (player.CurrentGame.Mode == Gamemode.Normal)
            {
                if (result == GameResult.Won)
                {
                    player.NormalStats = player.NormalStats.AddWin(player.CurrentGame.TargetItemId, player.CurrentGame.GuessItemIds.Count);
                }
                else if(result == GameResult.Lost)
                {
                    player.NormalStats = player.NormalStats.AddLose();
                }
            }
            else if (player.CurrentGame.Mode == Gamemode.Daily)
            {
                if (result == GameResult.Won)
                {
                    player.DailyStats = player.DailyStats.AddWin(player.CurrentGame.TargetItemId, player.CurrentGame.GuessItemIds.Count);
                }
                else if(result == GameResult.Lost)
                {
                    player.DailyStats = player.DailyStats.AddLose();
                }
                player.DailyAttempted = true;
            }
            player.CurrentGame.IsEnded = true;
            await _playersService.UpdateAsync(player.Id, player);
        }

        private async Task StartNewGameAsync(Player player, string targetItemId, Gamemode mode, bool reskinsExcluded)
        {
            player.CurrentGame = new Game
            {
                StartTime = DateTime.Now,
                TargetItemId = targetItemId,
                GuessItemIds = new List<string>(),
                IsEnded = false,
                ReskingExcluded = reskinsExcluded,
                Mode = mode
            };
            await _playersService.UpdateAsync(player.Id, player);
        }

        private async Task<Hints> GetHintsAsync(Player currentPlayer, Item guess)
        {
            if(currentPlayer.CurrentGame == null)
            {
                return new Hints();
            }
            Item target = await _itemsService.GetAsync(currentPlayer.CurrentGame.TargetItemId);
            Hints hints = new Hints
            {
                Tier = GetBinaryHint(item => (item.Tier+item.Reskin)),
                Type = GetBinaryHint(item => item.Type),
                NumberOfShots = GetHint(item => item.NumberOfShots),
                XpBonus = GetHint(item => item.XpBonus),
                Feedpower = GetHint(item => item.Feedpower)
            };
            return hints;

            Hint GetHint(Func<Item, IComparable> hintProperty)
            {
                IComparable guessProperty = hintProperty(guess);
                IComparable targetProperty = hintProperty(target);
                if (targetProperty.CompareTo(guessProperty) < 0)
                {
                    return Hint.Less;
                }
                if (targetProperty.CompareTo(guessProperty) > 0)
                {
                    return Hint.Greater;
                }
                return Hint.Correct;
            }

            Hint GetBinaryHint<T>(Func<Item, T> hintProperty)
            {
                T guessProperty = hintProperty(guess);
                T targetProperty = hintProperty(target);
                return guessProperty.Equals(targetProperty) ? Hint.Correct : Hint.Wrong;
            }
        }

        //WithPlayer
        private Task WithPlayer(string id, Action<Player> action)
        {
            return WithPlayerAsync(id, player =>
            {
                action(player);
                return Task.FromResult(0);
            });
        }

        private Task<T> WithPlayer<T>(string id, Func<Player, T> func)
        {
            return WithPlayerAsync(id, player => Task.FromResult(func(player)));
        }

        private Task WithPlayerAsync(string id, Func<Player, Task> func)
        {
            return WithPlayerAsync(id, async player =>
            {
                await func(player);
                return 0;
            });
        }

        private async Task<T> WithPlayerAsync<T>(string id, Func<Player, Task<T>> func)
        {
            Player player = await _playersService.GetAsync(id);
            return await func(player);
        }

        
    }
}
