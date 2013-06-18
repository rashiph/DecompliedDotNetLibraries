namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class SyncFromAllServersOperationException : ActiveDirectoryOperationException, ISerializable
    {
        private SyncFromAllServersErrorInformation[] errors;

        public SyncFromAllServersOperationException() : base(Res.GetString("DSSyncAllFailure"))
        {
        }

        public SyncFromAllServersOperationException(string message) : base(message)
        {
        }

        protected SyncFromAllServersOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SyncFromAllServersOperationException(string message, Exception inner) : base(message, inner)
        {
        }

        public SyncFromAllServersOperationException(string message, Exception inner, SyncFromAllServersErrorInformation[] errors) : base(message, inner)
        {
            this.errors = errors;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            base.GetObjectData(serializationInfo, streamingContext);
        }

        public SyncFromAllServersErrorInformation[] ErrorInformation
        {
            get
            {
                if (this.errors == null)
                {
                    return new SyncFromAllServersErrorInformation[0];
                }
                SyncFromAllServersErrorInformation[] informationArray = new SyncFromAllServersErrorInformation[this.errors.Length];
                for (int i = 0; i < this.errors.Length; i++)
                {
                    informationArray[i] = new SyncFromAllServersErrorInformation(this.errors[i].ErrorCategory, this.errors[i].ErrorCode, this.errors[i].ErrorMessage, this.errors[i].SourceServer, this.errors[i].TargetServer);
                }
                return informationArray;
            }
        }
    }
}

