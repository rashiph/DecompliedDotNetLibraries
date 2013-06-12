namespace System.Web.Compilation
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public sealed class ExpressionEditorAttribute : Attribute
    {
        private string _editorTypeName;

        public ExpressionEditorAttribute(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentNullException("typeName");
            }
            this._editorTypeName = typeName;
        }

        public ExpressionEditorAttribute(Type type) : this((type != null) ? type.AssemblyQualifiedName : null)
        {
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            ExpressionEditorAttribute attribute = obj as ExpressionEditorAttribute;
            return ((attribute != null) && (attribute.EditorTypeName == this.EditorTypeName));
        }

        public override int GetHashCode()
        {
            return this.EditorTypeName.GetHashCode();
        }

        public string EditorTypeName
        {
            get
            {
                return this._editorTypeName;
            }
        }
    }
}

