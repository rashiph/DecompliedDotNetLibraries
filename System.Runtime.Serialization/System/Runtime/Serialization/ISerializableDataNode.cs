namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    internal class ISerializableDataNode : DataNode<object>
    {
        private string factoryTypeName;
        private string factoryTypeNamespace;
        private IList<ISerializableDataMember> members;

        internal ISerializableDataNode()
        {
            base.dataType = Globals.TypeOfISerializableDataNode;
        }

        public override void Clear()
        {
            base.Clear();
            this.members = null;
            this.factoryTypeName = (string) (this.factoryTypeNamespace = null);
        }

        public override void GetData(ElementData element)
        {
            base.GetData(element);
            if (this.FactoryTypeName != null)
            {
                base.AddQualifiedNameAttribute(element, "z", "FactoryType", "http://schemas.microsoft.com/2003/10/Serialization/", this.FactoryTypeName, this.FactoryTypeNamespace);
            }
        }

        internal string FactoryTypeName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.factoryTypeName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.factoryTypeName = value;
            }
        }

        internal string FactoryTypeNamespace
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.factoryTypeNamespace;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.factoryTypeNamespace = value;
            }
        }

        internal IList<ISerializableDataMember> Members
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

