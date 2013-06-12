namespace System.ComponentModel
{
    using System;
    using System.ComponentModel.Design;
    using System.Globalization;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple=true, Inherited=true)]
    public sealed class DesignerAttribute : Attribute
    {
        private readonly string designerBaseTypeName;
        private readonly string designerTypeName;
        private string typeId;

        public DesignerAttribute(string designerTypeName)
        {
            designerTypeName.ToUpper(CultureInfo.InvariantCulture);
            this.designerTypeName = designerTypeName;
            this.designerBaseTypeName = typeof(IDesigner).FullName;
        }

        public DesignerAttribute(Type designerType)
        {
            this.designerTypeName = designerType.AssemblyQualifiedName;
            this.designerBaseTypeName = typeof(IDesigner).FullName;
        }

        public DesignerAttribute(string designerTypeName, string designerBaseTypeName)
        {
            designerTypeName.ToUpper(CultureInfo.InvariantCulture);
            this.designerTypeName = designerTypeName;
            this.designerBaseTypeName = designerBaseTypeName;
        }

        public DesignerAttribute(string designerTypeName, Type designerBaseType)
        {
            designerTypeName.ToUpper(CultureInfo.InvariantCulture);
            this.designerTypeName = designerTypeName;
            this.designerBaseTypeName = designerBaseType.AssemblyQualifiedName;
        }

        public DesignerAttribute(Type designerType, Type designerBaseType)
        {
            this.designerTypeName = designerType.AssemblyQualifiedName;
            this.designerBaseTypeName = designerBaseType.AssemblyQualifiedName;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            DesignerAttribute attribute = obj as DesignerAttribute;
            return (((attribute != null) && (attribute.designerBaseTypeName == this.designerBaseTypeName)) && (attribute.designerTypeName == this.designerTypeName));
        }

        public override int GetHashCode()
        {
            return (this.designerTypeName.GetHashCode() ^ this.designerBaseTypeName.GetHashCode());
        }

        public string DesignerBaseTypeName
        {
            get
            {
                return this.designerBaseTypeName;
            }
        }

        public string DesignerTypeName
        {
            get
            {
                return this.designerTypeName;
            }
        }

        public override object TypeId
        {
            get
            {
                if (this.typeId == null)
                {
                    string designerBaseTypeName = this.designerBaseTypeName;
                    int index = designerBaseTypeName.IndexOf(',');
                    if (index != -1)
                    {
                        designerBaseTypeName = designerBaseTypeName.Substring(0, index);
                    }
                    this.typeId = base.GetType().FullName + designerBaseTypeName;
                }
                return this.typeId;
            }
        }
    }
}

