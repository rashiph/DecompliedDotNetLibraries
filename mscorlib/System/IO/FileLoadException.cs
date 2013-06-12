namespace System.IO
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, ComVisible(true)]
    public class FileLoadException : IOException
    {
        private string _fileName;
        private string _fusionLog;

        public FileLoadException() : base(Environment.GetResourceString("IO.FileLoad"))
        {
            base.SetErrorCode(-2146232799);
        }

        public FileLoadException(string message) : base(message)
        {
            base.SetErrorCode(-2146232799);
        }

        [SecuritySafeCritical]
        protected FileLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._fileName = info.GetString("FileLoad_FileName");
            try
            {
                this._fusionLog = info.GetString("FileLoad_FusionLog");
            }
            catch
            {
                this._fusionLog = null;
            }
        }

        public FileLoadException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2146232799);
        }

        public FileLoadException(string message, string fileName) : base(message)
        {
            base.SetErrorCode(-2146232799);
            this._fileName = fileName;
        }

        public FileLoadException(string message, string fileName, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2146232799);
            this._fileName = fileName;
        }

        private FileLoadException(string fileName, string fusionLog, int hResult) : base(null)
        {
            base.SetErrorCode(hResult);
            this._fileName = fileName;
            this._fusionLog = fusionLog;
            this.SetMessageField();
        }

        [SecuritySafeCritical]
        internal static string FormatFileLoadExceptionMessage(string fileName, int hResult)
        {
            string s = null;
            GetFileLoadExceptionMessage(hResult, JitHelpers.GetStringHandleOnStack(ref s));
            string str2 = null;
            GetMessageForHR(hResult, JitHelpers.GetStringHandleOnStack(ref str2));
            return string.Format(CultureInfo.CurrentCulture, s, new object[] { fileName, str2 });
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetFileLoadExceptionMessage(int hResult, StringHandleOnStack retString);
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetMessageForHR(int hresult, StringHandleOnStack retString);
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("FileLoad_FileName", this._fileName, typeof(string));
            try
            {
                info.AddValue("FileLoad_FusionLog", this.FusionLog, typeof(string));
            }
            catch (SecurityException)
            {
            }
        }

        private void SetMessageField()
        {
            if (base._message == null)
            {
                base._message = FormatFileLoadExceptionMessage(this._fileName, base.HResult);
            }
        }

        [SecuritySafeCritical]
        public override string ToString()
        {
            string str = base.GetType().FullName + ": " + this.Message;
            if ((this._fileName != null) && (this._fileName.Length != 0))
            {
                str = str + Environment.NewLine + Environment.GetResourceString("IO.FileName_Name", new object[] { this._fileName });
            }
            if (base.InnerException != null)
            {
                str = str + " ---> " + base.InnerException.ToString();
            }
            if (this.StackTrace != null)
            {
                str = str + Environment.NewLine + this.StackTrace;
            }
            try
            {
                if (this.FusionLog == null)
                {
                    return str;
                }
                if (str == null)
                {
                    str = " ";
                }
                str = str + Environment.NewLine;
                str = str + Environment.NewLine;
                str = str + this.FusionLog;
            }
            catch (SecurityException)
            {
            }
            return str;
        }

        public string FileName
        {
            get
            {
                return this._fileName;
            }
        }

        public string FusionLog
        {
            [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy | SecurityPermissionFlag.ControlEvidence)]
            get
            {
                return this._fusionLog;
            }
        }

        public override string Message
        {
            get
            {
                this.SetMessageField();
                return base._message;
            }
        }
    }
}

