using RomgleWebApi.Data.Models;

namespace RomgleWebApi.Services
{
    public interface IComplaintService
    {
        Task<bool> FileComplaintAsync(string fingerprint, string email, string complaint);

        Task RemoveComplaintAsync(string complaintId);

        Task<IReadOnlyList<Complaint>> GetComplaintsAsync(string playerId);
    }
}
