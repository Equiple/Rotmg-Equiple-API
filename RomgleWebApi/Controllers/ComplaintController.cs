using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RotmgleWebApi.Complaints;
using RotmgleWebApi.ModelBinding;

namespace RotmgleWebApi.Controllers
{
    [ApiController]
    [Route("/complaints")]
    public class ComplaintController : ControllerBase
    {
        private readonly IComplaintService _complaintService;
        private readonly ILogger<ComplaintController> _logger;

        public ComplaintController(
            IComplaintService complaintService,
            ILogger<ComplaintController> logger)
        {
            _complaintService = complaintService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("FileComplaint")]
        public async Task<bool> FileComplaint(string fingerprint, string email, string complaint)
        {
            return await _complaintService.FileComplaintAsync(fingerprint, email, complaint);
        }

        [Authorize]
        [HttpPost("RemoveComplaint")]
        public async Task RemoveComplaint(string complaintId)
        {
            await _complaintService.RemoveComplaintAsync(complaintId);
        }

        [Authorize]
        [HttpGet("GetComplaints")]
        public async Task<IEnumerable<Complaint>> GetComplaints([UserId] string playerId)
        {
            return await _complaintService.GetComplaintsAsync(playerId);
        }
    }
}
