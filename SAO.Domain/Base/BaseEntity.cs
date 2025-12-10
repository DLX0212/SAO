using System;

namespace SAO.Domain.Base
{
    public abstract class BaseEntity
    {
        public DateTime CreatedAt { get; set; }


        public string? CreatedBy { get; set; }

        public DateTime UpdatedAt { get; set; }


        public string? UpdatedBy { get; set; }

        public bool IsActive { get; set; } = true;

        protected BaseEntity()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            IsActive = true;
            CreatedBy = "System";
            UpdatedBy = "System";
        }
    }
}
