using Hangfire;

namespace RotmgleWebApi.Jobs
{
    public static class RecurringJobInitializer
    {
        /// <summary>
        /// Initializes all recurrent jobs.
        /// </summary>
        public static void Initialize()
        {
            RecurringJob.AddOrUpdate<IJobService>("InvalidateExpiredDailyGames",
                jobService => jobService.InvalidateExpiredDailyGamesAsync(),
                Cron.Daily,
                timeZone: TimeZoneInfo.Utc);
            RecurringJob.AddOrUpdate<IJobService>("RemoveInactiveGuests",
                jobService => jobService.RemoveInactiveGuestsAsync(),
                Cron.Daily,
                timeZone: TimeZoneInfo.Utc);
        }
    }
}
