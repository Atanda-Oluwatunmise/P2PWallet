using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.DataObjects.WebHook;
using P2PWallet.Models.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Interface
{
    public interface IPaymentService
    {
        public Task<ServiceResponse<PaystackRequestView>> InitializePayment(DepositDto deposit);
        public Task<ServiceResponse<WebhookDto>> PayStackWebHook(WebhookDto webhook);

    }
}
