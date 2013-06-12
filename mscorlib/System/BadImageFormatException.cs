namespace System
{
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, ComVisible(true)]
    public class BadImageFormatException : SystemException
    {
        private string _fileName;
        private string _fusionLog;

        public BadImageFormatException() : base(Environment.GetResourceString("Arg_BadImageFormatException"))
        {
            base.SetErrorCode(-2147024885);
        }

        public BadImageFormatException(string message) : base(message)
        {
            base.SetErrorCode(-2147024885);
        }

        [SecuritySafeCritical]
        protected BadImageFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._fileName = info.GetString("BadImageFormat_FileName");
            try
            {
                this._fusionLog = info.GetString("BadImageFormat_FusionLog");
            }
            catch
            {
                this._fusionLog = null;
            }
        }

        public BadImageFormatException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2147024885);
        }

        public BadImageFormatException(string message, string fileName) : base(message)
        {
            base.SetErrorCode(-2147024885);
            this._fileName = fileName;
        }

        public BadImageFormatException(string message, string fileName, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2147024885);
            this._fileName = fileName;
        }

        private BadImageFormatException(string fileName, string fusionLog, int hResult) : base(null)
        {
            base.SetErrorCode(hResult);
            this._fileName = fileName;
            this._fusionLog = fusionLog;
            this.SetMessageField();
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("BadImageFormat_FileName", this._fileName, typeof(string));
            try
            {
                info.AddValue("BadImageFormat_FusionLog", this.FusionLog, typeof(string));
            }
            catch (SecurityException)
            {
            }
        }

        private void SetMessageField()
        {
            if (base._message == null)
            {
                if ((this._fileName == null) && (base.HResult == -2146233088))
                {
                    base._message = Environment.GetResourceString("Arg_BadImageFormatException");
                }
                else
                {
                    base._message = FileLoadException.FormatFileLoadExceptionMessage(this._fileName, base.HResult);
                }
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

