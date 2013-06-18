namespace System.Activities
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RequiredArgumentAttribute : Attribute
    {
        public override object TypeId
        {
            get
            {
                return this;
            }
        }
    }
}

