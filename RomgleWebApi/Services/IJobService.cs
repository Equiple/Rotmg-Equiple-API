namespace RomgleWebApi.Services
{
    public interface IJobService
    {
        Task InvalidateExpiredDailyGamesAsync();

        Task RemoveExpiredTokensAndGuestsAsync();
    }
}
