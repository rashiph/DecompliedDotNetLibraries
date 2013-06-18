namespace System.Security.Cryptography.Pkcs
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class CmsRecipientCollection : ICollection, IEnumerable
    {
        private ArrayList m_recipients;

        public CmsRecipientCollection()
        {
            this.m_recipients = new ArrayList();
        }

        public CmsRecipientCollection(CmsRecipient recipient)
        {
            this.m_recipients = new ArrayList(1);
            this.m_recipients.Add(recipient);
        }

        public CmsRecipientCollection(SubjectIdentifierType recipientIdentifierType, X509Certificate2Collection certificates)
        {
            this.m_recipients = new ArrayList(certificates.Count);
            for (int i = 0; i < certificates.Count; i++)
            {
                this.m_recipients.Add(new CmsRecipient(recipientIdentifierType, certificates[i]));
            }
        }

        public int Add(CmsRecipient recipient)
        {
            if (recipient == null)
            {
                throw new ArgumentNullException("recipient");
            }
            return this.m_recipients.Add(recipient);
        }

        public void CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (array.Rank != 1)
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Arg_RankMultiDimNotSupported"));
            }
            if ((index < 0) || (index >= array.Length))
            {
                throw new ArgumentOutOfRangeException("index", SecurityResources.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if ((index + this.Count) > array.Length)
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Argument_InvalidOffLen"));
            }
            for (int i = 0; i < this.Count; i++)
            {
                array.SetValue(this[i], index);
                index++;
            }
        }

        public void CopyTo(CmsRecipient[] array, int index)
        {
            this.CopyTo(array, index);
        }

        public CmsRecipientEnumerator GetEnumerator()
        {
            return new CmsRecipientEnumerator(this);
        }

        public void Remove(CmsRecipient recipient)
        {
            if (recipient == null)
            {
                throw new ArgumentNullException("recipient");
            }
            this.m_recipients.Remove(recipient);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new CmsRecipientEnumerator(this);
        }

        public int Count
        {
            get
            {
                return this.m_recipients.Count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public CmsRecipient this[int index]
        {
            get
            {
                if ((index < 0) || (index >= this.m_recipients.Count))
                {
                    throw new ArgumentOutOfRangeException("index", SecurityResources.GetResourceString("ArgumentOutOfRange_Index"));
                }
                return (CmsRecipient) this.m_recipients[index];
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }
    }
}

