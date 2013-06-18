namespace System.Windows.Markup
{
    using System;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"), AttributeUsage(AttributeTargets.Assembly, AllowMultiple=true)]
    public sealed class XmlnsCompatibleWithAttribute : Attribute
    {
        private string _newNamespace;
        private string _oldNamespace;

        public XmlnsCompatibleWithAttribute(string oldNamespace, string newNamespace)
        {
            if (oldNamespace == null)
            {
                throw new ArgumentNullException("oldNamespace");
            }
            if (newNamespace == null)
            {
                throw new ArgumentNullException("newNamespace");
            }
            this._oldNamespace = oldNamespace;
            this._newNamespace = newNamespace;
        }

        public string NewNamespace
        {
            get
            {
                return this._newNamespace;
            }
        }

        public string OldNamespace
        {
            get
            {
                return this._oldNamespace;
            }
        }
    }
}

