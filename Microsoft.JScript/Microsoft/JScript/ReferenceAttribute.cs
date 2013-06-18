namespace Microsoft.JScript
{
    using System;

    [AttributeUsage(AttributeTargets.Assembly)]
    public class ReferenceAttribute : Attribute
    {
        public string reference;

        public ReferenceAttribute(string reference)
        {
            this.reference = reference;
        }
    }
}

