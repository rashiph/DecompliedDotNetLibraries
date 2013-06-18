namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    public sealed class ExtensionDataObject
    {
        private IList<ExtensionDataMember> members;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ExtensionDataObject()
        {
        }

        internal IList<ExtensionDataMember> Members
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.members;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.members = value;
            }
        }
    }
}

