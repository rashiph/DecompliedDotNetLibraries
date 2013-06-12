namespace System.Net.Mail
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Text;

    public class MailAddressCollection : Collection<MailAddress>
    {
        public void Add(string addresses)
        {
            if (addresses == null)
            {
                throw new ArgumentNullException("addresses");
            }
            if (addresses == string.Empty)
            {
                throw new ArgumentException(SR.GetString("net_emptystringcall", new object[] { "addresses" }), "addresses");
            }
            this.ParseValue(addresses);
        }

        internal string Encode(int charsConsumed)
        {
            string str = string.Empty;
            foreach (MailAddress address in this)
            {
                if (string.IsNullOrEmpty(str))
                {
                    str = address.Encode(charsConsumed);
                }
                else
                {
                    str = str + ",\r\n " + address.Encode(1);
                }
            }
            return str;
        }

        protected override void InsertItem(int index, MailAddress item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            base.InsertItem(index, item);
        }

        internal void ParseValue(string addresses)
        {
            IList<MailAddress> list = MailAddressParser.ParseMultipleAddresses(addresses);
            for (int i = 0; i < list.Count; i++)
            {
                base.Add(list[i]);
            }
        }

        protected override void SetItem(int index, MailAddress item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            base.SetItem(index, item);
        }

        public override string ToString()
        {
            bool flag = true;
            StringBuilder builder = new StringBuilder();
            foreach (MailAddress address in this)
            {
                if (!flag)
                {
                    builder.Append(", ");
                }
                builder.Append(address.ToString());
                flag = false;
            }
            return builder.ToString();
        }
    }
}

