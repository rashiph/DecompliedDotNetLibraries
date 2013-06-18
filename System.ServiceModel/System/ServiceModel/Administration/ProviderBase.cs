namespace System.ServiceModel.Administration
{
    using System;
    using System.Collections;
    using System.ServiceModel;

    internal abstract class ProviderBase : IWmiProvider
    {
        protected ProviderBase()
        {
        }

        public static void FillCollectionInfo(ICollection info, IWmiInstance instance, string propertyName)
        {
            string[] strArray = new string[info.Count];
            int num = 0;
            foreach (object obj2 in info)
            {
                strArray[num++] = obj2.ToString();
            }
            instance.SetProperty(propertyName, strArray);
        }

        public static void FillCollectionInfo(IEnumerable info, IWmiInstance instance, string propertyName)
        {
            int num = 0;
            using (IEnumerator enumerator = info.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    object current = enumerator.Current;
                    num++;
                }
            }
            string[] strArray = new string[num];
            num = 0;
            foreach (object obj2 in info)
            {
                strArray[num++] = obj2.ToString();
            }
            instance.SetProperty(propertyName, strArray);
        }

        bool IWmiProvider.DeleteInstance(IWmiInstance instance)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WbemNotSupportedException());
        }

        void IWmiProvider.EnumInstances(IWmiInstances instances)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WbemNotSupportedException());
        }

        bool IWmiProvider.GetInstance(IWmiInstance contract)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WbemNotSupportedException());
        }

        bool IWmiProvider.InvokeMethod(IWmiMethodContext method)
        {
            method.ReturnParameter = 0;
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WbemNotSupportedException());
        }

        bool IWmiProvider.PutInstance(IWmiInstance instance)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WbemNotSupportedException());
        }
    }
}

