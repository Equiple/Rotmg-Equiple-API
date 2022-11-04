namespace RomgleWebApi.Services
{
    public interface IJobService
    {
        Task RemoveExpiredTokensAndGuestsAsync();
    }
}
