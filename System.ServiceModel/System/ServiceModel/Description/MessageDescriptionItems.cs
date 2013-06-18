namespace System.ServiceModel.Description
{
    using System;

    internal class MessageDescriptionItems
    {
        private MessageBodyDescription body;
        private MessageHeaderDescriptionCollection headers;
        private MessagePropertyDescriptionCollection properties;

        internal MessageBodyDescription Body
        {
            get
            {
                if (this.body == null)
                {
                    this.body = new MessageBodyDescription();
                }
                return this.body;
            }
            set
            {
                this.body = value;
            }
        }

        internal MessageHeaderDescriptionCollection Headers
        {
            get
            {
                if (this.headers == null)
                {
                    this.headers = new MessageHeaderDescriptionCollection();
                }
                return this.headers;
            }
        }

        internal MessagePropertyDescriptionCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    this.properties = new MessagePropertyDescriptionCollection();
                }
                return this.properties;
            }
        }
    }
}

