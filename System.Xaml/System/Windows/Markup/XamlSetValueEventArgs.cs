namespace System.Windows.Markup
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Xaml;

    public class XamlSetValueEventArgs : EventArgs
    {
        public XamlSetValueEventArgs(XamlMember member, object value)
        {
            this.Value = value;
            this.Member = member;
        }

        public virtual void CallBase()
        {
        }

        public bool Handled { get; set; }

        public XamlMember Member { get; private set; }

        public object Value { get; private set; }
    }
}

