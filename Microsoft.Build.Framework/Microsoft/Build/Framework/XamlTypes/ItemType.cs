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

    public sealed class ItemType : ISupportInitialize, IProjectSchemaNode
    {
        public ItemType()
        {
            this.UpToDateCheckInput = true;
        }

        public void BeginInit()
        {
        }

        public void EndInit()
        {
        }

        public IEnumerable<object> GetSchemaObjects(Type type)
        {
            if (!(type == typeof(ItemType)))
            {
                yield break;
            }
            yield return this;
        }

        public IEnumerable<Type> GetSchemaObjectTypes()
        {
            yield return typeof(ItemType);
        }

        public string DefaultContentType
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<DefaultContentType>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<DefaultContentType>k__BackingField = value;
            }
        }

        [Localizable(true)]
        public string DisplayName
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<DisplayName>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<DisplayName>k__BackingField = value;
            }
        }

        public string Name
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Name>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<Name>k__BackingField = value;
            }
        }

        public bool UpToDateCheckInput
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<UpToDateCheckInput>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<UpToDateCheckInput>k__BackingField = value;
            }
        }


    }
}

