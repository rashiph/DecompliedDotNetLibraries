namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    public class MessagePartSpecification
    {
        private List<XmlQualifiedName> headerTypes;
        private bool isBodyIncluded;
        private bool isReadOnly;
        private static MessagePartSpecification noParts;

        public MessagePartSpecification()
        {
        }

        public MessagePartSpecification(bool isBodyIncluded)
        {
            this.isBodyIncluded = isBodyIncluded;
        }

        public MessagePartSpecification(params XmlQualifiedName[] headerTypes) : this(false, headerTypes)
        {
        }

        public MessagePartSpecification(bool isBodyIncluded, params XmlQualifiedName[] headerTypes)
        {
            this.isBodyIncluded = isBodyIncluded;
            if ((headerTypes != null) && (headerTypes.Length > 0))
            {
                this.headerTypes = new List<XmlQualifiedName>(headerTypes.Length);
                for (int i = 0; i < headerTypes.Length; i++)
                {
                    this.headerTypes.Add(headerTypes[i]);
                }
            }
        }

        public void Clear()
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
            }
            if (this.headerTypes != null)
            {
                this.headerTypes.Clear();
            }
            this.isBodyIncluded = false;
        }

        internal bool IsEmpty()
        {
            if ((this.headerTypes != null) && (this.headerTypes.Count > 0))
            {
                return false;
            }
            return !this.IsBodyIncluded;
        }

        internal bool IsHeaderIncluded(MessageHeader header)
        {
            if (header == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("header");
            }
            return this.IsHeaderIncluded(header.Name, header.Namespace);
        }

        internal bool IsHeaderIncluded(string name, string ns)
        {
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }
            if (ns == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ns");
            }
            if (this.headerTypes != null)
            {
                for (int i = 0; i < this.headerTypes.Count; i++)
                {
                    XmlQualifiedName name2 = this.headerTypes[i];
                    if (string.IsNullOrEmpty(name2.Name))
                    {
                        if (name2.Namespace == ns)
                        {
                            return true;
                        }
                    }
                    else if ((name2.Name == name) && (name2.Namespace == ns))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                if (this.headerTypes != null)
                {
                    List<XmlQualifiedName> list = new List<XmlQualifiedName>(this.headerTypes.Count);
                    for (int i = 0; i < this.headerTypes.Count; i++)
                    {
                        XmlQualifiedName item = this.headerTypes[i];
                        if (item != null)
                        {
                            bool flag = true;
                            for (int j = 0; j < list.Count; j++)
                            {
                                XmlQualifiedName name2 = list[j];
                                if ((item.Name == name2.Name) && (item.Namespace == name2.Namespace))
                                {
                                    flag = false;
                                    break;
                                }
                            }
                            if (flag)
                            {
                                list.Add(item);
                            }
                        }
                    }
                    this.headerTypes = list;
                }
                this.isReadOnly = true;
            }
        }

        public void Union(MessagePartSpecification specification)
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
            }
            if (specification == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("specification");
            }
            this.isBodyIncluded |= specification.IsBodyIncluded;
            List<XmlQualifiedName> headerTypes = specification.headerTypes;
            if ((headerTypes != null) && (headerTypes.Count > 0))
            {
                if (this.headerTypes == null)
                {
                    this.headerTypes = new List<XmlQualifiedName>(headerTypes.Count);
                }
                for (int i = 0; i < headerTypes.Count; i++)
                {
                    XmlQualifiedName item = headerTypes[i];
                    this.headerTypes.Add(item);
                }
            }
        }

        internal bool HasHeaders
        {
            get
            {
                return ((this.headerTypes != null) && (this.headerTypes.Count > 0));
            }
        }

        public ICollection<XmlQualifiedName> HeaderTypes
        {
            get
            {
                if (this.headerTypes == null)
                {
                    this.headerTypes = new List<XmlQualifiedName>();
                }
                if (this.isReadOnly)
                {
                    return new ReadOnlyCollection<XmlQualifiedName>(this.headerTypes);
                }
                return this.headerTypes;
            }
        }

        public bool IsBodyIncluded
        {
            get
            {
                return this.isBodyIncluded;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.isBodyIncluded = value;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }

        public static MessagePartSpecification NoParts
        {
            get
            {
                if (noParts == null)
                {
                    MessagePartSpecification specification = new MessagePartSpecification();
                    specification.MakeReadOnly();
                    noParts = specification;
                }
                return noParts;
            }
        }
    }
}

