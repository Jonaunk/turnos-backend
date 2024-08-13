using Domain.Common.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Domain.Common
{
    public abstract class AuditableBaseEntity : BaseEntity, IAuditableBaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string? CreatedBy { get; set; }
        [Required]
        public DateTime Created { get; set; }
        [MaxLength(50)]
        public string? ModifiedBy { get; set; }
        public DateTime? Modified { get; set; }
        [MaxLength(50)]
        public string? DeleteBy { get; set; }
        public DateTime? Deleted { get; set; }
    }
}