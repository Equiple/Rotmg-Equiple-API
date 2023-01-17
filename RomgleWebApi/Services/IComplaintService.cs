using RomgleWebApi.Data.Models;

namespace RomgleWebApi.Services
{
    public interface IComplaintService
    {
        Task FileComplaintAsync(string author, string complaint);

        Task RemoveComplaintAsync(string complaintId);

        Task<IReadOnlyList<Complaint>> GetComplaintsAsync(string playerId);
    }
}
