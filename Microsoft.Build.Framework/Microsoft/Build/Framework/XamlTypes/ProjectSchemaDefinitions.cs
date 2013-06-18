namespace Microsoft.Build.Framework.XamlTypes
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Markup;

    [ContentProperty("Nodes")]
    public sealed class ProjectSchemaDefinitions : IProjectSchemaNode
    {
        public ProjectSchemaDefinitions()
        {
            this.Nodes = new List<IProjectSchemaNode>();
        }

        public IEnumerable<object> GetSchemaObjects(Type type)
        {
            foreach (IProjectSchemaNode iteratorVariable0 in this.Nodes)
            {
                foreach (object iteratorVariable1 in iteratorVariable0.GetSchemaObjects(type))
                {
                    yield return iteratorVariable1;
                }
            }
        }

        public IEnumerable<Type> GetSchemaObjectTypes()
        {
            Dictionary<Type, bool> dictionary = new Dictionary<Type, bool>();
            foreach (IProjectSchemaNode node in this.Nodes)
            {
                foreach (Type type in node.GetSchemaObjectTypes())
                {
                    dictionary[type] = true;
                }
            }
            return dictionary.Keys;
        }

        public List<IProjectSchemaNode> Nodes
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Nodes>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<Nodes>k__BackingField = value;
            }
        }

    }
}

