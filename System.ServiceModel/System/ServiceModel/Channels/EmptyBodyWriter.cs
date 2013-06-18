namespace System.ServiceModel.Channels
{
    using System;
    using System.Xml;

    internal class EmptyBodyWriter : BodyWriter
    {
        private static EmptyBodyWriter value;

        private EmptyBodyWriter() : base(true)
        {
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
        }

        internal override bool IsEmpty
        {
            get
            {
                return true;
            }
        }

        public static EmptyBodyWriter Value
        {
            get
            {
                if (value == null)
                {
                    value = new EmptyBodyWriter();
                }
                return value;
            }
        }
    }
}

