using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Models.Entities
{
    public class KycDocumentUpload
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("KycDocument")]
        public int DocumentId { get; set; }
        //[ForeignKey("User")]
        public int UserId { get; set; }
        public string FileName { get; set; }
        public int Status { get; set; }

        public virtual KycDocument UserkycDocumentList { get; set; }
        //public virtual User KycUsers { get; set; }
    }
}
