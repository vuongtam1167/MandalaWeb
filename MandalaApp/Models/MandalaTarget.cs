using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MandalaApp.Models
{
    public class MandalaTarget
    {
        [Key, Column(Order = 0)]
        public int MandalaLv { get; set; }

        [Key, Column(Order = 1)]
        public long MandalaID { get; set; }

        [StringLength(250)]
        public string Target { get; set; }
    }
}
