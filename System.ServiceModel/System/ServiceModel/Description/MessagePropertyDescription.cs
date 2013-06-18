namespace System.ServiceModel.Description
{
    using System;

    public class MessagePropertyDescription : MessagePartDescription
    {
        internal MessagePropertyDescription(MessagePropertyDescription other) : base(other)
        {
        }

        public MessagePropertyDescription(string name) : base(name, "")
        {
        }

        internal override MessagePartDescription Clone()
        {
            return new MessagePropertyDescription(this);
        }
    }
}

