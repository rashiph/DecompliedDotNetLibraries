namespace System.Web.Configuration
{
    using System;
    using System.Runtime.InteropServices;

    internal class IISVersionHelper : IDisposable
    {
        private IIISVersion _version;
        private IIISVersionManager _versionManager;
        private const int IIS_PRODUCT_EXPRESS = 2;

        internal IISVersionHelper(string version)
        {
            if (version != null)
            {
                try
                {
                    this._versionManager = CreateVersionManager();
                    this._version = this._versionManager.GetVersionObject(version, 2);
                    this._version.ApplyManifestContext();
                }
                catch
                {
                    this.Release();
                    throw;
                }
            }
        }

        private static IIISVersionManager CreateVersionManager()
        {
            bool throwOnError = true;
            return (IIISVersionManager) Activator.CreateInstance(Type.GetTypeFromProgID("Microsoft.IIS.VersionManager", throwOnError));
        }

        public void Dispose()
        {
            if (this._version != null)
            {
                this._version.ClearManifestContext();
                this.Release();
            }
        }

        private void Release()
        {
            if (this._version != null)
            {
                Marshal.ReleaseComObject(this._version);
                this._version = null;
            }
            if (this._versionManager != null)
            {
                Marshal.ReleaseComObject(this._versionManager);
                this._versionManager = null;
            }
        }

        [ComImport, Guid("1B036F99-B240-4116-A6A0-B54EC5B2438E"), InterfaceType((short) 1)]
        private interface IIISVersion
        {
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetPropertyValue([In, MarshalAs(UnmanagedType.BStr)] string bstrName);
            [return: MarshalAs(UnmanagedType.Struct)]
            object CreateObjectFromProgId([In, MarshalAs(UnmanagedType.BStr)] string bstrObjectName);
            [return: MarshalAs(UnmanagedType.Struct)]
            object CreateObjectFromClsId([In] Guid clsidObject);
            void ApplyIISEnvironmentVariables();
            void ClearIISEnvironmentVariables();
            void ApplyManifestContext();
            void ClearManifestContext();
        }

        [ComImport, InterfaceType((short) 1), Guid("9CDA0717-2EB5-42b3-B5B0-16F4941B2029")]
        private interface IIISVersionManager
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            IISVersionHelper.IIISVersion GetVersionObject([In, MarshalAs(UnmanagedType.BStr)] string bstrVersion, [In, MarshalAs(UnmanagedType.I4)] int productType);
            [return: MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_VARIANT)]
            IISVersionHelper.IIISVersion[] GetAllVersionObjects();
        }
    }
}

