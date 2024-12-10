namespace Domain.Common.Interfaces
{
    public interface IAuditableBaseEntity
    {
        string? CreatedBy { get; set; }
        DateTime CreatedDate { get; set; }
        string? ModifiedBy { get; set; }
        DateTime? ModifiedDate { get; set; }
        string? DeletedBy { get; set; }
        DateTime? DeletedDate { get; set; }
    }
}