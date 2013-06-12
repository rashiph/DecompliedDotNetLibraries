namespace System.Web.UI
{
    using System;
    using System.Web.Util;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ParseChildrenAttribute : Attribute
    {
        private bool _allowChanges;
        private Type _childControlType;
        private bool _childrenAsProps;
        private string _defaultProperty;
        public static readonly ParseChildrenAttribute Default = ParseAsChildren;
        public static readonly ParseChildrenAttribute ParseAsChildren = new ParseChildrenAttribute(false, false);
        public static readonly ParseChildrenAttribute ParseAsProperties = new ParseChildrenAttribute(true, false);

        public ParseChildrenAttribute() : this(false, (string) null)
        {
        }

        public ParseChildrenAttribute(bool childrenAsProperties) : this(childrenAsProperties, (string) null)
        {
        }

        public ParseChildrenAttribute(Type childControlType) : this(false, (string) null)
        {
            if (childControlType == null)
            {
                throw new ArgumentNullException("childControlType");
            }
            this._childControlType = childControlType;
        }

        private ParseChildrenAttribute(bool childrenAsProperties, bool allowChanges) : this(childrenAsProperties, (string) null)
        {
            this._allowChanges = allowChanges;
        }

        public ParseChildrenAttribute(bool childrenAsProperties, string defaultProperty)
        {
            this._allowChanges = true;
            this._childrenAsProps = childrenAsProperties;
            if (this._childrenAsProps)
            {
                this._defaultProperty = defaultProperty;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            ParseChildrenAttribute attribute = obj as ParseChildrenAttribute;
            if (attribute == null)
            {
                return false;
            }
            if (!this._childrenAsProps)
            {
                return (!attribute.ChildrenAsProperties && (attribute._childControlType == this._childControlType));
            }
            return (attribute.ChildrenAsProperties && this.DefaultProperty.Equals(attribute.DefaultProperty));
        }

        public override int GetHashCode()
        {
            if (!this._childrenAsProps)
            {
                return HashCodeCombiner.CombineHashCodes(this._childrenAsProps.GetHashCode(), this._childControlType.GetHashCode());
            }
            return HashCodeCombiner.CombineHashCodes(this._childrenAsProps.GetHashCode(), this.DefaultProperty.GetHashCode());
        }

        public override bool IsDefaultAttribute()
        {
            return this.Equals(Default);
        }

        public Type ChildControlType
        {
            get
            {
                if (this._childControlType == null)
                {
                    return typeof(Control);
                }
                return this._childControlType;
            }
        }

        public bool ChildrenAsProperties
        {
            get
            {
                return this._childrenAsProps;
            }
            set
            {
                if (!this._allowChanges)
                {
                    throw new NotSupportedException();
                }
                this._childrenAsProps = value;
            }
        }

        public string DefaultProperty
        {
            get
            {
                if (this._defaultProperty == null)
                {
                    return string.Empty;
                }
                return this._defaultProperty;
            }
            set
            {
                if (!this._allowChanges)
                {
                    throw new NotSupportedException();
                }
                this._defaultProperty = value;
            }
        }
    }
}

