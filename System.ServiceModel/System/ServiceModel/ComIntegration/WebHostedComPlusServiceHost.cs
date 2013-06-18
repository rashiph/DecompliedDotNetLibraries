namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;

    internal class WebHostedComPlusServiceHost : ComPlusServiceHost
    {
        public WebHostedComPlusServiceHost(string webhostParams, Uri[] baseAddresses)
        {
            Guid guid;
            Guid guid2;
            HostingMode webHostInProcess;
            foreach (Uri uri in baseAddresses)
            {
                base.InternalBaseAddresses.Add(uri);
            }
            string[] strArray = webhostParams.Split(new char[] { ',' });
            if (strArray.Length != 2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(System.ServiceModel.SR.GetString("ServiceStringFormatError", new object[] { webhostParams })));
            }
            if (!DiagnosticUtility.Utility.TryCreateGuid(strArray[0], out guid))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(System.ServiceModel.SR.GetString("ServiceStringFormatError", new object[] { webhostParams })));
            }
            if (!DiagnosticUtility.Utility.TryCreateGuid(strArray[1], out guid2))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(System.ServiceModel.SR.GetString("ServiceStringFormatError", new object[] { webhostParams })));
            }
            string str = guid.ToString("B").ToUpperInvariant();
            ComCatalogObject applicationObject = CatalogUtil.FindApplication(guid2);
            if (applicationObject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(System.ServiceModel.SR.GetString("ApplicationNotFound", new object[] { guid2.ToString("B").ToUpperInvariant() })));
            }
            ComCatalogCollection collection = applicationObject.GetCollection("Components");
            ComCatalogObject classObject = null;
            ComCatalogCollection.Enumerator enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ComCatalogObject current = enumerator.Current;
                string str2 = (string) current.GetValue("CLSID");
                if (str.Equals(str2, StringComparison.OrdinalIgnoreCase))
                {
                    classObject = current;
                    break;
                }
            }
            if (classObject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(System.ServiceModel.SR.GetString("ClsidNotInApplication", new object[] { str, guid2.ToString("B").ToUpperInvariant() })));
            }
            ServicesSection section = ServicesSection.GetSection();
            ServiceElement service = null;
            foreach (ServiceElement element2 in section.Services)
            {
                Guid empty = Guid.Empty;
                Guid result = Guid.Empty;
                string[] strArray2 = element2.Name.Split(new char[] { ',' });
                if ((((strArray2.Length == 2) && DiagnosticUtility.Utility.TryCreateGuid(strArray2[0], out result)) && (DiagnosticUtility.Utility.TryCreateGuid(strArray2[1], out empty) && (empty == guid))) && (result == guid2))
                {
                    service = element2;
                    break;
                }
            }
            if (service == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(System.ServiceModel.SR.GetString("ClsidNotInConfiguration", new object[] { str })));
            }
            if (((int) applicationObject.GetValue("Activation")) == 0)
            {
                webHostInProcess = HostingMode.WebHostInProcess;
            }
            else
            {
                webHostInProcess = HostingMode.WebHostOutOfProcess;
            }
            base.Initialize(guid, service, applicationObject, classObject, webHostInProcess);
        }
    }
}

