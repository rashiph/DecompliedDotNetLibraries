namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public sealed class TrackingProfileDeserializationException : SystemException
    {
        private List<System.Xml.Schema.ValidationEventArgs> _args;

        public TrackingProfileDeserializationException()
        {
            this._args = new List<System.Xml.Schema.ValidationEventArgs>();
        }

        public TrackingProfileDeserializationException(string message) : base(message)
        {
            this._args = new List<System.Xml.Schema.ValidationEventArgs>();
        }

        private TrackingProfileDeserializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._args = new List<System.Xml.Schema.ValidationEventArgs>();
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            this._args = (List<System.Xml.Schema.ValidationEventArgs>) info.GetValue("__TrackingProfileDeserializationException_args__", typeof(List<System.Xml.Schema.ValidationEventArgs>));
        }

        public TrackingProfileDeserializationException(string message, Exception innerException) : base(message, innerException)
        {
            this._args = new List<System.Xml.Schema.ValidationEventArgs>();
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("__TrackingProfileDeserializationException_args__", this._args);
        }

        public IList<System.Xml.Schema.ValidationEventArgs> ValidationEventArgs
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._args;
            }
        }
    }
}

