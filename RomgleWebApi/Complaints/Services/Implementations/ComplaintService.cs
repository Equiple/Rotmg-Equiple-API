using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using RotmgleWebApi.Players;

namespace RotmgleWebApi.Complaints
{
    public class ComplaintService : IComplaintService
    {
        private readonly IMongoCollection<Complaint> _complaintCollection;
        private readonly IPlayerService _playerService;

        public ComplaintService(
            IOptions<RotmgleDatabaseSettings> rotmgleDatabaseSettings,
            IPlayerService playerService)
        {
            _complaintCollection = MongoUtils.GetCollection<Complaint>(
                rotmgleDatabaseSettings.Value,
                x => x.ComplaintCollectionName);
            _playerService = playerService;
        }

        public async Task<Complaint> GetAsync(string fingerprint)
        {
            Complaint complaint = await _complaintCollection
                .Find(complaint => complaint.Fingerprint == fingerprint)
                .FirstAsync();
            return complaint;
        }

        public async Task<bool> FileComplaintAsync(string fingerprint, string email, string complaint)
        {
            if (await _complaintCollection
                .CountDocumentsAsync(complaint => complaint.Fingerprint == fingerprint) > 1)
            {
                IMongoQueryable<Complaint> query = _complaintCollection.AsQueryable()
                    .Where(complaint => complaint.Fingerprint == fingerprint)
                    .OrderByDescending(complaint => complaint.Id);
                Complaint lastComplaint = query.First();
                TimeSpan fiveHoursSpan = lastComplaint.Date.Time + TimeSpan.FromHours(5);
                TimeSpan current = DateTime.UtcNow.TimeOfDay;
                if (lastComplaint.Date.Date == DateTime.UtcNow.Date && current <= fiveHoursSpan)
                {
                    return false;
                }
            }
            await _complaintCollection.InsertOneAsync(new Complaint()
            {
                Fingerprint = fingerprint,
                Email = email,
                Body = complaint,
                Date = DateTime.UtcNow,
            });
            return true;
        }

        public async Task RemoveComplaintAsync(string complaintId)
        {
            await _complaintCollection.DeleteOneAsync(complaint => complaint.Id == complaintId);
        }

        public async Task<IEnumerable<Complaint>> GetComplaintsAsync(string playerId)
        {
            Player player = await _playerService.GetAsync(playerId);
            if (player.Role == "user" || await _complaintCollection.CountDocumentsAsync(complaint => true) == 0)
            {
                return Enumerable.Empty<Complaint>();
            }
            IMongoQueryable<Complaint> query = _complaintCollection.AsQueryable().Where(complaint => true);
            return await query.ToListAsync();
        }
    }
}
