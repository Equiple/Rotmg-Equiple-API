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

        public async Task FileComplaintAsync(string author, string complaint)
        {
            await _complaintsCollection.InsertOneAsync(new Complaint()
            {
                Email = author,
                Body = complaint,
                Date = DateTime.UtcNow
            });
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
