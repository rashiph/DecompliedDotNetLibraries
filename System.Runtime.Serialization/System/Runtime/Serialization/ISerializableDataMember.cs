namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime;

    internal class ISerializableDataMember
    {
        private string name;
        private IDataNode value;

        internal string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.name;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.name = value;
            }
        }

        internal IDataNode Value
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.value;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.value = value;
            }
        }
    }
}

