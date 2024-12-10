using Domain.Common;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Identity.Models
{
    public class ApplicationUser : IdentityUser
    {
        [JsonIgnore]
        private readonly List<DomainEvent> _domainEvents = new();


        public string? Nombre { get; set; }
        public string? Apellido { get; set; }

        [NotMapped]
        public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);
        public void RemoveDomainEvent(DomainEvent domainEvent) => _domainEvents.Remove(domainEvent);
        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}
