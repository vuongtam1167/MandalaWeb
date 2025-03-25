using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MandalaApp.Models
{
    public class Mandala
    {
        [Key]
        public long ID { get; set; }

        [StringLength(50)]
        public string Name { get; set; }
        public int Class { get; set; }

        public DateTime? CreatedDate { get; set; }  

        public long CreatedUserID { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public long ModifiedUserID { get; set; }

        public bool Status { get; set; } = true;
    }
}
