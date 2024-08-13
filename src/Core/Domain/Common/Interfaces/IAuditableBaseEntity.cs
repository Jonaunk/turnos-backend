namespace Domain.Common.Interfaces
{
    public interface IAuditableBaseEntity
    {
        string? CreatedBy { get; set; }
        DateTime Created { get; set; }
        string? ModifiedBy { get; set; }
        DateTime? Modified { get; set; }
    }
}