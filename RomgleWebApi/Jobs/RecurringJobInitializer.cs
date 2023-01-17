using Hangfire;
using RomgleWebApi.Services;

namespace RomgleWebApi.Jobs
{
    public static class RecurringJobInitializer
    {
        #region public methods

        /// <summary>
        /// Initializes all recurrent jobs.
        /// </summary>
        public static void Initialize()
        {
            RecurringJob.AddOrUpdate<IJobService>("InvalidateExpiredDailyGames",
                jobService => jobService.InvalidateExpiredDailyGamesAsync(),
                Cron.Daily,
                timeZone: TimeZoneInfo.Utc);
            RecurringJob.AddOrUpdate<IJobService>("RemoveExpiredTokensAndGuests",
                jobService => jobService.RemoveExpiredTokensAndGuestsAsync(),
                Cron.Weekly,
                timeZone: TimeZoneInfo.Utc);
        }

        #endregion public methods
    }
}
