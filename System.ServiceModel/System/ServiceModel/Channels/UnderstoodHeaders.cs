namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public sealed class UnderstoodHeaders : IEnumerable<MessageHeaderInfo>, IEnumerable
    {
        private MessageHeaders messageHeaders;
        private bool modified;

        internal UnderstoodHeaders(MessageHeaders messageHeaders, bool modified)
        {
            this.messageHeaders = messageHeaders;
            this.modified = modified;
        }

        public void Add(MessageHeaderInfo headerInfo)
        {
            this.messageHeaders.AddUnderstood(headerInfo);
            this.modified = true;
        }

        public bool Contains(MessageHeaderInfo headerInfo)
        {
            return this.messageHeaders.IsUnderstood(headerInfo);
        }

        public IEnumerator<MessageHeaderInfo> GetEnumerator()
        {
            return this.messageHeaders.GetUnderstoodEnumerator();
        }

        public void Remove(MessageHeaderInfo headerInfo)
        {
            this.messageHeaders.RemoveUnderstood(headerInfo);
            this.modified = true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        internal bool Modified
        {
            get
            {
                return this.modified;
            }
            set
            {
                this.modified = value;
            }
        }
    }
}

