namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Security;

    internal interface IStreamable
    {
        [SecurityCritical]
        void Read(__BinaryParser input);
        void Write(__BinaryWriter sout);
    }
}

