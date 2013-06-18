namespace System.Web.Hosting
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    internal class HostingEnvironmentException : Exception
    {
        private string _details;

        protected HostingEnvironmentException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._details = info.GetString("_details");
        }

        internal HostingEnvironmentException(string message, string details) : base(message)
        {
            this._details = details;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_details", this._details);
        }

        internal string Details
        {
            get
            {
                if (this._details == null)
                {
                    return string.Empty;
                }
                return this._details;
            }
        }
    }
}

