using System.ComponentModel.DataAnnotations;

namespace MandalaApp.Models
{
    public class MandalaHome
    {

        [StringLength(50)]
        public string NameMandala { get; set; }

        public DateTime? ModifiedDate { get; set; }

        [StringLength(50)]
        public string NameUser { get; set; }

        public long MandalaID { get; set; }
    }
}
