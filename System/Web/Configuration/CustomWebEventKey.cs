namespace System.Web.Configuration
{
    using System;

    internal class CustomWebEventKey
    {
        internal int _eventCode;
        internal Type _type;

        internal CustomWebEventKey(Type eventType, int eventCode)
        {
            this._type = eventType;
            this._eventCode = eventCode;
        }
    }
}

