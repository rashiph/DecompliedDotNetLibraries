namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Drawing;
    using System.Runtime;

    internal class ItemInfo
    {
        private int commandID;
        private System.Drawing.Image image;
        private string text;
        private IDictionary userData;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ItemInfo(int id)
        {
            this.commandID = id;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ItemInfo(int id, System.Drawing.Image image, string text) : this(id)
        {
            this.image = image;
            this.text = text;
        }

        public override bool Equals(object obj)
        {
            return (((obj != null) && (obj is System.Workflow.ComponentModel.Design.ItemInfo)) && (((System.Workflow.ComponentModel.Design.ItemInfo) obj).commandID == this.commandID));
        }

        public override int GetHashCode()
        {
            return (base.GetHashCode() ^ this.commandID.GetHashCode());
        }

        public int Identifier
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.commandID;
            }
        }

        public System.Drawing.Image Image
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.image;
            }
        }

        public string Text
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.text;
            }
        }

        public IDictionary UserData
        {
            get
            {
                if (this.userData == null)
                {
                    this.userData = new HybridDictionary();
                }
                return this.userData;
            }
        }
    }
}

