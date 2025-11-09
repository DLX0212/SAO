

namespace SAO.Domain.Base
{
    public abstract class BaseEntity
    {
        public DateTime CreatedAt { get; set; }

        public DateTime CreatedBy { get; set; }

        public DateTime UpdatedAt { get; set; } 
        public DateTime UpdatedBy { get; set; }

        public bool IsActive { get; set; } = true;

        protected BaseEntity()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            IsActive = true;
        }
    }
}
