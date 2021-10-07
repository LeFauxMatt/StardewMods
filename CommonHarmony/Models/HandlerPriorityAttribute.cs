namespace CommonHarmony.Models
{
    using System;
    using Enums;

    [AttributeUsage(AttributeTargets.Method)]
    internal class HandlerPriorityAttribute : Attribute
    {
        public HandlerPriorityAttribute(HandlerPriority priority)
        {
            this.Priority = priority;
        }
        public HandlerPriority Priority { get; }
    }
}