namespace System.Net
{
    using System;
    using System.Collections.Specialized;
    using System.Reflection;

    internal class TrackingStringDictionary : StringDictionary
    {
        private bool isChanged;
        private bool isReadOnly;

        internal TrackingStringDictionary() : this(false)
        {
        }

        internal TrackingStringDictionary(bool isReadOnly)
        {
            this.isReadOnly = isReadOnly;
        }

        public override void Add(string key, string value)
        {
            if (this.isReadOnly)
            {
                throw new InvalidOperationException(SR.GetString("MailCollectionIsReadOnly"));
            }
            base.Add(key, value);
            this.isChanged = true;
        }

        public override void Clear()
        {
            if (this.isReadOnly)
            {
                throw new InvalidOperationException(SR.GetString("MailCollectionIsReadOnly"));
            }
            base.Clear();
            this.isChanged = true;
        }

        public override void Remove(string key)
        {
            if (this.isReadOnly)
            {
                throw new InvalidOperationException(SR.GetString("MailCollectionIsReadOnly"));
            }
            base.Remove(key);
            this.isChanged = true;
        }

        internal bool IsChanged
        {
            get
            {
                return this.isChanged;
            }
            set
            {
                this.isChanged = value;
            }
        }

        public override string this[string key]
        {
            get
            {
                return base[key];
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw new InvalidOperationException(SR.GetString("MailCollectionIsReadOnly"));
                }
                base[key] = value;
                this.isChanged = true;
            }
        }
    }
}

