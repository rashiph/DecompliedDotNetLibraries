namespace System.Runtime.InteropServices
{
    using System;

    [ComVisible(true), AttributeUsage(AttributeTargets.Interface, Inherited=false)]
    public sealed class ComEventInterfaceAttribute : Attribute
    {
        internal Type _EventProvider;
        internal Type _SourceInterface;

        public ComEventInterfaceAttribute(Type SourceInterface, Type EventProvider)
        {
            this._SourceInterface = SourceInterface;
            this._EventProvider = EventProvider;
        }

        public Type EventProvider
        {
            get
            {
                return this._EventProvider;
            }
        }

        public Type SourceInterface
        {
            get
            {
                return this._SourceInterface;
            }
        }
    }
}

