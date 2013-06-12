namespace System.ComponentModel
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    [ComVisible(true), HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public abstract class MemberDescriptor
    {
        private AttributeCollection attributeCollection;
        private Attribute[] attributes;
        private bool attributesFilled;
        private bool attributesFiltered;
        private string category;
        private string description;
        private string displayName;
        private object lockCookie;
        private int metadataVersion;
        private string name;
        private int nameHash;
        private Attribute[] originalAttributes;

        protected MemberDescriptor(MemberDescriptor descr)
        {
            this.lockCookie = new object();
            this.name = descr.Name;
            this.displayName = this.name;
            this.nameHash = this.name.GetHashCode();
            this.attributes = new Attribute[descr.Attributes.Count];
            descr.Attributes.CopyTo(this.attributes, 0);
            this.attributesFiltered = true;
            this.originalAttributes = this.attributes;
        }

        protected MemberDescriptor(string name) : this(name, null)
        {
        }

        protected MemberDescriptor(MemberDescriptor oldMemberDescriptor, Attribute[] newAttributes)
        {
            this.lockCookie = new object();
            this.name = oldMemberDescriptor.Name;
            this.displayName = oldMemberDescriptor.DisplayName;
            this.nameHash = this.name.GetHashCode();
            ArrayList list = new ArrayList();
            if (oldMemberDescriptor.Attributes.Count != 0)
            {
                foreach (object obj2 in oldMemberDescriptor.Attributes)
                {
                    list.Add(obj2);
                }
            }
            if (newAttributes != null)
            {
                Attribute[] attributeArray = newAttributes;
                for (int i = 0; i < attributeArray.Length; i++)
                {
                    object obj3 = attributeArray[i];
                    list.Add(obj3);
                }
            }
            this.attributes = new Attribute[list.Count];
            list.CopyTo(this.attributes, 0);
            this.attributesFiltered = false;
            this.originalAttributes = this.attributes;
        }

        protected MemberDescriptor(string name, Attribute[] attributes)
        {
            this.lockCookie = new object();
            try
            {
                if ((name == null) || (name.Length == 0))
                {
                    throw new ArgumentException(SR.GetString("InvalidMemberName"));
                }
                this.name = name;
                this.displayName = name;
                this.nameHash = name.GetHashCode();
                if (attributes != null)
                {
                    this.attributes = attributes;
                    this.attributesFiltered = false;
                }
                this.originalAttributes = this.attributes;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        private void CheckAttributesValid()
        {
            if (this.attributesFiltered && (this.metadataVersion != TypeDescriptor.MetadataVersion))
            {
                this.attributesFilled = false;
                this.attributesFiltered = false;
                this.attributeCollection = null;
            }
        }

        protected virtual AttributeCollection CreateAttributeCollection()
        {
            return new AttributeCollection(this.AttributeArray);
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != base.GetType())
            {
                return false;
            }
            MemberDescriptor descriptor = (MemberDescriptor) obj;
            this.FilterAttributesIfNeeded();
            descriptor.FilterAttributesIfNeeded();
            if (descriptor.nameHash != this.nameHash)
            {
                return false;
            }
            if (((descriptor.category == null) != (this.category == null)) || ((this.category != null) && !descriptor.category.Equals(this.category)))
            {
                return false;
            }
            if (((descriptor.description == null) != (this.description == null)) || ((this.description != null) && !descriptor.category.Equals(this.description)))
            {
                return false;
            }
            if ((descriptor.attributes == null) != (this.attributes == null))
            {
                return false;
            }
            if (this.attributes != null)
            {
                if (this.attributes.Length != descriptor.attributes.Length)
                {
                    return false;
                }
                for (int i = 0; i < this.attributes.Length; i++)
                {
                    if (!this.attributes[i].Equals(descriptor.attributes[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        protected virtual void FillAttributes(IList attributeList)
        {
            if (this.originalAttributes != null)
            {
                foreach (Attribute attribute in this.originalAttributes)
                {
                    attributeList.Add(attribute);
                }
            }
        }

        private void FilterAttributesIfNeeded()
        {
            IList list;
            Hashtable hashtable;
            if (this.attributesFiltered)
            {
                return;
            }
            if (!this.attributesFilled)
            {
                list = new ArrayList();
                try
                {
                    this.FillAttributes(list);
                    goto Label_0034;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception)
                {
                    goto Label_0034;
                }
            }
            list = new ArrayList(this.attributes);
        Label_0034:
            hashtable = new Hashtable(list.Count);
            foreach (Attribute attribute in list)
            {
                hashtable[attribute.TypeId] = attribute;
            }
            Attribute[] array = new Attribute[hashtable.Values.Count];
            hashtable.Values.CopyTo(array, 0);
            lock (this.lockCookie)
            {
                this.attributes = array;
                this.attributesFiltered = true;
                this.attributesFilled = true;
                this.metadataVersion = TypeDescriptor.MetadataVersion;
            }
        }

        protected static MethodInfo FindMethod(Type componentClass, string name, Type[] args, Type returnType)
        {
            return FindMethod(componentClass, name, args, returnType, true);
        }

        protected static MethodInfo FindMethod(Type componentClass, string name, Type[] args, Type returnType, bool publicOnly)
        {
            MethodInfo method = null;
            if (publicOnly)
            {
                method = componentClass.GetMethod(name, args);
            }
            else
            {
                method = componentClass.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, args, null);
            }
            if ((method != null) && !method.ReturnType.IsEquivalentTo(returnType))
            {
                method = null;
            }
            return method;
        }

        public override int GetHashCode()
        {
            return this.nameHash;
        }

        protected virtual object GetInvocationTarget(Type type, object instance)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            return TypeDescriptor.GetAssociation(type, instance);
        }

        [Obsolete("This method has been deprecated. Use GetInvocationTarget instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        protected static object GetInvokee(Type componentClass, object component)
        {
            if (componentClass == null)
            {
                throw new ArgumentNullException("componentClass");
            }
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            return TypeDescriptor.GetAssociation(componentClass, component);
        }

        protected static ISite GetSite(object component)
        {
            if (component is IComponent)
            {
                return ((IComponent) component).Site;
            }
            return null;
        }

        protected virtual Attribute[] AttributeArray
        {
            get
            {
                this.CheckAttributesValid();
                this.FilterAttributesIfNeeded();
                return this.attributes;
            }
            set
            {
                lock (this.lockCookie)
                {
                    this.attributes = value;
                    this.originalAttributes = value;
                    this.attributesFiltered = false;
                    this.attributeCollection = null;
                }
            }
        }

        public virtual AttributeCollection Attributes
        {
            get
            {
                this.CheckAttributesValid();
                AttributeCollection attributeCollection = this.attributeCollection;
                if (attributeCollection == null)
                {
                    lock (this.lockCookie)
                    {
                        attributeCollection = this.CreateAttributeCollection();
                        this.attributeCollection = attributeCollection;
                    }
                }
                return attributeCollection;
            }
        }

        public virtual string Category
        {
            get
            {
                if (this.category == null)
                {
                    this.category = ((CategoryAttribute) this.Attributes[typeof(CategoryAttribute)]).Category;
                }
                return this.category;
            }
        }

        public virtual string Description
        {
            get
            {
                if (this.description == null)
                {
                    this.description = ((DescriptionAttribute) this.Attributes[typeof(DescriptionAttribute)]).Description;
                }
                return this.description;
            }
        }

        public virtual bool DesignTimeOnly
        {
            get
            {
                return DesignOnlyAttribute.Yes.Equals(this.Attributes[typeof(DesignOnlyAttribute)]);
            }
        }

        public virtual string DisplayName
        {
            get
            {
                DisplayNameAttribute attribute = this.Attributes[typeof(DisplayNameAttribute)] as DisplayNameAttribute;
                if ((attribute != null) && !attribute.IsDefaultAttribute())
                {
                    return attribute.DisplayName;
                }
                return this.displayName;
            }
        }

        public virtual bool IsBrowsable
        {
            get
            {
                return ((BrowsableAttribute) this.Attributes[typeof(BrowsableAttribute)]).Browsable;
            }
        }

        public virtual string Name
        {
            get
            {
                if (this.name == null)
                {
                    return "";
                }
                return this.name;
            }
        }

        protected virtual int NameHashCode
        {
            get
            {
                return this.nameHash;
            }
        }
    }
}

