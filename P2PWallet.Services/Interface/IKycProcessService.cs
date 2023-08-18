using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Interface
{
    public interface IKycProcessService
    {
        Task<ServiceResponse<string>> AddToKycDocumentList(KycDocumentsView kycDocumentsdto);
        Task<ServiceResponse<List<KycDocumentsView>>> GetDocumentsRequired();
        Task<ServiceResponse<string>> UploadDocuments(KycDto kycDto);
        Task<ServiceResponse<List<PendingUsersView>>> GetListOfPendingUsers();
        Task<ServiceResponse<List<KycUserDetails>>> GetKycDetailsForUser(NewKycDocumentsView kycDocumentsView);
        Task<ServiceResponse<string>> ApproveDocument(KycDocumentsView kycDocumentsView);
        Task<ServiceResponse<string>> RejectDocument(RejectDocsDto rejectDocsDto);
        Task<ServiceResponse<string>> UpgradeUserAccount();
        Task<ServiceResponse<List<KycDocumentsView>>> ListsofUnUploadedandRejectedDocuments();


    }
}
