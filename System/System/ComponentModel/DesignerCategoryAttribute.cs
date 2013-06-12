namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    public sealed class DesignerCategoryAttribute : Attribute
    {
        private string category;
        public static readonly DesignerCategoryAttribute Component = new DesignerCategoryAttribute("Component");
        public static readonly DesignerCategoryAttribute Default = new DesignerCategoryAttribute();
        public static readonly DesignerCategoryAttribute Form = new DesignerCategoryAttribute("Form");
        public static readonly DesignerCategoryAttribute Generic = new DesignerCategoryAttribute("Designer");
        private string typeId;

        public DesignerCategoryAttribute()
        {
            this.category = string.Empty;
        }

        public DesignerCategoryAttribute(string category)
        {
            this.category = category;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            DesignerCategoryAttribute attribute = obj as DesignerCategoryAttribute;
            return ((attribute != null) && (attribute.category == this.category));
        }

        public override int GetHashCode()
        {
            return this.category.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return this.category.Equals(Default.Category);
        }

        public string Category
        {
            get
            {
                return this.category;
            }
        }

        public override object TypeId
        {
            get
            {
                if (this.typeId == null)
                {
                    this.typeId = base.GetType().FullName + this.Category;
                }
                return this.typeId;
            }
        }
    }
}

