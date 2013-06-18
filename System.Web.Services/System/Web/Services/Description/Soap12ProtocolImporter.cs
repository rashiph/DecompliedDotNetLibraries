namespace System.Web.Services.Description
{
    using System;
    using System.Security.Permissions;
    using System.Web.Services;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    internal class Soap12ProtocolImporter : SoapProtocolImporter
    {
        protected override bool IsBindingSupported()
        {
            Soap12Binding binding = (Soap12Binding) base.Binding.Extensions.Find(typeof(Soap12Binding));
            if (binding == null)
            {
                return false;
            }
            if (base.GetTransport(binding.Transport) == null)
            {
                base.UnsupportedBindingWarning(Res.GetString("ThereIsNoSoapTransportImporterThatUnderstands1", new object[] { binding.Transport }));
                return false;
            }
            return true;
        }

        protected override bool IsSoapEncodingPresent(string uriList)
        {
            int startIndex = 0;
            do
            {
                startIndex = uriList.IndexOf("http://www.w3.org/2003/05/soap-encoding", startIndex, StringComparison.Ordinal);
                if (startIndex < 0)
                {
                    break;
                }
                int num2 = startIndex + "http://www.w3.org/2003/05/soap-encoding".Length;
                if (((startIndex == 0) || (uriList[startIndex - 1] == ' ')) && ((num2 == uriList.Length) || (uriList[num2] == ' ')))
                {
                    return true;
                }
                startIndex = num2;
            }
            while (startIndex < uriList.Length);
            if (base.IsSoapEncodingPresent(uriList))
            {
                base.UnsupportedOperationBindingWarning(Res.GetString("WebSoap11EncodingStyleNotSupported1", new object[] { "http://www.w3.org/2003/05/soap-encoding" }));
            }
            return false;
        }

        public override string ProtocolName
        {
            get
            {
                return "Soap12";
            }
        }
    }
}

