namespace System.ServiceModel
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    public class CorrelationActionMessageFilter : MessageFilter
    {
        private ActionMessageFilter innerFilter;

        public override bool Equals(object other)
        {
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }
            CorrelationActionMessageFilter filter = other as CorrelationActionMessageFilter;
            if (filter == null)
            {
                return false;
            }
            return (this.Action == filter.Action);
        }

        public override int GetHashCode()
        {
            if (this.Action == null)
            {
                return 0;
            }
            return this.Action.GetHashCode();
        }

        private ActionMessageFilter GetInnerFilter()
        {
            if (this.innerFilter == null)
            {
                this.innerFilter = new ActionMessageFilter(new string[] { this.Action });
            }
            return this.innerFilter;
        }

        public override bool Match(Message message)
        {
            if (message == null)
            {
                throw FxTrace.Exception.ArgumentNull("message");
            }
            return this.GetInnerFilter().Match(message);
        }

        public override bool Match(MessageBuffer messageBuffer)
        {
            if (messageBuffer == null)
            {
                throw FxTrace.Exception.ArgumentNull("messageBuffer");
            }
            return this.GetInnerFilter().Match(messageBuffer);
        }

        public override string ToString()
        {
            if (this.Action != null)
            {
                return string.Format(CultureInfo.InvariantCulture, "Action: {0}", new object[] { this.Action });
            }
            return base.ToString();
        }

        public string Action { get; set; }
    }
}

