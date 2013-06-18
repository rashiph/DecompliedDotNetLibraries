namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public class XPathMessageQueryCollection : MessageQueryCollection
    {
        private InverseQueryMatcher matcher = new InverseQueryMatcher(false);

        public override IEnumerable<KeyValuePair<MessageQuery, TResult>> Evaluate<TResult>(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (((typeof(TResult) != typeof(XPathResult)) && (typeof(TResult) != typeof(string))) && ((typeof(TResult) != typeof(bool)) && (typeof(TResult) != typeof(object))))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TResult", System.ServiceModel.SR.GetString("UnsupportedMessageQueryResultType", new object[] { typeof(TResult) }));
            }
            return (IEnumerable<KeyValuePair<MessageQuery, TResult>>) this.matcher.Evaluate<TResult>(message, false);
        }

        public override IEnumerable<KeyValuePair<MessageQuery, TResult>> Evaluate<TResult>(MessageBuffer buffer)
        {
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
            }
            if (((typeof(TResult) != typeof(XPathResult)) && (typeof(TResult) != typeof(string))) && ((typeof(TResult) != typeof(bool)) && (typeof(TResult) != typeof(object))))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TResult", System.ServiceModel.SR.GetString("UnsupportedMessageQueryResultType", new object[] { typeof(TResult) }));
            }
            return (IEnumerable<KeyValuePair<MessageQuery, TResult>>) this.matcher.Evaluate<TResult>(buffer);
        }

        protected override void InsertItem(int index, MessageQuery item)
        {
            if (item == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }
            if (!(item is XPathMessageQuery))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("item");
            }
            base.InsertItem(index, item);
            XPathMessageQuery query = (XPathMessageQuery) item;
            this.matcher.Add(query.Expression, query.Namespaces, query, false);
        }

        protected override void RemoveItem(int index)
        {
            this.matcher.Remove((XPathMessageQuery) base[index]);
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, MessageQuery item)
        {
            if (!(item is XPathMessageQuery))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("item");
            }
            this.matcher.Remove((XPathMessageQuery) base[index]);
            XPathMessageQuery query = (XPathMessageQuery) item;
            base.SetItem(index, item);
            this.matcher.Add(query.Expression, query.Namespaces, query, false);
        }
    }
}

