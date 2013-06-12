namespace System.Runtime.Serialization.Formatters
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;

    [ComVisible(true)]
    public interface ISoapMessage
    {
        Header[] Headers { get; set; }

        string MethodName { get; set; }

        string[] ParamNames { get; set; }

        Type[] ParamTypes { get; set; }

        object[] ParamValues { get; set; }

        string XmlNameSpace { get; set; }
    }
}

