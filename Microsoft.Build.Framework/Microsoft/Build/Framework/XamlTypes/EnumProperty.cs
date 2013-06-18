namespace Microsoft.Build.Framework.XamlTypes
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("AdmissibleValues")]
    public sealed class EnumProperty : BaseProperty
    {
        public EnumProperty()
        {
            this.AdmissibleValues = new List<EnumValue>();
        }

        public override void EndInit()
        {
            base.EndInit();
        }

        public List<EnumValue> AdmissibleValues
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<AdmissibleValues>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<AdmissibleValues>k__BackingField = value;
            }
        }
    }
}

