using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Models.Entities
{
    public class KycDocument
    {
        [Key]
        public int Id { get; set; }
        public string DocumentName { get; set; }

        public ICollection <KycDocumentUpload> UserKycDocumentUploaded { get; set; }
    }
}
