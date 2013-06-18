namespace System.Windows.Markup
{
    using System;

    public abstract class MemberDefinition
    {
        protected MemberDefinition()
        {
        }

        public abstract string Name { get; set; }
    }
}

