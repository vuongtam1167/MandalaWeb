namespace MandalaApp.Models
{
    public class SaveMandalaRequest
    {
        public long MandalaId { get; set; }
        public List<MandalaDetail> Details { get; set; }
        public List<long> DeletedIds { get; set; }
    }
}
