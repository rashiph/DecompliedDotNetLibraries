namespace System.Activities
{
    using System;

    [AttributeUsage(AttributeTargets.Property, AllowMultiple=true)]
    public sealed class OverloadGroupAttribute : Attribute
    {
        private string groupName;

        public OverloadGroupAttribute()
        {
        }

        public OverloadGroupAttribute(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("groupName");
            }
            this.groupName = groupName;
        }

        public string GroupName
        {
            get
            {
                return this.groupName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw FxTrace.Exception.ArgumentNullOrEmpty("value");
                }
                this.groupName = value;
            }
        }

        public override object TypeId
        {
            get
            {
                return this;
            }
        }
    }
}

