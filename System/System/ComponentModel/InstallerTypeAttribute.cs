namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class InstallerTypeAttribute : Attribute
    {
        private string _typeName;

        public InstallerTypeAttribute(string typeName)
        {
            this._typeName = typeName;
        }

        public InstallerTypeAttribute(Type installerType)
        {
            this._typeName = installerType.AssemblyQualifiedName;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            InstallerTypeAttribute attribute = obj as InstallerTypeAttribute;
            return ((attribute != null) && (attribute._typeName == this._typeName));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public virtual Type InstallerType
        {
            get
            {
                return Type.GetType(this._typeName);
            }
        }
    }
}

