using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Models.Entities
{
    public class ImageDetail
    {
        public int Id { get; set; }
        [ForeignKey("User")]
        public int ImageUserId { get; set; }
        public string ImageName { get; set; }
        public byte[] Image { get; set; }

        public virtual User UserImage { get; set; }
    }
}
