namespace System.Web.UI
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ValidationPropertyAttribute : Attribute
    {
        private readonly string name;

        public ValidationPropertyAttribute(string name)
        {
            this.name = name;
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }
    }
}

