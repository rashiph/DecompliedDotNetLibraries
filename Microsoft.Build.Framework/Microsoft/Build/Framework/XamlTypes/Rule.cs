namespace Microsoft.Build.Framework.XamlTypes
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Markup;

    [ContentProperty("Properties"), DebuggerDisplay("Rule: {Name}")]
    public sealed class Rule : RuleSchema, ISupportInitialize, IProjectSchemaNode
    {
        private OrderedDictionary categoryNamePropertyListMap;
        private string displayName;
        private List<Category> evaluatedCategories;

        public Rule()
        {
            this.Properties = new List<BaseProperty>();
            this.Categories = new List<Category>();
            this.SupportsFileBatching = false;
            this.ShowOnlyRuleProperties = true;
            this.SwitchPrefix = string.Empty;
            this.Separator = string.Empty;
        }

        public void BeginInit()
        {
        }

        private void CreateCategoryNamePropertyListMap()
        {
            this.evaluatedCategories = new List<Category>();
            if (this.Categories != null)
            {
                this.evaluatedCategories.AddRange(this.Categories);
            }
            this.categoryNamePropertyListMap = new OrderedDictionary();
            foreach (Category category in this.Categories)
            {
                this.categoryNamePropertyListMap.Add(category.Name, new List<BaseProperty>());
            }
            foreach (BaseProperty property in this.Properties)
            {
                if (!this.categoryNamePropertyListMap.Contains(property.Category))
                {
                    Category item = new Category {
                        Name = property.Category
                    };
                    this.evaluatedCategories.Add(item);
                    this.categoryNamePropertyListMap.Add(item.Name, new List<BaseProperty>());
                }
                (this.categoryNamePropertyListMap[property.Category] as List<BaseProperty>).Add(property);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void EndInit()
        {
            this.Initialize();
        }

        public OrderedDictionary GetPropertiesByCategory()
        {
            if (this.categoryNamePropertyListMap == null)
            {
                this.CreateCategoryNamePropertyListMap();
            }
            return this.categoryNamePropertyListMap;
        }

        public IList<BaseProperty> GetPropertiesInCategory(string categoryName)
        {
            if (this.categoryNamePropertyListMap == null)
            {
                this.CreateCategoryNamePropertyListMap();
            }
            return (this.categoryNamePropertyListMap[categoryName] as IList<BaseProperty>);
        }

        public IEnumerable<object> GetSchemaObjects(Type type)
        {
            if (!(type == typeof(Rule)))
            {
                yield break;
            }
            yield return this;
        }

        public IEnumerable<Type> GetSchemaObjectTypes()
        {
            yield return typeof(Rule);
        }

        private void Initialize()
        {
            if (this.Properties != null)
            {
                foreach (BaseProperty property in this.Properties)
                {
                    property.ContainingRule = this;
                }
            }
        }

        public string AdditionalInputs
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<AdditionalInputs>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<AdditionalInputs>k__BackingField = value;
            }
        }

        public List<Category> Categories
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Categories>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<Categories>k__BackingField = value;
            }
        }

        public string CommandLine
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<CommandLine>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<CommandLine>k__BackingField = value;
            }
        }

        public Microsoft.Build.Framework.XamlTypes.DataSource DataSource
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<DataSource>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<DataSource>k__BackingField = value;
            }
        }

        [Localizable(true)]
        public string Description
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Description>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<Description>k__BackingField = value;
            }
        }

        [Localizable(true)]
        public string DisplayName
        {
            get
            {
                return (this.displayName ?? this.Name);
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.displayName = value;
            }
        }

        public List<Category> EvaluatedCategories
        {
            get
            {
                if (this.evaluatedCategories == null)
                {
                    this.CreateCategoryNamePropertyListMap();
                }
                return this.evaluatedCategories;
            }
        }

        public string ExecutionDescription
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<ExecutionDescription>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<ExecutionDescription>k__BackingField = value;
            }
        }

        public string FileExtension
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<FileExtension>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<FileExtension>k__BackingField = value;
            }
        }

        [Localizable(true)]
        public string HelpString
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<HelpString>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<HelpString>k__BackingField = value;
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

        public int Order
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Order>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<Order>k__BackingField = value;
            }
        }

        public string Outputs
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Outputs>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<Outputs>k__BackingField = value;
            }
        }

        public string PageTemplate
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<PageTemplate>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<PageTemplate>k__BackingField = value;
            }
        }

        public List<BaseProperty> Properties
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Properties>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<Properties>k__BackingField = value;
            }
        }

        public string Separator
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Separator>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<Separator>k__BackingField = value;
            }
        }

        public bool ShowOnlyRuleProperties
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<ShowOnlyRuleProperties>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<ShowOnlyRuleProperties>k__BackingField = value;
            }
        }

        public bool SupportsFileBatching
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<SupportsFileBatching>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<SupportsFileBatching>k__BackingField = value;
            }
        }

        public string SwitchPrefix
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<SwitchPrefix>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<SwitchPrefix>k__BackingField = value;
            }
        }

        public string ToolName
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<ToolName>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<ToolName>k__BackingField = value;
            }
        }


    }
}

