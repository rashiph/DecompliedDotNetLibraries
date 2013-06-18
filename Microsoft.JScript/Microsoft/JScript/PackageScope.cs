namespace Microsoft.JScript
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal class PackageScope : ActivationObject
    {
        internal string name;
        internal Package owner;

        public PackageScope(ScriptObject parent) : base(parent)
        {
            base.fast = true;
            this.name = null;
            this.owner = null;
            base.isKnownAtCompileTime = true;
        }

        internal override JSVariableField AddNewField(string name, object value, FieldAttributes attributeFlags)
        {
            base.AddNewField(this.name + "." + name, value, attributeFlags);
            return base.AddNewField(name, value, attributeFlags);
        }

        internal void AddOwnName()
        {
            string name = this.name;
            int index = name.IndexOf('.');
            if (index > 0)
            {
                name = name.Substring(0, index);
            }
            base.AddNewField(name, Namespace.GetNamespace(name, base.engine), FieldAttributes.Literal | FieldAttributes.Public);
        }

        protected override JSVariableField CreateField(string name, FieldAttributes attributeFlags, object value)
        {
            return new JSGlobalField(this, name, value, attributeFlags);
        }

        internal override string GetName()
        {
            return this.name;
        }

        internal void MergeWith(PackageScope p)
        {
            foreach (object obj2 in p.field_table)
            {
                JSGlobalField field = (JSGlobalField) obj2;
                ClassScope scope = field.value as ClassScope;
                if (base.name_table[field.Name] != null)
                {
                    if (scope != null)
                    {
                        scope.owner.context.HandleError(JSError.DuplicateName, field.Name, true);
                        scope.owner.name = scope.owner.name + p.GetHashCode().ToString(CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    base.field_table.Add(field);
                    base.name_table[field.Name] = field;
                    if (scope != null)
                    {
                        scope.owner.enclosingScope = this;
                        scope.package = this;
                    }
                }
            }
        }
    }
}

