namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime;

    internal class CollectionDataNode : DataNode<Array>
    {
        private string itemName;
        private string itemNamespace;
        private IList<IDataNode> items;
        private int size = -1;

        internal CollectionDataNode()
        {
            base.dataType = Globals.TypeOfCollectionDataNode;
        }

        public override void Clear()
        {
            base.Clear();
            this.items = null;
            this.size = -1;
        }

        public override void GetData(ElementData element)
        {
            base.GetData(element);
            element.AddAttribute("z", "http://schemas.microsoft.com/2003/10/Serialization/", "Size", this.Size.ToString(NumberFormatInfo.InvariantInfo));
        }

        internal string ItemName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.itemName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.itemName = value;
            }
        }

        internal string ItemNamespace
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.itemNamespace;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.itemNamespace = value;
            }
        }

        internal IList<IDataNode> Items
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.items;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.items = value;
            }
        }

        internal int Size
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.size;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.size = value;
            }
        }
    }
}

