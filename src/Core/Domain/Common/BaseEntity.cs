using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Common
{
    public abstract class BaseEntity : IBaseEntity
    {
        [JsonIgnore]
        private readonly List<BaseEvent> _domainEvents = new();

        public Guid Id { get; set; }

        [NotMapped]
        public IReadOnlyCollection<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvent(BaseEvent domainEvent) => _domainEvents.Add(domainEvent);
        public void RemoveDomainEvent(BaseEvent domainEvent) => _domainEvents.Remove(domainEvent);
        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}