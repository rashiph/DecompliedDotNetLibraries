namespace System.ComponentModel
{
    using System;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.All)]
    public class PropertyTabAttribute : Attribute
    {
        private Type[] tabClasses;
        private string[] tabClassNames;
        private PropertyTabScope[] tabScopes;

        public PropertyTabAttribute()
        {
            this.tabScopes = new PropertyTabScope[0];
            this.tabClassNames = new string[0];
        }

        public PropertyTabAttribute(string tabClassName) : this(tabClassName, PropertyTabScope.Component)
        {
        }

        public PropertyTabAttribute(Type tabClass) : this(tabClass, PropertyTabScope.Component)
        {
        }

        public PropertyTabAttribute(string tabClassName, PropertyTabScope tabScope)
        {
            this.tabClassNames = new string[] { tabClassName };
            if (tabScope < PropertyTabScope.Document)
            {
                throw new ArgumentException(SR.GetString("PropertyTabAttributeBadPropertyTabScope"), "tabScope");
            }
            this.tabScopes = new PropertyTabScope[] { tabScope };
        }

        public PropertyTabAttribute(Type tabClass, PropertyTabScope tabScope)
        {
            this.tabClasses = new Type[] { tabClass };
            if (tabScope < PropertyTabScope.Document)
            {
                throw new ArgumentException(SR.GetString("PropertyTabAttributeBadPropertyTabScope"), "tabScope");
            }
            this.tabScopes = new PropertyTabScope[] { tabScope };
        }

        public bool Equals(PropertyTabAttribute other)
        {
            if (other != this)
            {
                if ((other.TabClasses.Length != this.TabClasses.Length) || (other.TabScopes.Length != this.TabScopes.Length))
                {
                    return false;
                }
                for (int i = 0; i < this.TabClasses.Length; i++)
                {
                    if ((this.TabClasses[i] != other.TabClasses[i]) || (this.TabScopes[i] != other.TabScopes[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override bool Equals(object other)
        {
            return ((other is PropertyTabAttribute) && this.Equals((PropertyTabAttribute) other));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        protected void InitializeArrays(string[] tabClassNames, PropertyTabScope[] tabScopes)
        {
            this.InitializeArrays(tabClassNames, null, tabScopes);
        }

        protected void InitializeArrays(Type[] tabClasses, PropertyTabScope[] tabScopes)
        {
            this.InitializeArrays(null, tabClasses, tabScopes);
        }

        private void InitializeArrays(string[] tabClassNames, Type[] tabClasses, PropertyTabScope[] tabScopes)
        {
            if (tabClasses != null)
            {
                if ((tabScopes != null) && (tabClasses.Length != tabScopes.Length))
                {
                    throw new ArgumentException(SR.GetString("PropertyTabAttributeArrayLengthMismatch"));
                }
                this.tabClasses = (Type[]) tabClasses.Clone();
            }
            else if (tabClassNames != null)
            {
                if ((tabScopes != null) && (tabClasses.Length != tabScopes.Length))
                {
                    throw new ArgumentException(SR.GetString("PropertyTabAttributeArrayLengthMismatch"));
                }
                this.tabClassNames = (string[]) tabClassNames.Clone();
                this.tabClasses = null;
            }
            else if ((this.tabClasses == null) && (this.tabClassNames == null))
            {
                throw new ArgumentException(SR.GetString("PropertyTabAttributeParamsBothNull"));
            }
            if (tabScopes != null)
            {
                for (int i = 0; i < tabScopes.Length; i++)
                {
                    if (tabScopes[i] < PropertyTabScope.Document)
                    {
                        throw new ArgumentException(SR.GetString("PropertyTabAttributeBadPropertyTabScope"));
                    }
                }
                this.tabScopes = (PropertyTabScope[]) tabScopes.Clone();
            }
            else
            {
                this.tabScopes = new PropertyTabScope[tabClasses.Length];
                for (int j = 0; j < this.TabScopes.Length; j++)
                {
                    this.tabScopes[j] = PropertyTabScope.Component;
                }
            }
        }

        public Type[] TabClasses
        {
            get
            {
                if ((this.tabClasses == null) && (this.tabClassNames != null))
                {
                    this.tabClasses = new Type[this.tabClassNames.Length];
                    for (int i = 0; i < this.tabClassNames.Length; i++)
                    {
                        int index = this.tabClassNames[i].IndexOf(',');
                        string typeName = null;
                        string assemblyString = null;
                        if (index != -1)
                        {
                            typeName = this.tabClassNames[i].Substring(0, index).Trim();
                            assemblyString = this.tabClassNames[i].Substring(index + 1).Trim();
                        }
                        else
                        {
                            typeName = this.tabClassNames[i];
                        }
                        this.tabClasses[i] = Type.GetType(typeName, false);
                        if (this.tabClasses[i] == null)
                        {
                            if (assemblyString == null)
                            {
                                throw new TypeLoadException(SR.GetString("PropertyTabAttributeTypeLoadException", new object[] { typeName }));
                            }
                            Assembly assembly = Assembly.Load(assemblyString);
                            if (assembly != null)
                            {
                                this.tabClasses[i] = assembly.GetType(typeName, true);
                            }
                        }
                    }
                }
                return this.tabClasses;
            }
        }

        protected string[] TabClassNames
        {
            get
            {
                if (this.tabClassNames != null)
                {
                    return (string[]) this.tabClassNames.Clone();
                }
                return null;
            }
        }

        public PropertyTabScope[] TabScopes
        {
            get
            {
                return this.tabScopes;
            }
        }
    }
}

