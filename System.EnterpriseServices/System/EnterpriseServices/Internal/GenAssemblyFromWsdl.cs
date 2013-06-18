namespace System.EnterpriseServices.Internal
{
    using System;
    using System.Collections;
    using System.EnterpriseServices;
    using System.EnterpriseServices.Thunk;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.MetadataServices;
    using System.Threading;

    internal class GenAssemblyFromWsdl
    {
        private bool ExceptionThrown;
        private string filename = "";
        private string pathname = "";
        private Exception SavedException;
        private Thread thisthread;
        private SafeUserTokenHandle threadtoken;
        private const uint TOKEN_IMPERSONATE = 4;
        private string wsdlurl = "";

        public GenAssemblyFromWsdl()
        {
            this.thisthread = new Thread(new ThreadStart(this.Generate));
            this.threadtoken = new SafeUserTokenHandle();
        }

        public void Generate()
        {
            try
            {
                if ((this.threadtoken != null) && !System.EnterpriseServices.Internal.NativeMethods.SetThreadToken(IntPtr.Zero, this.threadtoken))
                {
                    throw new COMException(Resource.FormatString("Err_SetThreadToken"), Marshal.GetHRForLastWin32Error());
                }
                if (this.wsdlurl.Length > 0)
                {
                    Stream outputStream = new MemoryStream();
                    ArrayList outCodeStreamList = new ArrayList();
                    MetaData.RetrieveSchemaFromUrlToStream(this.wsdlurl, outputStream);
                    outputStream.Position = 0L;
                    MetaData.ConvertSchemaStreamToCodeSourceStream(true, this.pathname, outputStream, outCodeStreamList);
                    MetaData.ConvertCodeSourceStreamToAssemblyFile(outCodeStreamList, this.filename, null);
                }
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                ComSoapPublishError.Report(exception.ToString());
                this.SavedException = exception;
                this.ExceptionThrown = true;
            }
        }

        public void Run(string WsdlUrl, string FileName, string PathName)
        {
            try
            {
                if ((WsdlUrl.Length > 0) && (FileName.Length > 0))
                {
                    this.wsdlurl = WsdlUrl;
                    this.filename = PathName + FileName;
                    this.pathname = PathName;
                    if (!System.EnterpriseServices.Internal.NativeMethods.OpenThreadToken(System.EnterpriseServices.Internal.NativeMethods.GetCurrentThread(), 4, true, ref this.threadtoken) && (Marshal.GetLastWin32Error() != 0x3f0))
                    {
                        throw new COMException(Resource.FormatString("Err_OpenThreadToken"), Marshal.GetHRForLastWin32Error());
                    }
                    SafeUserTokenHandle handle = null;
                    try
                    {
                        handle = new SafeUserTokenHandle(Security.SuspendImpersonation(), true);
                        this.thisthread.Start();
                    }
                    finally
                    {
                        if (handle != null)
                        {
                            Security.ResumeImpersonation(handle.DangerousGetHandle());
                            handle.Dispose();
                        }
                    }
                    this.thisthread.Join();
                    if (this.ExceptionThrown)
                    {
                        throw this.SavedException;
                    }
                }
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                ComSoapPublishError.Report(exception.ToString());
                throw;
            }
        }
    }
}

