using System;
using System.ComponentModel.DataAnnotations;

namespace MandalaApp.Models
{
    public class MandalaDetail
    {
        [Key]
        public long ID { get; set; }

        // Added property for Mandala Level
        public int MandalaLv { get; set; }

        // Added property for Mandala ID to link this detail to a Mandala record
        public long MandalaID { get; set; }

        public string Target { get; set; }

        public DateTime Deadline { get; set; }

        public bool Status { get; set; } = true;

        [StringLength(250)]
        public string Action { get; set; }

        [StringLength(250)]
        public string Result { get; set; }

        [StringLength(50)]
        public string Person { get; set; }
    }
}
