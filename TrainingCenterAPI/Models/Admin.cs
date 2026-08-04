using TrainingCenterAPI.Enums;

namespace TrainingCenterAPI.Models
{
    public class Admin
    {
        public int AdminId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PersonId { get; set; }
        public virtual Person Person { get; set; } = null!;
    }
}