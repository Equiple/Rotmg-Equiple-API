using System.Drawing;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using RotmgleWebApi.Dailies;
using RotmgleWebApi.Players;
using RotmgleWebApi.Items;
using RotmgleWebApi.Complaints;

namespace RotmgleWebApi.Games
{
    public class GameService : IGameService
    {
        private readonly IItemService _itemService;
        private readonly IDailyService _dailyService;
        private readonly IPlayerService _playerService;
        private readonly IConfiguration _configuration;
        private readonly IMongoCollection<Complaint> _complaintCollection;

        public GameService(
            IOptions<RotmgleDatabaseSettings> rotmgleDatabaseSettings,
            IItemService itemService,
            IDailyService dailyService,
            IPlayerService playerService,
            IConfiguration config)
        {
            _itemService = itemService;
            _dailyService = dailyService;
            _playerService = playerService;
            _configuration = config;
            _complaintCollection = MongoUtils.GetCollection<Complaint>(
                rotmgleDatabaseSettings.Value,
                x => x.ComplaintCollectionName);
        }

        #region public methods

        public async Task<GuessResult> CheckGuessAsync(string playerId, string guessId, Gamemode mode, bool reskinsExcluded)
        {
            Player player = await _playerService.GetAsync(playerId);
            GuessResult result = new()
            {
                Guess = await _itemService.GetAsync(guessId),
            };
            Game currentGame;
            if (player.CurrentGame != null && !player.CurrentGame.IsEnded)
            {
                if (player.CurrentGame.Mode != mode)
                {
                    throw new Exception($"Exception at {nameof(CheckGuessAsync)}, {nameof(GameService)} class: " +
                        $"Gamemode [mode:{mode}] was not the same as current gamemode.\n");
                }
                currentGame = player.CurrentGame;
            }
            else
            {
                Item newItem;
                if (mode == Gamemode.Daily)
                {
                    Daily dailyItem = await _dailyService.GetAsync();
                    newItem = await _itemService.GetAsync(dailyItem.TargetItemId);
                }
                else
                {
                    newItem = await _itemService.GetRandomItemAsync(reskinsExcluded);
                }
                currentGame = CreateNewGame(newItem.Id, mode, reskinsExcluded);
                player.CurrentGame = currentGame;
            }
            //TODO: Guest
            Item target = await _itemService.GetAsync(currentGame.TargetItemId);
            currentGame.GuessItemIds.Add(guessId);
            await _playerService.UpdateAsync(player);
            result.Tries = await GetTriesAsync(playerId);
            result.Hints = await GetHintsAsync(player, result.Guess);
            if (target.Id == guessId)
            {
                await _playerService.UpdatePlayerScoreAsync(player, GameResult.Won);
                result.TargetItem = target;
                result.Status = GuessStatus.Guessed;
            }
            else if (player.IsOutOfTries())
            {
                result.TargetItem = target;
                await _playerService.UpdatePlayerScoreAsync(player, GameResult.Lost);
                result.Status = GuessStatus.Lost;
            }
            else
            {
                int hintsCount = result.Hints.CountCorrect();
                result.Status = GuessStatus.NotGuessed;
                result.Anagram = target.GenerateAnagramIfEligible(hintsCount);
                result.Description = target.GetDescriptionIfEligible(hintsCount);
            }
            return result;
        }

        public async Task<int> GetTriesAsync(string playerId)
        {
            Player player = await _playerService.GetAsync(playerId);
            return player.CurrentGame?.GuessItemIds.Count ?? 0;
        }

        public async Task<IEnumerable<Item>> GetGuessesAsync(string playerId)
        {
            return await GetGuessesInternalAsync(playerId);
        }

        public async Task<IEnumerable<Hints>> GetHintsAsync(string playerId)
        {
            return await GetHintsInternalAsync(playerId);
        }

        public async Task<Hints> GetHintsAsync(string playerId, string guessId)
        {
            Player player = await _playerService.GetAsync(playerId);
            Item item = await _itemService.GetAsync(guessId);
            Hints hints = await GetHintsAsync(player, item);
            return hints;
        }

        public async Task<Item> GetTargetItemAsync(string playerId)
        {
            Player player = await _playerService.GetAsync(playerId);
            if (player.CurrentGame == null || !player.CurrentGame.IsEnded)
            {
                throw new Exception($"Exception at {nameof(GetTargetItemAsync)} method, " +
                    $"{GetType().Name} class: Player {playerId} does not have ended game.\n");
            }
            Item target = await _itemService.GetAsync(player.CurrentGame.TargetItemId);
            return target;
        }

        public async Task<GameOptions?> GetActiveGameOptionsAsync(string playerId)
        {
            Player player = await _playerService.GetAsync(playerId);
            if (player.CurrentGame == null || player.CurrentGame.IsEnded)
            {
                return null;
            }
            Item item = await _itemService.GetAsync(player.CurrentGame.TargetItemId);
            List<Hints> allHints = await GetHintsInternalAsync(playerId);
            int hintsCount = allHints.CountCorrect();
            GameOptions gameOptions = new()
            {
                Mode = player.CurrentGame.Mode,
                Guesses = await GetGuessesInternalAsync(playerId),
                AllHints = allHints,
                Anagram = item.GenerateAnagramIfEligible(hintsCount),
                Description = item.GetDescriptionIfEligible(hintsCount),
                ReskinsExcluded = player.CurrentGame.ReskingExcluded,
            };
            return gameOptions;
        }

        public async Task CloseGameAsync(string playerId)
        {
            Player player = await _playerService.GetAsync(playerId);
            await _playerService.UpdatePlayerScoreAsync(player, GameResult.Lost);
        }

        //public async Task<string> SendReportMailAsync(string author, string complaint)
        //{
        //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //    MailAddress to = new MailAddress(_configuration.GetSection("SmtpServer")["Mail"]);
        //    MailMessage message = new MailMessage(to, to);
        //    string subject = "";
        //    if (complaint.Length < 20)
        //    {
        //        subject = complaint;
        //    }
        //    else subject = complaint.Substring(0, 19);
        //    message.Subject = $"{subject}";
        //    message.Body = $"{complaint}\n\nSent by {author}.";
        //    SmtpClient client = new SmtpClient(_configuration.GetSection("SmtpServer")["RelayAdress"], 
        //        _configuration.GetSection("SmtpServer")["TlsPort"].ParseInt() ?? 0)
        //    {
        //        UseDefaultCredentials = false,
        //        Credentials = new NetworkCredential(_configuration.GetSection("SmtpServer")["Mail"], 
        //            _configuration.GetSection("SmtpServer")["Pass"]),
        //        TargetName = $"STARTTLS/{_configuration.GetSection("SmtpServer")["RelayAdress"]}",
        //        EnableSsl = true
        //    };
        //    try
        //    {
        //        client.Send(message);
        //    }
        //    catch (SmtpException ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //    }
        //    return "success";
        //}

        #endregion

        #region private methods

        private static Game CreateNewGame(string targetItemId, Gamemode mode, bool reskinsExcluded)
        {
            Game game = new()
            {
                StartDate = DateTime.UtcNow,
                TargetItemId = targetItemId,
                GuessItemIds = new List<string>(),
                IsEnded = false,
                ReskingExcluded = reskinsExcluded,
                Mode = mode,
            };
            return game;
        }

        private async Task<List<Item>> GetGuessesInternalAsync(string playerId)
        {
            Player player = await _playerService.GetAsync(playerId);
            List<Item> guesses = new();
            if (player.CurrentGame != null)
            {
                foreach (string itemId in player.CurrentGame.GuessItemIds)
                {
                    Item item = await _itemService.GetAsync(itemId);
                    guesses.Add(item);
                }
            }
            return guesses;
        }

        private async Task<List<Hints>> GetHintsInternalAsync(string playerId)
        {
            Player player = await _playerService.GetAsync(playerId);
            List<Hints> hints = new();
            List<Item> guesses = new();
            if (player.CurrentGame != null)
            {
                foreach (string itemId in player.CurrentGame.GuessItemIds)
                {
                    guesses.Add(await _itemService.GetAsync(itemId));
                }
                foreach (Item guess in guesses)
                {
                    hints.Add(await GetHintsAsync(player, guess));
                }
            }
            return hints;
        }

        private async Task<Hints> GetHintsAsync(Player currentPlayer, Item guess)
        {
            if (currentPlayer.CurrentGame == null)
            {
                return new Hints();
            }
            Item target = await _itemService.GetAsync(currentPlayer.CurrentGame.TargetItemId);
            //IEnumerable<Item> items = await _itemsService.FindAllAsync("", reskinsExcluded: false);
            //double maxDistance = GetMaxDistance(items); 
            Hints hints = new()
            {
                Tier = GetBinaryHint(item => item.Tier + item.Reskin),
                Type = GetBinaryHint(item => item.Type),
                XpBonus = GetHint(item => item.XpBonus),
                Feedpower = GetHint(item => item.Feedpower),
                DominantColor = GetColorHint(item => item.DominantColor),
                ColorClass = GetClassBasedColorHint(item => item.ColorClass)
            };
            return hints;

            Hint GetHint(Func<Item, IComparable> hintProperty)
            {
                IComparable guessProperty = hintProperty.Invoke(guess);
                IComparable targetProperty = hintProperty.Invoke(target);
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
                T guessProperty = hintProperty.Invoke(guess);
                T targetProperty = hintProperty.Invoke(target);
                return Equals(guessProperty, targetProperty) ? Hint.Correct : Hint.Wrong;
            }

            string GetColorHint(Func<Item, string> hintProperty)
            {
                string guessProperty = hintProperty.Invoke(guess);
                string targetProperty = hintProperty.Invoke(target);
                Color targetColor = Color.FromName(targetProperty);
                Color guessColor = Color.FromName(guessProperty);
                Color result = GetColorHintLAB(targetColor, guessColor);
                return ColorTranslator.ToHtml(result);
            }

            Color GetColorHintOld(Color targetColor, Color guessColor)
            {
                //Max distance is mean of all item colour distances
                const double maxDistance = 117.0;
                Color greenColor = ColorUtils.defaultGreen;
                Color redColor = ColorUtils.defaultRed;
                double distance = targetColor.GetRGBDistanceFrom(guessColor);
                double distancePercent = Utils.MapValue(
                    value: distance,
                    fromLow: 0,
                    fromHigh: maxDistance * 2,
                    toLow: 0,
                    toHigh: 1);
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
                return result;
            }

            Color GetColorHintLAB(Color targetColor, Color guessColor)
            {
                double distance = targetColor.GetDistanceFrom(guessColor);
                double defaultDistance = ColorUtils.GetDefaultGreenRedCIELabDistance();
                double distancePercent = Utils.MapValue(
                    value: distance,
                    fromLow: 0,
                    fromHigh: defaultDistance,
                    toLow: 0,
                    toHigh: 1);
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
                return result;
            }

            Color GetColorHintGradient(Color targetColor, Color guessColor)
            {
                double distance = targetColor.GetDistanceFrom(guessColor);
                double defaultDistance = ColorUtils.GetDefaultGreenRedCIELabDistance();
                int distancePercent = (int)Math.Round(Utils.MapValue(
                    value: distance,
                    fromLow: 0,
                    fromHigh: defaultDistance,
                    toLow: 0,
                    toHigh: ColorUtils.differenceGradientLAB.Count));
                Color result = ColorUtils.differenceGradientLAB[distancePercent];
                return result;
            }

            string GetClassBasedColorHint(Func<Item, string> hintProperty)
            {
                string guessProperty = hintProperty(guess);
                string targetProperty = hintProperty(target);
                Color targetColor = Color.FromName(targetProperty);
                Color guessColor = Color.FromName(guessProperty);
                int targetIndex = ColorUtils.rainbowSmall.IndexOf(targetColor);
                int guessIndex = ColorUtils.rainbowSmall.IndexOf(guessColor);
                int distance = Math.Abs(targetIndex - guessIndex);
                int newIndex = (int)Math.Round(Utils.MapValue(
                    value: distance,
                    fromLow: 0,
                    fromHigh: ColorUtils.rainbowSmall.Count,
                    toLow: 0,
                    toHigh: ColorUtils.differenceGradientRGBsmall.Count));
                Color result = ColorUtils.differenceGradientRGBsmall[newIndex];
                return ColorTranslator.ToHtml(result);
            }

            double GetMaxDistance(IEnumerable<Item> items)
            {
                List<double> distances = new();
                foreach (Item item in items)
                {
                    foreach (Item anotherItem in items)
                    {
                        double distance = Color.FromName(item.DominantColor)
                            .GetRGBDistanceFrom(Color.FromName(anotherItem.DominantColor));
                        if (distance != 0)
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
