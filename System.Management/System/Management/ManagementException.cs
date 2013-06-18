namespace System.Management
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class ManagementException : SystemException
    {
        private ManagementStatus errorCode;
        private ManagementBaseObject errorObject;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementException() : this(ManagementStatus.Failed, "", null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementException(string message) : this(ManagementStatus.Failed, message, null)
        {
        }

        protected ManagementException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.errorCode = (ManagementStatus) info.GetValue("errorCode", typeof(ManagementStatus));
            this.errorObject = info.GetValue("errorObject", typeof(ManagementBaseObject)) as ManagementBaseObject;
        }

        public ManagementException(string message, Exception innerException) : this(innerException, message, null)
        {
            if (!(innerException is ManagementException))
            {
                this.errorCode = ManagementStatus.Failed;
            }
        }

        internal ManagementException(Exception e, string msg, ManagementBaseObject errObj) : base(msg, e)
        {
            try
            {
                if (e is ManagementException)
                {
                    this.errorCode = ((ManagementException) e).ErrorCode;
                    if (this.errorObject != null)
                    {
                        this.errorObject = (ManagementBaseObject) ((ManagementException) e).errorObject.Clone();
                    }
                    else
                    {
                        this.errorObject = null;
                    }
                }
                else if (e is COMException)
                {
                    this.errorCode = (ManagementStatus) ((COMException) e).ErrorCode;
                }
                else
                {
                    this.errorCode = (ManagementStatus) base.HResult;
                }
            }
            catch
            {
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ManagementException(ManagementStatus errorCode, string msg, ManagementBaseObject errObj) : base(msg)
        {
            this.errorCode = errorCode;
            this.errorObject = errObj;
        }

        private static string GetMessage(Exception e)
        {
            string message = null;
            if (e is COMException)
            {
                message = GetMessage((ManagementStatus) ((COMException) e).ErrorCode);
            }
            if (message == null)
            {
                message = e.Message;
            }
            return message;
        }

        private static string GetMessage(ManagementStatus errorCode)
        {
            string messageText = null;
            IWbemStatusCodeText text = null;
            text = (IWbemStatusCodeText) new WbemStatusCodeText();
            if (text != null)
            {
                try
                {
                    if (text.GetErrorCodeText_((int) errorCode, 0, 1, out messageText) != 0)
                    {
                        int num = text.GetErrorCodeText_((int) errorCode, 0, 0, out messageText);
                    }
                }
                catch
                {
                }
            }
            return messageText;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("errorCode", this.errorCode);
            info.AddValue("errorObject", this.errorObject);
        }

        internal static void ThrowWithExtendedInfo(Exception e)
        {
            ManagementBaseObject errObj = null;
            string msg = null;
            IWbemClassObjectFreeThreaded errorInfo = WbemErrorInfo.GetErrorInfo();
            if (errorInfo != null)
            {
                errObj = new ManagementBaseObject(errorInfo);
            }
            if (((msg = GetMessage(e)) == null) && (errObj != null))
            {
                try
                {
                    msg = (string) errObj["Description"];
                }
                catch
                {
                }
            }
            throw new ManagementException(e, msg, errObj);
        }

        internal static void ThrowWithExtendedInfo(ManagementStatus errorCode)
        {
            ManagementBaseObject errObj = null;
            string msg = null;
            IWbemClassObjectFreeThreaded errorInfo = WbemErrorInfo.GetErrorInfo();
            if (errorInfo != null)
            {
                errObj = new ManagementBaseObject(errorInfo);
            }
            if (((msg = GetMessage(errorCode)) == null) && (errObj != null))
            {
                try
                {
                    msg = (string) errObj["Description"];
                }
                catch
                {
                }
            }
            throw new ManagementException(errorCode, msg, errObj);
        }

        public ManagementStatus ErrorCode
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.errorCode;
            }
        }

        public ManagementBaseObject ErrorInformation
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.errorObject;
            }
        }
    }
}

