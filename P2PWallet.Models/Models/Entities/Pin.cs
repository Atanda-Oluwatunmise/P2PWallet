using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Models.Entities
{
    public class Pin
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public int PinId { get; set; }
        public byte[] UserPin { get; set; } = new byte[32];
        public byte[] PinKey { get; set; } = new byte[32];

        public virtual User User { get; set; }
    }
}
