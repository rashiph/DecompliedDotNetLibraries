namespace Microsoft.Build.Framework.XamlTypes
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Markup;

    [ContentProperty("Rules")]
    public sealed class RuleBag : ISupportInitialize, IProjectSchemaNode
    {
        public RuleBag()
        {
            this.Rules = new List<Rule>();
        }

        public void BeginInit()
        {
        }

        public void EndInit()
        {
        }

        public IEnumerable<object> GetSchemaObjects(Type type)
        {
            if (type == typeof(Rule))
            {
                foreach (Rule iteratorVariable0 in this.Rules)
                {
                    yield return iteratorVariable0;
                }
            }
        }

        public IEnumerable<Type> GetSchemaObjectTypes()
        {
            yield return typeof(Rule);
        }

        public List<Rule> Rules
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Rules>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<Rules>k__BackingField = value;
            }
        }


    }
}

