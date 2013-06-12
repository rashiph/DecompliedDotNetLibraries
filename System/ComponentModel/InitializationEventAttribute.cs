namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class InitializationEventAttribute : Attribute
    {
        private string eventName;

        public InitializationEventAttribute(string eventName)
        {
            this.eventName = eventName;
        }

        public string EventName
        {
            get
            {
                return this.eventName;
            }
        }
    }
}

