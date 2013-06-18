namespace System.Windows.Markup
{
    using System;
    using System.Runtime.CompilerServices;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=true), TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public sealed class ContentWrapperAttribute : Attribute
    {
        private Type _contentWrapper;

        public ContentWrapperAttribute(Type contentWrapper)
        {
            this._contentWrapper = contentWrapper;
        }

        public override bool Equals(object obj)
        {
            ContentWrapperAttribute attribute = obj as ContentWrapperAttribute;
            if (attribute == null)
            {
                return false;
            }
            return (this._contentWrapper == attribute._contentWrapper);
        }

        public override int GetHashCode()
        {
            return this._contentWrapper.GetHashCode();
        }

        public Type ContentWrapper
        {
            get
            {
                return this._contentWrapper;
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

