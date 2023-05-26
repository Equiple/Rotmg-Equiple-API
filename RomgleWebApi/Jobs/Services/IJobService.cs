namespace RotmgleWebApi.Jobs
{
    public interface IJobService
    {
        Task InvalidateExpiredDailyGamesAsync();

        Task RemoveInactiveGuestsAsync();
    }
}
