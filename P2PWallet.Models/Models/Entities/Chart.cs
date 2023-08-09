using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Models.Entities
{
    public class Chart
    {
        public List<int> Data { get; set; }
        public string? Label { get; set; }
        public string? BackgroundColor { get; set; }
        public Chart()
        {
            Data = new List<int>();
        }
    }
}
