using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Models.Entities
{
    public class ServiceResponse<T>
    {
        public ServiceResponse()
        {
            Status = true;
            StatusMessage = "Succcessful";
        }
        public bool Status { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
        public T? Data { get; set; }

    }
    
}
