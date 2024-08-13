using Domain.Common;

namespace Domain.Entities.Users
{
    public class Country : BaseEntity
    {
        public string? Name { get; set; }

        public virtual List<User>? Users { get; set; }
    }
}