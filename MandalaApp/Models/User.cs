using System.ComponentModel.DataAnnotations;

namespace MandalaApp.Models {
public class User
{
    [Key]
    public long ID { get; set; }

    [StringLength(50)]
    public string UserName { get; set; }

    [StringLength(32)]
    public string Password { get; set; }

    [StringLength(50)]
    public string Name { get; set; }

    [StringLength(50)]
    public string Email { get; set; }

    public string Avatar { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool Status { get; set; } = true;
}
}
