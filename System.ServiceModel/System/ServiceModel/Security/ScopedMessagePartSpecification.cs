namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Xml;

    public class ScopedMessagePartSpecification
    {
        private Dictionary<string, MessagePartSpecification> actionParts;
        private MessagePartSpecification channelParts;
        private bool isReadOnly;
        private Dictionary<string, MessagePartSpecification> readOnlyNormalizedActionParts;

        public ScopedMessagePartSpecification()
        {
            this.channelParts = new MessagePartSpecification();
            this.actionParts = new Dictionary<string, MessagePartSpecification>();
        }

        public ScopedMessagePartSpecification(ScopedMessagePartSpecification other) : this()
        {
            if (other == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("other"));
            }
            this.channelParts.Union(other.channelParts);
            if (other.actionParts != null)
            {
                foreach (string str in other.actionParts.Keys)
                {
                    MessagePartSpecification specification = new MessagePartSpecification();
                    specification.Union(other.actionParts[str]);
                    this.actionParts[str] = specification;
                }
            }
        }

        internal ScopedMessagePartSpecification(ScopedMessagePartSpecification other, bool newIncludeBody) : this(other)
        {
            this.channelParts.IsBodyIncluded = newIncludeBody;
            foreach (string str in this.actionParts.Keys)
            {
                this.actionParts[str].IsBodyIncluded = newIncludeBody;
            }
        }

        public void AddParts(MessagePartSpecification parts)
        {
            if (parts == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("parts"));
            }
            this.ThrowIfReadOnly();
            this.channelParts.Union(parts);
        }

        public void AddParts(MessagePartSpecification parts, string action)
        {
            if (action == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("action"));
            }
            if (parts == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("parts"));
            }
            this.ThrowIfReadOnly();
            if (!this.actionParts.ContainsKey(action))
            {
                this.actionParts[action] = new MessagePartSpecification();
            }
            this.actionParts[action].Union(parts);
        }

        internal void AddParts(MessagePartSpecification parts, XmlDictionaryString action)
        {
            if (action == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("action"));
            }
            this.AddParts(parts, action.Value);
        }

        internal void CopyTo(ScopedMessagePartSpecification target)
        {
            if (target == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("target");
            }
            target.ChannelParts.IsBodyIncluded = this.ChannelParts.IsBodyIncluded;
            foreach (XmlQualifiedName name in this.ChannelParts.HeaderTypes)
            {
                if (!target.channelParts.IsHeaderIncluded(name.Name, name.Namespace))
                {
                    target.ChannelParts.HeaderTypes.Add(name);
                }
            }
            foreach (string str in this.actionParts.Keys)
            {
                target.AddParts(this.actionParts[str], str);
            }
        }

        internal bool IsEmpty()
        {
            if (!this.channelParts.IsEmpty())
            {
                return false;
            }
            foreach (string str in this.Actions)
            {
                MessagePartSpecification specification;
                if (this.TryGetParts(str, true, out specification) && !specification.IsEmpty())
                {
                    return false;
                }
            }
            return true;
        }

        public void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                this.readOnlyNormalizedActionParts = new Dictionary<string, MessagePartSpecification>();
                foreach (string str in this.actionParts.Keys)
                {
                    MessagePartSpecification specification = new MessagePartSpecification();
                    specification.Union(this.actionParts[str]);
                    specification.Union(this.channelParts);
                    specification.MakeReadOnly();
                    this.readOnlyNormalizedActionParts[str] = specification;
                }
                this.isReadOnly = true;
            }
        }

        private void ThrowIfReadOnly()
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
            }
        }

        public bool TryGetParts(string action, out MessagePartSpecification parts)
        {
            return this.TryGetParts(action, false, out parts);
        }

        public bool TryGetParts(string action, bool excludeChannelScope, out MessagePartSpecification parts)
        {
            if (action == null)
            {
                action = "*";
            }
            parts = null;
            if (this.isReadOnly)
            {
                if (this.readOnlyNormalizedActionParts.ContainsKey(action))
                {
                    if (excludeChannelScope)
                    {
                        parts = this.actionParts[action];
                    }
                    else
                    {
                        parts = this.readOnlyNormalizedActionParts[action];
                    }
                }
            }
            else if (this.actionParts.ContainsKey(action))
            {
                MessagePartSpecification specification = new MessagePartSpecification();
                specification.Union(this.actionParts[action]);
                if (!excludeChannelScope)
                {
                    specification.Union(this.channelParts);
                }
                parts = specification;
            }
            return (parts != null);
        }

        public ICollection<string> Actions
        {
            get
            {
                return this.actionParts.Keys;
            }
        }

        public MessagePartSpecification ChannelParts
        {
            get
            {
                return this.channelParts;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }
    }
}

