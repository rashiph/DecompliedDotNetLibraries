namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Services;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [DataContract]
    public class PersistStreamTypeWrapper : IExtensibleDataObject
    {
        [DataMember]
        internal Guid clsid;
        [DataMember]
        internal byte[] dataStream;

        public void GetObject<T>(ref T obj)
        {
            if (this.clsid == typeof(T).GUID)
            {
                IntPtr iUnknownForObject = Marshal.GetIUnknownForObject((T) obj);
                if (IntPtr.Zero == iUnknownForObject)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("UnableToRetrievepUnk")));
                }
                try
                {
                    IntPtr zero = IntPtr.Zero;
                    Guid gUID = typeof(IPersistStream).GUID;
                    int num = Marshal.QueryInterface(iUnknownForObject, ref gUID, out zero);
                    if (HR.S_OK == num)
                    {
                        try
                        {
                            if (IntPtr.Zero == zero)
                            {
                                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("PersistWrapperIsNull")));
                            }
                            IPersistStream persistableObject = (IPersistStream) EnterpriseServicesHelper.WrapIUnknownWithComObject(zero);
                            try
                            {
                                PersistHelper.LoadIntoObjectFromByteArray(persistableObject, this.dataStream);
                            }
                            finally
                            {
                                Marshal.ReleaseComObject(persistableObject);
                            }
                            return;
                        }
                        finally
                        {
                            Marshal.Release(zero);
                        }
                    }
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CLSIDDoesNotSupportIPersistStream", new object[] { typeof(T).GUID.ToString("B") })));
                }
                finally
                {
                    Marshal.Release(iUnknownForObject);
                }
            }
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CLSIDOfTypeDoesNotMatch", new object[] { typeof(T).GUID.ToString(), this.clsid.ToString("B") })));
        }

        public void SetObject<T>(T obj)
        {
            if (Marshal.IsComObject(obj))
            {
                IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(obj);
                if (IntPtr.Zero == iUnknownForObject)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("UnableToRetrievepUnk")));
                }
                try
                {
                    IntPtr zero = IntPtr.Zero;
                    Guid gUID = typeof(IPersistStream).GUID;
                    int num = Marshal.QueryInterface(iUnknownForObject, ref gUID, out zero);
                    if (HR.S_OK == num)
                    {
                        try
                        {
                            if (IntPtr.Zero == zero)
                            {
                                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("PersistWrapperIsNull")));
                            }
                            IPersistStream persistableObject = (IPersistStream) EnterpriseServicesHelper.WrapIUnknownWithComObject(zero);
                            try
                            {
                                this.dataStream = PersistHelper.PersistIPersistStreamToByteArray(persistableObject);
                                this.clsid = typeof(T).GUID;
                            }
                            finally
                            {
                                Marshal.ReleaseComObject(persistableObject);
                            }
                            return;
                        }
                        finally
                        {
                            Marshal.Release(zero);
                        }
                    }
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CLSIDDoesNotSupportIPersistStream", new object[] { typeof(T).GUID.ToString("B") })));
                }
                finally
                {
                    Marshal.Release(iUnknownForObject);
                }
            }
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("NotAComObject")));
        }

        public ExtensionDataObject ExtensionData { get; set; }
    }
}

