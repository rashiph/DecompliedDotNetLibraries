namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public sealed class ExceptionCollection : Exception
    {
        private ArrayList exceptions;

        public ExceptionCollection(ArrayList exceptions)
        {
            this.exceptions = exceptions;
        }

        private ExceptionCollection(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.exceptions = (ArrayList) info.GetValue("exceptions", typeof(ArrayList));
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("exceptions", this.exceptions);
            base.GetObjectData(info, context);
        }

        public ArrayList Exceptions
        {
            get
            {
                if (this.exceptions != null)
                {
                    return (ArrayList) this.exceptions.Clone();
                }
                return null;
            }
        }
    }
}

