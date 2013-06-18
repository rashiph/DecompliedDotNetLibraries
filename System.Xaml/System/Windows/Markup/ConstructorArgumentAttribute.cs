namespace System.Windows.Markup
{
    using System;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"), AttributeUsage(AttributeTargets.Property, AllowMultiple=false, Inherited=false)]
    public sealed class ConstructorArgumentAttribute : Attribute
    {
        private string _argumentName;

        public ConstructorArgumentAttribute(string argumentName)
        {
            this._argumentName = argumentName;
        }

        public string ArgumentName
        {
            get
            {
                return this._argumentName;
            }
        }
    }
}

