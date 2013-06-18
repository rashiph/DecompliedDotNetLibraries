namespace System.Runtime.Serialization.Json
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public interface IXmlJsonWriterInitializer
    {
        void SetOutput(Stream stream, Encoding encoding, bool ownsStream);
    }
}

