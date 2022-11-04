using Hangfire;
using Hangfire.Common;
using RomgleWebApi.Services;

namespace RomgleWebApi.Utils
{
    public static class RecurringJobInitializer
    {
        #region public methods

        /// <summary>
        /// Initializes all recurrent jobs.
        /// </summary>
        public static void Initialize()
        {
            RecurringJob.AddOrUpdate<IPlayerService>(recurringJobId: "InvalidateExpiredDailyGames", 
                playerService => playerService.InvalidateExpiredDailyGamesAsync(),
                Cron.Daily,
                timeZone: TimeZoneInfo.Utc);
            RecurringJob.AddOrUpdate<IJobService>(recurringJobId: "RemoveExpiredTokensAndGuests",
                jobService => jobService.RemoveExpiredTokensAndGuestsAsync(),
                Cron.Weekly,
                timeZone: TimeZoneInfo.Utc);
        }

        #endregion public methods
    }
}
