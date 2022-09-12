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

        public async Task<GuessResult> CheckGuessAsync(string guessId, string playerId, string gamemode)
        {
            Player currentPlayer = await _playersService.GetAsync(playerId);
            Item guess = await _itemsService.GetAsync(guessId);
            GuessResult result = new GuessResult();
            if (!currentPlayer.HasCurrentGame())
            {
                Item newItem;
                if (gamemode == "Daily")
                {
                    Daily dd = await _dailiesService.GetDailyItem();
                    newItem = await _itemsService.GetAsync(dd.Id);
                    currentPlayer.StartNewGame(newItem.Id, gamemode);
                }
                else
                {
                    newItem = await _itemsService.GetRandomItemAsync();
                    currentPlayer.StartNewGame(newItem.Id, gamemode);
                }
            }
            //TODO: Guest

            if (currentPlayer.CheckCurrentGame(guessId))
            {
                currentPlayer.UpdatePlayerScore(true);
                result.Status = GuessStatus.Guessed;
            }
            else if (currentPlayer.IsOutOfTries())
            {
                result.Status = GuessStatus.Lost;
            }
            else 
            {
                result.Hints = await GetHintsAsync(currentPlayer, guess);
            }
            return result;
        }

        public async Task<int> GetTriesAsync(string playerId)
        {
            Player currentPlayer = await _playersService.GetAsync(playerId);
            return currentPlayer.CurrentGame.GuessItemIds.Count();
        }

        public async Task<List<Item>> GetGuessesAsync(string playerId)
        {
            Player currentPlayer = await _playersService.GetAsync(playerId);
            List<Item> guesses = new List<Item>();
            foreach(string itemId in currentPlayer.CurrentGame.GuessItemIds)
            {
                guesses.Add(await _itemsService.GetAsync(itemId));
            }
            return guesses;
        }
        public async Task<List<Hints>> GetHintsAsync(string playerId)
        {
            Player currentPlayer = await _playersService.GetAsync(playerId);
            List<Hints> hints = new List<Hints>();
            List<Item> guesses = new List<Item>();
            foreach(string itemId in currentPlayer.CurrentGame.GuessItemIds)
            {
                guesses.Add(await _itemsService.GetAsync(itemId));
            }
            foreach(Item guess in guesses)
            {
                hints.Add(await GetHintsAsync(currentPlayer, guess));
            }
            return hints;
        }
        public async Task<Hints> GetHintsAsync(string  playerId, string guessId)
        {
            Item item = await _itemsService.GetAsync(guessId);
            Player currentPlayer = await _playersService.GetAsync(playerId);
            return await GetHintsAsync(currentPlayer, item);
        }

        public async Task<Hints> GetHintsAsync(Player currentPlayer, Item guess)
        {
            Item target = await _itemsService.GetAsync(currentPlayer.CurrentGame.TargetItemId);
            Hints hints = new Hints
            {
                Tier = GetBinaryHint(item => item.Tier),
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
                    return Hint.Smaller;
                }
                if (targetProperty.CompareTo(guessProperty) > 0)
                {
                    return Hint.Bigger;
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
        
        public async Task<string> GetTargetItemNameAsync(string playerId)
        {
            Player currentPlayer = await _playersService.GetAsync(playerId);
            Item target = await _itemsService.GetAsync(currentPlayer.CurrentGame.TargetItemId);
            return target.Name;
        }

        public async Task<bool> HasAnActiveGameAsync(string playerId)
        {
            Player currentPlayer = await _playersService.GetAsync(playerId);
            return currentPlayer.CurrentGame != null;
        }
    }
}
