using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomgleWebApi.Data.Models;
using RomgleWebApi.ModelBinding.Attributes;
using RomgleWebApi.Services;
using RomgleWebApi.Services.Implementations;

namespace RomgleWebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/complaints")]
    public class ComplaintController : ControllerBase
    {
        private readonly ILogger<ComplaintController> _logger;
        private readonly IComplaintService _complaintService;

        public ComplaintController(ILogger<ComplaintController> logger,
            IComplaintService complaintService)
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

        [AllowAnonymous]
        [HttpPost("RemoveComplaint")]
        public async Task RemoveComplaint(string complaintId)
        {
            await _complaintService.RemoveComplaintAsync(complaintId);
        }

        [HttpGet("GetComplaints")]
        public async Task<IReadOnlyList<Complaint>> GetComplaints([UserId] string playerId)
        {
            return await _complaintService.GetComplaintsAsync(playerId);
        }
    }
}
