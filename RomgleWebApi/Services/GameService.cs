using RomgleWebApi.Data.Extensions;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Utils;
using System.Drawing;

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

        #region public methods

        public Task<GuessResult> CheckGuessAsync(string guessId, string playerId, Gamemode mode, bool reskinsExcluded) 
            => WithPlayerAsync(playerId, async player =>
        {
            Item guess = await _itemsService.GetAsync(guessId);
            GuessResult result = new GuessResult();
            if (player.HasActiveGame())
            {
                if (player.CurrentGame!.Mode != mode)
                {
                    throw new Exception($"Exception at {nameof(CheckGuessAsync)}, {nameof(GameService)} class: " +
                        $"Gamemode [mode:{mode}] was not the same as current gamemode.\n");
                }
            }
            else
            {
                Item newItem;
                if (mode == Gamemode.Daily)
                {
                    Daily dailyItem = await _dailiesService.GetDailyItem();
                    newItem = await _itemsService.GetAsync(dailyItem.TargetItemId);
                }
                else 
                {
                    newItem = _itemsService.GetRandomItem(reskinsExcluded);
                }
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
                await UpdatePlayerScoreAsync(player, GameResult.Lost);
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

        public Task<GameOptions?> GetActiveGameOptionsAsync(string playerId) => WithPlayer<GameOptions?>(playerId, player =>
        {
            if (player.HasActiveGame())
            {
                return new GameOptions { Mode= player.CurrentGame!.Mode, ReskinsExcluded= player.CurrentGame!.ReskingExcluded };
            }
            else return null;
        });

        public Task CloseTheGameAsync(string playerId) => WithPlayer(playerId, async player =>
        {
            await UpdatePlayerScoreAsync(player, GameResult.Lost);
            await _playersService.UpdateAsync(playerId, player);
        });

        public Task<string> GetTargetItemImage(string playerId) => WithPlayerAsync(playerId, async player =>
        {
            if (player.CurrentGame.IsEnded)
            {
                string targetItemId = player.CurrentGame.TargetItemId;
                Item item = await _itemsService.GetAsync(targetItemId);
                return item.ImageURL;
            }
            else return "";
        });

        public Task<int> GetPlayerLeaderboardPlacementAsync(string playerId, Gamemode mode) => WithPlayerAsync(playerId, async player =>
        {
            List<PlayerProfile> leaderboard = new List<PlayerProfile>();
            if (mode == Gamemode.Daily)
            {
                leaderboard = await _playersService.GetDailyLeaderboardAsync();

            }
            else if (mode == Gamemode.Normal)
            {
                leaderboard = await _playersService.GetNormalLeaderboardAsync();
            }
            else
            {
                throw new Exception($"Exception at {nameof(GetPlayerLeaderboardPlacementAsync)} method, {nameof(GameService)} class: " +
                    $"Invalid {nameof(Gamemode)} value: [{mode}]\n");
            }
            return leaderboard.FindIndex(player => player.Id == playerId);
        });

        #endregion

        #region private methods
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
            }
            player.CurrentGame.IsEnded = true;
            player.CurrentGame.GameResult = result;
            player.EndedGames.Add(player.CurrentGame);
            await _playersService.UpdateAsync(player.Id, player);
        }

        private async Task StartNewGameAsync(Player player, string targetItemId, Gamemode mode, bool reskinsExcluded)
        {
            player.CurrentGame = new Game
            {
                StartDate = DateTimeUtils.UtcNowDateString,
                StartTime = DateTimeUtils.UtcNowTimeString,
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
            if (currentPlayer.CurrentGame == null)
            {
                return new Hints();
            }
            Item target = await _itemsService.GetAsync(currentPlayer.CurrentGame.TargetItemId);
            //IEnumerable<Item> items = await _itemsService.FindAllAsync("", reskinsExcluded: false);
            //double maxDistance = GetMaxDistance(items); 
            Hints hints = new Hints
            {
                Tier = GetBinaryHint(item => (item.Tier + item.Reskin)),
                Type = GetBinaryHint(item => item.Type),
                NumberOfShots = Hint.Correct,
                XpBonus = GetHint(item => item.XpBonus),
                Feedpower = GetHint(item => item.Feedpower),
                DominantColor = GetHeatHint(item => item.DominantColor),
                ColorClass = GetHeatHint(item => item.DominantColor)
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

            string GetHeatHint(Func<Item, string> hintProperty)
            {
                //Max distance is mean of all item colour distances
                const double maxDistance = 117.0; //255 * Math.Sqrt(3);
                string guessProperty = hintProperty(guess);
                string targetProperty = hintProperty(target);

                Color targetColor = Color.FromName(targetProperty);
                Color guessColor = Color.FromName(guessProperty);
                double distance = GetColorDistance(targetColor, guessColor);

                Color greenColor = Color.FromArgb(alpha: 255, red: 51, green: 153, blue: 0);
                Color redColor = Color.FromArgb(alpha: 255, red: 204, green: 0, blue: 0);
                //double distanceTwo = GetColorDistance(greenColor, redColor);

                double distancePercent = MapValue(x: distance, xLeft: 0, xRight: maxDistance*2, resLeft: 0, resRight: 1);
                if(distancePercent > 1)
                {
                    distancePercent = 1;
                }
                double[] vector = new double[] {
                    (redColor.R - greenColor.R) * distancePercent,
                    (redColor.G - greenColor.G) * distancePercent,
                    (redColor.B - greenColor.B) * distancePercent
                };

                Color result = Color.FromArgb(alpha: 255,
                    red: (int)(greenColor.R + vector[0]),
                    green: (int)(greenColor.G + vector[1]),
                    blue: (int)(greenColor.B + vector[2])
                );

                string colorHex = ColorTranslator.ToHtml(result);
                return colorHex;
            }

            double GetMaxDistance(IEnumerable<Item> items)
            {
                List<double> distances = new List<double>();
                foreach(Item item in items)
                {
                    foreach(Item anotherItem in items)
                    {
                        double distance = GetColorDistance(
                            Color.FromName(item.DominantColor!),
                            Color.FromName(anotherItem.DominantColor!)
                        );
                        if(distance != 0)
                        {
                            distances.Add(distance);
                        }
                    }
                }
                distances.Sort();
                double result = distances[distances.Count / 2];
                return result;
            }

            double GetColorDistance(Color color, Color secondColor) 
            {
                double red = Math.Pow(Convert.ToDouble(color.R) - secondColor.R, 2.0);
                double green = Math.Pow(Convert.ToDouble(color.G) - secondColor.G, 2.0);
                double blue = Math.Pow(Convert.ToDouble(color.B) - secondColor.B, 2.0);

                double distance = Math.Sqrt(blue + green + red);
                return distance;
            }

            double MapValue(double x, double xLeft, double xRight, double resLeft, double resRight)
            {
                if (xLeft == xRight)
                {
                    return resLeft;
                }
                return (x - xLeft) / (xRight - xLeft) * (resRight - resLeft) + resLeft;
            }
        }

        #region with player methods

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

        #endregion

        #endregion
    }
}
