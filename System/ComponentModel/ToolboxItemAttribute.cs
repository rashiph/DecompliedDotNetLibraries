namespace System.ComponentModel
{
    using System;
    using System.Globalization;

    [AttributeUsage(AttributeTargets.All)]
    public class ToolboxItemAttribute : Attribute
    {
        public static readonly ToolboxItemAttribute Default = new ToolboxItemAttribute("System.Drawing.Design.ToolboxItem, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
        public static readonly ToolboxItemAttribute None = new ToolboxItemAttribute(false);
        private Type toolboxItemType;
        private string toolboxItemTypeName;

        public ToolboxItemAttribute(bool defaultType)
        {
            if (defaultType)
            {
                this.toolboxItemTypeName = "System.Drawing.Design.ToolboxItem, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
            }
        }

        public ToolboxItemAttribute(string toolboxItemTypeName)
        {
            toolboxItemTypeName.ToUpper(CultureInfo.InvariantCulture);
            this.toolboxItemTypeName = toolboxItemTypeName;
        }

        public ToolboxItemAttribute(Type toolboxItemType)
        {
            this.toolboxItemType = toolboxItemType;
            this.toolboxItemTypeName = toolboxItemType.AssemblyQualifiedName;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            ToolboxItemAttribute attribute = obj as ToolboxItemAttribute;
            return ((attribute != null) && (attribute.ToolboxItemTypeName == this.ToolboxItemTypeName));
        }

        public override int GetHashCode()
        {
            if (this.toolboxItemTypeName != null)
            {
                return this.toolboxItemTypeName.GetHashCode();
            }
            return base.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return this.Equals(Default);
        }

        public Type ToolboxItemType
        {
            get
            {
                if ((this.toolboxItemType == null) && (this.toolboxItemTypeName != null))
                {
                    try
                    {
                        this.toolboxItemType = Type.GetType(this.toolboxItemTypeName, true);
                    }
                    catch (Exception exception)
                    {
                        throw new ArgumentException(SR.GetString("ToolboxItemAttributeFailedGetType", new object[] { this.toolboxItemTypeName }), exception);
                    }
                }
                return this.toolboxItemType;
            }
        }

        public string ToolboxItemTypeName
        {
            get
            {
                if (this.toolboxItemTypeName == null)
                {
                    return string.Empty;
                }
                return this.toolboxItemTypeName;
            }
        }
    }
}

