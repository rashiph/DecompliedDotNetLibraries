namespace System.Windows.Markup
{
    using System;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"), AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple=true)]
    public sealed class DependsOnAttribute : Attribute
    {
        private string _name;

        public DependsOnAttribute(string name)
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

        public override object TypeId
        {
            get
            {
                return this;
            }
        }
    }
}

