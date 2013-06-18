namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel;

    internal class SendSecurityHeaderElement
    {
        private string id;
        private ISecurityElement item;
        private bool markedForEncryption;

        public SendSecurityHeaderElement(string id, ISecurityElement item)
        {
            this.id = id;
            this.item = item;
            this.markedForEncryption = false;
        }

        public bool IsSameItem(ISecurityElement item)
        {
            if (this.item != item)
            {
                return this.item.Equals(item);
            }
            return true;
        }

        public void Replace(string id, ISecurityElement item)
        {
            this.item = item;
            this.id = id;
        }

        public string Id
        {
            get
            {
                return this.id;
            }
        }

        public ISecurityElement Item
        {
            get
            {
                return this.item;
            }
        }

        public bool MarkedForEncryption
        {
            get
            {
                return this.markedForEncryption;
            }
            set
            {
                this.markedForEncryption = value;
            }
        }
    }
}

