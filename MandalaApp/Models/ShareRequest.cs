namespace MandalaApp.Models
{
    public class ShareRequest
    {
        public long MandalaId { get; set; }
        public long SharedUserId { get; set; }
        public string Permission { get; set; } = "Hạn chế";
    }
}
