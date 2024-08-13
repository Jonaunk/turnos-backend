using MediatR;

namespace Domain.Common
{
    public abstract class BaseEvent : INotification
    {
        protected BaseEvent()
        {
            DateOccurred = DateTimeOffset.UtcNow;
        }
        public bool IsPublished { get; set; }
        public DateTimeOffset DateOccurred { get; protected set; } = DateTime.UtcNow;
    }
}