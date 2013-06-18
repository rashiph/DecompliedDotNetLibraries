namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.IdentityModel;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.ServiceModel;

    internal class PersistHelper
    {
        internal static object ActivateAndLoadFromByteStream(Guid clsid, byte[] byteStream)
        {
            IPersistStream persistableObject = SafeNativeMethods.CoCreateInstance(clsid, null, CLSCTX.INPROC_SERVER, typeof(IPersistStream).GUID) as IPersistStream;
            if (persistableObject == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CLSIDDoesNotSupportIPersistStream", new object[] { clsid.ToString("B") })));
            }
            LoadIntoObjectFromByteArray(persistableObject, byteStream);
            return persistableObject;
        }

        internal static byte[] ConvertHGlobalToByteArray(SafeHGlobalHandle hGlobal)
        {
            int length = SafeNativeMethods.GlobalSize(hGlobal).ToInt32();
            if (length <= 0)
            {
                return null;
            }
            byte[] destination = new byte[length];
            IntPtr source = SafeNativeMethods.GlobalLock(hGlobal);
            if (IntPtr.Zero == source)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new OutOfMemoryException());
            }
            try
            {
                Marshal.Copy(source, destination, 0, length);
            }
            finally
            {
                SafeNativeMethods.GlobalUnlock(hGlobal);
            }
            return destination;
        }

        internal static void LoadIntoObjectFromByteArray(IPersistStream persistableObject, byte[] byteStream)
        {
            SafeHGlobalHandle hGlobal = SafeHGlobalHandle.AllocHGlobal(byteStream.Length);
            IntPtr destination = SafeNativeMethods.GlobalLock(hGlobal);
            if (IntPtr.Zero == destination)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new OutOfMemoryException());
            }
            try
            {
                Marshal.Copy(byteStream, 0, destination, byteStream.Length);
                IStream pStm = SafeNativeMethods.CreateStreamOnHGlobal(hGlobal, false);
                try
                {
                    persistableObject.Load(pStm);
                }
                finally
                {
                    Marshal.ReleaseComObject(pStm);
                }
            }
            finally
            {
                SafeNativeMethods.GlobalUnlock(hGlobal);
            }
        }

        internal static byte[] PersistIPersistStreamToByteArray(IPersistStream persistableObject)
        {
            byte[] buffer;
            IStream pStm = SafeNativeMethods.CreateStreamOnHGlobal(SafeHGlobalHandle.InvalidHandle, false);
            try
            {
                persistableObject.Save(pStm, true);
                SafeHGlobalHandle hGlobalFromStream = SafeNativeMethods.GetHGlobalFromStream(pStm);
                if ((hGlobalFromStream == null) || (IntPtr.Zero == hGlobalFromStream.DangerousGetHandle()))
                {
                    throw Fx.AssertAndThrow("HGlobal returned from  GetHGlobalFromStream is NULL");
                }
                buffer = ConvertHGlobalToByteArray(hGlobalFromStream);
            }
            finally
            {
                Marshal.ReleaseComObject(pStm);
            }
            return buffer;
        }
    }
}

