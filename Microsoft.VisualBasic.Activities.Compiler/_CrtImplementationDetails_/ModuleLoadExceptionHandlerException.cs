namespace <CrtImplementationDetails>
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable]
    internal class ModuleLoadExceptionHandlerException : ModuleLoadException
    {
        private Exception <backing_store>NestedException;
        private const string formatString = "\n{0}: {1}\n--- Start of primary exception ---\n{2}\n--- End of primary exception ---\n\n--- Start of nested exception ---\n{3}\n--- End of nested exception ---\n";

        protected ModuleLoadExceptionHandlerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.NestedException = (Exception) info.GetValue("NestedException", typeof(Exception));
        }

        public ModuleLoadExceptionHandlerException(string message, Exception innerException, Exception nestedException) : base(message, innerException)
        {
            this.NestedException = nestedException;
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("NestedException", this.NestedException, typeof(Exception));
        }

        public override string ToString()
        {
            string str;
            string str2;
            string str3;
            string str4;
            string message;
            if (this.InnerException != null)
            {
                str2 = this.InnerException.ToString();
            }
            else
            {
                str2 = string.Empty;
            }
            if (this.NestedException != null)
            {
                str = this.NestedException.ToString();
            }
            else
            {
                str = string.Empty;
            }
            object[] args = new object[4];
            args[0] = this.GetType();
            if (this.Message != null)
            {
                message = this.Message;
            }
            else
            {
                message = string.Empty;
            }
            args[1] = message;
            if (str2 != null)
            {
                str4 = str2;
            }
            else
            {
                str4 = string.Empty;
            }
            args[2] = str4;
            if (str != null)
            {
                str3 = str;
            }
            else
            {
                str3 = string.Empty;
            }
            args[3] = str3;
            return string.Format("\n{0}: {1}\n--- Start of primary exception ---\n{2}\n--- End of primary exception ---\n\n--- Start of nested exception ---\n{3}\n--- End of nested exception ---\n", args);
        }

        public Exception NestedException
        {
            get
            {
                return this.<backing_store>NestedException;
            }
            set
            {
                this.<backing_store>NestedException = value;
            }
        }
    }
}

