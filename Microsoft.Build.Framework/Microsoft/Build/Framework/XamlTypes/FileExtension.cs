namespace Microsoft.Build.Framework.XamlTypes
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public sealed class FileExtension : IProjectSchemaNode
    {
        public IEnumerable<object> GetSchemaObjects(Type type)
        {
            if (!(type == typeof(FileExtension)))
            {
                yield break;
            }
            yield return this;
        }

        public IEnumerable<Type> GetSchemaObjectTypes()
        {
            yield return typeof(FileExtension);
        }

        public string ContentType
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<ContentType>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<ContentType>k__BackingField = value;
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


    }
}

