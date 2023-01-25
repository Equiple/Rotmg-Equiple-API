using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using RomgleWebApi.DAL;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Settings;

namespace RomgleWebApi.Services.Implementations
{
    public class ComplaintService : IComplaintService
    {
        private readonly IMongoCollection<Complaint> _complaintsCollection;
        private readonly IPlayerService _playersService;

        public ComplaintService(
            IOptions<RotmgleDatabaseSettings> rotmgleDatabaseSettings,
            IDataCollectionProvider dataCollectionProvider,
            IPlayerService playersService
            ) 
        {
            _complaintsCollection = dataCollectionProvider
                .GetDataCollection<Complaint>(rotmgleDatabaseSettings.Value.ComplaintCollectionName)
                .AsMongo();
            _playersService = playersService;
        }

        public async Task<Complaint> GetAsync(string fingerprint)
        {
            return await _complaintsCollection.Find(complaint => complaint.Fingerprint == fingerprint).FirstAsync();
        }

        public async Task<bool> FileComplaintAsync(string fingerprint, string email, string complaint)
        {
            if (await _complaintsCollection
                .CountDocumentsAsync(complaint => complaint.Fingerprint == fingerprint) > 1)
            {
                IMongoQueryable<Complaint> query = _complaintsCollection.AsQueryable()
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
            await _complaintsCollection.InsertOneAsync(new Complaint()
            {
                Fingerprint = fingerprint,
                Email = email,
                Body = complaint,
                Date = DateTime.UtcNow
            });
            return true;
        }

        public async Task RemoveComplaintAsync(string complaintId)
        {
            await _complaintsCollection.DeleteOneAsync(complaint => complaint.Id == complaintId);
        }

        public async Task<IReadOnlyList<Complaint>> GetComplaintsAsync(string playerId)
        {
            Player player = await _playersService.GetAsync(playerId);
            if (player.Role == "user" || await _complaintsCollection.CountDocumentsAsync(complaint => true) == 0)
            {
                return new List<Complaint>();
            }
            IMongoQueryable<Complaint> query = _complaintsCollection.AsQueryable().Where(complaint => true);
            return await query.ToListAsync();
        }
    }
}
