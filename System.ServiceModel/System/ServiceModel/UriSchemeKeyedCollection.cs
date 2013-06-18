namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;

    public class UriSchemeKeyedCollection : SynchronizedKeyedCollection<string, Uri>
    {
        internal UriSchemeKeyedCollection(object syncRoot) : base(syncRoot)
        {
        }

        public UriSchemeKeyedCollection(params Uri[] addresses)
        {
            if (addresses == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("addresses");
            }
            for (int i = 0; i < addresses.Length; i++)
            {
                base.Add(addresses[i]);
            }
        }

        protected override string GetKeyForItem(Uri item)
        {
            return item.Scheme;
        }

        protected override void InsertItem(int index, Uri item)
        {
            ValidateBaseAddress(item, "item");
            if (base.Contains(item.Scheme))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("item", System.ServiceModel.SR.GetString("BaseAddressDuplicateScheme", new object[] { item.Scheme }));
            }
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, Uri item)
        {
            ValidateBaseAddress(item, "item");
            if ((base[index].Scheme != item.Scheme) && base.Contains(item.Scheme))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("item", System.ServiceModel.SR.GetString("BaseAddressDuplicateScheme", new object[] { item.Scheme }));
            }
            base.SetItem(index, item);
        }

        internal static void ValidateBaseAddress(Uri uri, string argumentName)
        {
            if (uri == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(argumentName);
            }
            if (!uri.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(argumentName, System.ServiceModel.SR.GetString("BaseAddressMustBeAbsolute"));
            }
            if (!string.IsNullOrEmpty(uri.UserInfo))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(argumentName, System.ServiceModel.SR.GetString("BaseAddressCannotHaveUserInfo"));
            }
            if (!string.IsNullOrEmpty(uri.Query))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(argumentName, System.ServiceModel.SR.GetString("BaseAddressCannotHaveQuery"));
            }
            if (!string.IsNullOrEmpty(uri.Fragment))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(argumentName, System.ServiceModel.SR.GetString("BaseAddressCannotHaveFragment"));
            }
        }
    }
}

