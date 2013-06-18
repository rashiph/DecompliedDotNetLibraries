namespace System.ServiceModel.MsmqIntegration
{
    using System;

    internal static class MsmqMessageSerializationFormatHelper
    {
        internal static bool IsDefined(MsmqMessageSerializationFormat value)
        {
            if (((value != MsmqMessageSerializationFormat.ActiveX) && (value != MsmqMessageSerializationFormat.Binary)) && ((value != MsmqMessageSerializationFormat.ByteArray) && (value != MsmqMessageSerializationFormat.Stream)))
            {
                return (value == MsmqMessageSerializationFormat.Xml);
            }
            return true;
        }
    }
}

