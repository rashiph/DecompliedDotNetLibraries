namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    internal class ClassDataNode : DataNode<object>
    {
        private IList<ExtensionDataMember> members;

        internal ClassDataNode()
        {
            base.dataType = Globals.TypeOfClassDataNode;
        }

        public override void Clear()
        {
            base.Clear();
            this.members = null;
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

