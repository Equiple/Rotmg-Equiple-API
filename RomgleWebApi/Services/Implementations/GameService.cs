using RomgleWebApi.ColorConvertion;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Extensions;
using RomgleWebApi.Utils;
using System.Drawing;
using System.Net.Mail;
using System.Net;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Microsoft.Extensions.Options;
using RomgleWebApi.DAL;
using RomgleWebApi.Data.Settings;

namespace RomgleWebApi.Services.Implementations
{
    public class GameService : IGameService
    {
        private readonly IItemService _itemsService;
        private readonly IDailyService _dailiesService;
        private readonly IPlayerService _playersService;
        private readonly IConfiguration _configuration;
        private readonly IMongoCollection<Complaint> _complaintsCollection;

        public GameService(
            IOptions<RotmgleDatabaseSettings> rotmgleDatabaseSettings,
            IDataCollectionProvider dataCollectionProvider,
            IItemService itemsService,
            IDailyService dailiesService,
            IPlayerService playersService,
            IConfiguration config)
        {
            _itemsService = itemsService;
            _dailiesService = dailiesService;
            _playersService = playersService;
            _configuration = config;
            _complaintsCollection = dataCollectionProvider
                .GetDataCollection<Complaint>(rotmgleDatabaseSettings.Value.ComplaintCollectionName)
                .AsMongo();
        }

        #region public methods

        public async Task<GuessResult> CheckGuessAsync(string playerId, string guessId, Gamemode mode, bool reskinsExcluded)
        {
            Player player = await _playersService.GetAsync(playerId);
            GuessResult result = new GuessResult();
            result.Guess = await _itemsService.GetAsync(guessId);
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
            Item target = await _itemsService.GetAsync(player.CurrentGame!.TargetItemId);
            player.CurrentGame!.GuessItemIds.Add(guessId);
            await _playersService.UpdateAsync(player);
            result.Tries = await GetTriesAsync(playerId);
            result.Hints = await GetHintsAsync(player, result.Guess);
            if (target.Id == guessId)
            {
                await _playersService.UpdatePlayerScoreAsync(player, GameResult.Won);
                result.TargetItem = target;
                result.Status = GuessStatus.Guessed;
            }
            else if (player.IsOutOfTries())
            {
                result.TargetItem = target;
                await _playersService.UpdatePlayerScoreAsync(player, GameResult.Lost);
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
            Item item = await _itemsService.GetAsync(player.CurrentGame!.TargetItemId);
            IReadOnlyList<Hints> allHints = await GetHintsAsync(playerId);
            int hintsCount = allHints.CountCorrect();
            return new GameOptions
            {
                Mode = player.CurrentGame!.Mode,
                Guesses = await GetGuessesAsync(playerId),
                AllHints = allHints,
                Anagram = item.GenerateAnagramIfEligible(hintsCount),
                Description = item.GetDescriptionIfEligible(hintsCount),
                ReskinsExcluded = player.CurrentGame!.ReskingExcluded
            };
        }

        public async Task CloseGameAsync(string playerId)
        {
            Player player = await _playersService.GetAsync(playerId);
            await _playersService.UpdatePlayerScoreAsync(player, GameResult.Lost);
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
                XpBonus = GetHint(item => item.XpBonus),
                Feedpower = GetHint(item => item.Feedpower),
                DominantColor = GetColorHint(item => item.DominantColor),
                ColorClass = GetClassBasedColorHint(item => item.ColorClass)
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

            string GetColorHint(Func<Item, string> hintProperty)
            {
                string guessProperty = hintProperty(guess);
                string targetProperty = hintProperty(target);
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
                double distancePercent = CommonUtils
                    .MapValue(value: distance, fromLow: 0, fromHigh: maxDistance*2, toLow: 0, toHigh: 1);
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
                double distancePercent = CommonUtils
                    .MapValue(value: distance, fromLow: 0, fromHigh: defaultDistance, toLow: 0, toHigh: 1);
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
                int distancePercent = (int) Math.Round(CommonUtils
                    .MapValue(value: distance, fromLow: 0, fromHigh: defaultDistance, 
                        toLow: 0, toHigh: ColorUtils.differenceGradientLAB.Count));
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
                int newIndex = (int) Math.Round(CommonUtils
                    .MapValue(value: distance, fromLow: 0, fromHigh: ColorUtils.rainbowSmall.Count, 
                        toLow: 0, toHigh: ColorUtils.differenceGradientRGBsmall.Count));
                Color result = ColorUtils.differenceGradientRGBsmall[newIndex];
                return ColorTranslator.ToHtml(result);
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
