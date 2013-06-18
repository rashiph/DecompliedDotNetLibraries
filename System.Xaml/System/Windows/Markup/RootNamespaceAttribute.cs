namespace System.Windows.Markup
{
    using System;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"), AttributeUsage(AttributeTargets.Assembly)]
    public sealed class RootNamespaceAttribute : Attribute
    {
        private string _nameSpace;

        public RootNamespaceAttribute(string nameSpace)
        {
            this._nameSpace = nameSpace;
        }

        public string Namespace
        {
            get
            {
                return this._nameSpace;
            }
        }
    }
}

