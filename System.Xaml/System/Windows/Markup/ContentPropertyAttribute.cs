namespace System.Windows.Markup
{
    using System;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"), AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    public sealed class ContentPropertyAttribute : Attribute
    {
        private string _name;

        public ContentPropertyAttribute()
        {
        }

        public ContentPropertyAttribute(string name)
        {
            this._name = name;
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }
    }
}

