﻿namespace RotmgleWebApi.Dailies
{
    public interface IDailyService
    {
        Task<Daily> GetAsync();

        Task<int> CountDailiesAsync();
    }
}
