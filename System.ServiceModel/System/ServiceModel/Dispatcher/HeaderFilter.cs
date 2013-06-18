namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal abstract class HeaderFilter : MessageFilter
    {
        protected HeaderFilter()
        {
        }

        public override bool Match(MessageBuffer buffer)
        {
            bool flag;
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
            }
            Message message = buffer.CreateMessage();
            try
            {
                flag = this.Match(message);
            }
            finally
            {
                message.Close();
            }
            return flag;
        }
    }
}

