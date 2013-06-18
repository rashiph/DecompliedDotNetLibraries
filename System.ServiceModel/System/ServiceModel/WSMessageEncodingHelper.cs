namespace System.ServiceModel
{
    using System;
    using System.ServiceModel.Channels;

    internal static class WSMessageEncodingHelper
    {
        internal static bool IsDefined(WSMessageEncoding value)
        {
            if (value != WSMessageEncoding.Text)
            {
                return (value == WSMessageEncoding.Mtom);
            }
            return true;
        }

        internal static void SyncUpEncodingBindingElementProperties(TextMessageEncodingBindingElement textEncoding, MtomMessageEncodingBindingElement mtomEncoding)
        {
            textEncoding.ReaderQuotas.CopyTo(mtomEncoding.ReaderQuotas);
            mtomEncoding.WriteEncoding = textEncoding.WriteEncoding;
        }
    }
}

