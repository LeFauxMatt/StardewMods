namespace CommonHarmony.Models
{
    using System;
    using Enums;

    internal class ServiceHandler<TEventArgs> : IComparable
    {
        public ServiceHandler(TEventArgs handler, int registrationOrder, HandlerPriority priority)
        {
            this.Handler = handler;
            this.RegistrationOrder = registrationOrder;
            this.Priority = priority;
        }
        public TEventArgs Handler { get; }

        public int RegistrationOrder { get; }

        public HandlerPriority Priority { get; }

        public int CompareTo(object obj)
        {
            if (obj is not ServiceHandler<TEventArgs> other)
            {
                throw new ArgumentException("Can't compare to an unrelated object type.");
            }

            var priorityCompare = -this.Priority.CompareTo(other.Priority); // higher value = sort first
            return priorityCompare != 0
                ? priorityCompare
                : this.RegistrationOrder.CompareTo(other.RegistrationOrder);
        }
    }
}