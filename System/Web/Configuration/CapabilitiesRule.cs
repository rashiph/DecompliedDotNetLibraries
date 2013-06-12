namespace System.Web.Configuration
{
    using System;

    internal abstract class CapabilitiesRule
    {
        internal int _type;
        internal const int Assign = 1;
        internal const int Case = 3;
        internal const int Filter = 2;
        internal const int Use = 0;

        protected CapabilitiesRule()
        {
        }

        internal abstract void Evaluate(CapabilitiesState state);

        internal virtual int Type
        {
            get
            {
                return this._type;
            }
        }
    }
}

