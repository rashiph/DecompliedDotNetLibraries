namespace System.ServiceModel.Channels
{
    using System;

    internal class NullMessage : StringMessage
    {
        public NullMessage() : base(string.Empty)
        {
        }
    }
}

