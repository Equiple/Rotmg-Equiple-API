using RomgleWebApi.ColorConvertion;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Extensions;
using RomgleWebApi.Utils;
using System.Drawing;

namespace RomgleWebApi.Services.Implementations
{
    public class GameService : IGameService
    {
        private readonly IItemService _itemsService;
        private readonly IDailyService _dailiesService;
        private readonly IPlayerService _playersService;

        public GameService(
            IItemService itemsService,
            IDailyService dailiesService,
            IPlayerService playersService)
        {
            _itemsService = itemsService;
            _dailiesService = dailiesService;
            _playersService = playersService;
        }

        #region public methods

        public async Task<GuessResult> CheckGuessAsync(string playerId, string guessId, Gamemode mode, bool reskinsExcluded)
        {
            Player player = await _playersService.GetAsync(playerId);
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
                    Daily dailyItem = await _dailiesService.GetAsync();
                    newItem = await _itemsService.GetAsync(dailyItem.TargetItemId);
                }
                else
                {
                    newItem = await _itemsService.GetRandomItemAsync(reskinsExcluded);
                }
                await StartNewGameAsync(player, newItem.Id, mode, reskinsExcluded);
            }
            //TODO: Guest

            player.CurrentGame!.GuessItemIds.Add(guessId);
            await _playersService.UpdateAsync(player);

            if (player.CurrentGame.TargetItemId == guessId)
            {
                await _playersService.UpdatePlayerScoreAsync(player, GameResult.Won);
                result.Status = GuessStatus.Guessed;
            }
            else if (player.IsOutOfTries())
            {
                result.TargetItem = await _itemsService.GetAsync(player.CurrentGame.TargetItemId);
                await _playersService.UpdatePlayerScoreAsync(player, GameResult.Lost);
                result.Status = GuessStatus.Lost;
            }
            else
            {
                result.Status = GuessStatus.NotGuessed;
                result.Hints = await GetHintsAsync(player, guess);
            }
            return result;
        }

        public async Task<int> GetTriesAsync(string playerId)
        {
            Player player = await _playersService.GetAsync(playerId);
            return player.CurrentGame?.GuessItemIds.Count ?? 0;
        }

        public async Task<IReadOnlyList<Item>> GetGuessesAsync(string playerId)
        {
            Player player = await _playersService.GetAsync(playerId);
            List<Item> guesses = new List<Item>();
            if (player.CurrentGame != null)
            {
                foreach (string itemId in player.CurrentGame.GuessItemIds)
                {
                    Item item = await _itemsService.GetAsync(itemId);
                    guesses.Add(item);
                }
            }
            return guesses;
        }

        public async Task<IReadOnlyList<Hints>> GetHintsAsync(string playerId)
        {
            Player player = await _playersService.GetAsync(playerId);
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
        }

        public async Task<Hints> GetHintsAsync(string playerId, string guessId)
        {
            Player player = await _playersService.GetAsync(playerId);
            Item item = await _itemsService.GetAsync(guessId);
            return await GetHintsAsync(player, item);
        }

        public async Task<Item> GetTargetItemAsync(string playerId)
        {
            Player player = await _playersService.GetAsync(playerId);
            if (player.CurrentGame == null || !player.CurrentGame.IsEnded)
            {
                throw new Exception($"Exception at {nameof(GetTargetItemAsync)} method, " +
                    $"{GetType().Name} class: Player {playerId} does not have ended game.\n");
            }
            Item target = await _itemsService.GetAsync(player.CurrentGame.TargetItemId);
            return target;
        }

        public async Task<GameOptions?> GetActiveGameOptionsAsync(string playerId)
        {
            Player player = await _playersService.GetAsync(playerId);
            if (!player.HasActiveGame())
            {
                return null;
            }
            return new GameOptions
            {
                Mode = player.CurrentGame!.Mode,
                ReskinsExcluded = player.CurrentGame!.ReskingExcluded
            };
        }

        public async Task CloseGameAsync(string playerId)
        {
            Player player = await _playersService.GetAsync(playerId);
            await _playersService.UpdatePlayerScoreAsync(player, GameResult.Lost);
            await _playersService.UpdateAsync(player);
        }

        #endregion

        #region private methods

        private async Task StartNewGameAsync(Player player, string targetItemId, Gamemode mode, bool reskinsExcluded)
        {
            player.CurrentGame = new Game
            {
                StartDate = DateTime.UtcNow,
                TargetItemId = targetItemId,
                GuessItemIds = new List<string>(),
                IsEnded = false,
                ReskingExcluded = reskinsExcluded,
                Mode = mode
            };
            await _playersService.UpdateAsync(player);
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
                Tier = GetBinaryHint(item => item.Tier + item.Reskin),
                Type = GetBinaryHint(item => item.Type),
                NumberOfShots = Hint.Correct,
                XpBonus = GetHint(item => item.XpBonus),
                Feedpower = GetHint(item => item.Feedpower),
                DominantColor = GetColorHint(item => item.DominantColor),
                ColorClass = GetColorHint(item => item.DominantColor)
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

            string GetOldColorHint(Func<Item, string> hintProperty)
            {
                //Max distance is mean of all item colour distances
                const double maxDistance = 117.0;
                string guessProperty = hintProperty(guess);
                string targetProperty = hintProperty(target);
                Color targetColor = Color.FromName(targetProperty);
                Color guessColor = Color.FromName(guessProperty);
                Color greenColor = ColorUtils.defaultGreen;
                Color redColor = ColorUtils.defaultRed;
                double distance = targetColor.GetRGBDistanceFrom(guessColor);
                double distancePercent = CommonUtils
                    .MapValue(x: distance, xLeft: 0, xRight: maxDistance*2, resLeft: 0, resRight: 1);
                if (distancePercent > 1)
                {
                    distancePercent = 1;
                }
                double[] vector = new double[] {
                    (redColor.R - greenColor.R) * distancePercent,
                    (redColor.G - greenColor.G) * distancePercent,
                    (redColor.B - greenColor.B) * distancePercent
                };
                Color result = Color.FromArgb(
                    alpha: 255,
                    red: (int)(greenColor.R + vector[0]),
                    green: (int)(greenColor.G + vector[1]),
                    blue: (int)(greenColor.B + vector[2]));
                string colorHex = ColorTranslator.ToHtml(result);
                return colorHex;
            }

            string GetColorHint(Func<Item, string> hintProperty)
            {
                string guessProperty = hintProperty(guess);
                string targetProperty = hintProperty(target);
                Color targetColor = Color.FromName(targetProperty);
                Color guessColor = Color.FromName(guessProperty);
                double distance = targetColor.GetDistanceFrom(guessColor);
                double defaultDistance = ColorUtils.GetDefaultGreenRedCIELabDistance();
                double distancePercent = CommonUtils
                    .MapValue(x: distance, xLeft: 0, xRight: defaultDistance, resLeft: 0, resRight: 1);
                if (distancePercent > 1)
                {
                    distancePercent = 1;
                }
                Color greenColor = ColorUtils.defaultGreen;
                Color redColor = ColorUtils.defaultRed;
                double[] vector = new double[] {
                    (redColor.R - greenColor.R) * distancePercent,
                    (redColor.G - greenColor.G) * distancePercent,
                    (redColor.B - greenColor.B) * distancePercent
                };
                Color result = Color.FromArgb(
                    alpha: 255,
                    red: (int)(greenColor.R + vector[0]),
                    green: (int)(greenColor.G + vector[1]),
                    blue: (int)(greenColor.B + vector[2]));
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
                        double distance = Color.FromName(item.DominantColor!)
                            .GetRGBDistanceFrom(Color.FromName(anotherItem.DominantColor!));
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
        }

        #endregion
    }
}
