namespace Microsoft.SqlServer.Server
{
    using System;
    using System.IO;

    public interface IBinarySerialize
    {
        void Read(BinaryReader r);
        void Write(BinaryWriter w);
    }
}

