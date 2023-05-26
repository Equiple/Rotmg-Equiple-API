namespace RotmgleWebApi.Complaints
{
    public interface IComplaintService
    {
        Task<bool> FileComplaintAsync(string fingerprint, string email, string complaint);

        Task RemoveComplaintAsync(string complaintId);

        Task<IEnumerable<Complaint>> GetComplaintsAsync(string playerId);
    }
}
