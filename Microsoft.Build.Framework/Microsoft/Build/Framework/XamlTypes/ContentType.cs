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

    [ContentProperty("Metadata")]
    public sealed class ContentType : ISupportInitialize, IProjectSchemaNode
    {
        private Dictionary<string, string> metadata;

        public ContentType()
        {
            this.Metadata = new List<NameValuePair>();
        }

        public void BeginInit()
        {
        }

        public void EndInit()
        {
        }

        public string GetMetadata(string metadataName)
        {
            if (string.IsNullOrEmpty(metadataName))
            {
                throw new ArgumentException();
            }
            if (this.metadata == null)
            {
                this.metadata = new Dictionary<string, string>(this.Metadata.Count, StringComparer.OrdinalIgnoreCase);
                foreach (NameValuePair pair in this.Metadata)
                {
                    this.metadata.Add(pair.Name, pair.Value);
                }
                this.Metadata = null;
            }
            string str = null;
            if (!this.metadata.TryGetValue(metadataName, out str))
            {
                str = null;
            }
            return str;
        }

        public IEnumerable<object> GetSchemaObjects(Type type)
        {
            if (!(type == typeof(ContentType)))
            {
                yield break;
            }
            yield return this;
        }

        public IEnumerable<Type> GetSchemaObjectTypes()
        {
            yield return typeof(ContentType);
        }

        public bool DefaultContentTypeForItemType
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<DefaultContentTypeForItemType>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<DefaultContentTypeForItemType>k__BackingField = value;
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

        public string ItemGroupName
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<ItemGroupName>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<ItemGroupName>k__BackingField = value;
            }
        }

        public string ItemType
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<ItemType>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<ItemType>k__BackingField = value;
            }
        }

        public List<NameValuePair> Metadata
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Metadata>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<Metadata>k__BackingField = value;
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

