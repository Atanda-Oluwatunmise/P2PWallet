using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using P2PWallet.Services.Interface;

namespace P2PWallet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KycProcessController:ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IKycProcessService _kycProcessService;
        public KycProcessController(DataContext dataContext, IKycProcessService kycProcessService) 
        {
            _dataContext = dataContext;
            _kycProcessService = kycProcessService;
        }

        [HttpPost("newkyclist"), Authorize]
        public async Task<ServiceResponse<string>> AddToKycDocumentList(KycDocumentsView kycDocumentsdto)
        {
            var result = await _kycProcessService.AddToKycDocumentList(kycDocumentsdto);
            return result;
        }

        [HttpGet("kycdocumentlist"), Authorize]
        public async Task<ServiceResponse<List<KycDocumentsView>>> GetDocumentsRequired()
        {
            var result = await _kycProcessService.GetDocumentsRequired();
            return result;
        } 
        
        [HttpGet("unuploadeddocs"), Authorize]
        public async Task<ServiceResponse<List<KycDocumentsView>>> ListsofUnUploadedandRejectedDocuments()
        {
            var result = await _kycProcessService.ListsofUnUploadedandRejectedDocuments();
            return result;
        }

        [HttpPost("uploadkycdocument"), Authorize]
        public async Task<ServiceResponse<string>> UploadDocuments ([FromForm] KycDto kycDto)
        {
            var result = await _kycProcessService.UploadDocuments(kycDto);
            return result;
        }

        [HttpGet("userspending"), Authorize]
        public async Task<ServiceResponse<List<PendingUsersView>>> GetListOfPendingUsers()
        {
            var result = await _kycProcessService.GetListOfPendingUsers();
            return result;
        }

        [HttpPost("getuserkycdetail"), Authorize]
        public async Task<ServiceResponse<List<KycUserDetails>>> GetKycDetailsForUser(NewKycDocumentsView kycDocumentsView)
        {
            var result = await _kycProcessService.GetKycDetailsForUser(kycDocumentsView);
            return result;
        }

        [HttpPost("approvedocument"), Authorize]
        public async Task<ServiceResponse<string>> ApproveDocument(KycDocumentsView kycDocumentsView)
        {
            var result = await _kycProcessService.ApproveDocument(kycDocumentsView);
            return result;
        }

        [HttpPost("rejectdocument"), Authorize] 
        public async Task<ServiceResponse<string>> RejectDocument(RejectDocsDto rejectDocsDto)
        {
            var result = await _kycProcessService.RejectDocument(rejectDocsDto);
            return result;
        }

        [HttpGet("upgradeuseraccount"), Authorize]
        public async Task<ServiceResponse<string>> UpgradeUserAccount()
        {
            var result = await _kycProcessService.UpgradeUserAccount();
            return result;
        }



    }
}
