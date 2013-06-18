namespace System.Windows.Forms
{
    using System;

    internal class LabelImageIndexer : System.Windows.Forms.ImageList.Indexer
    {
        private Label owner;
        private bool useIntegerIndex = true;

        public LabelImageIndexer(Label owner)
        {
            this.owner = owner;
        }

        public override int ActualIndex
        {
            get
            {
                if (this.useIntegerIndex)
                {
                    if (this.Index >= this.ImageList.Images.Count)
                    {
                        return (this.ImageList.Images.Count - 1);
                    }
                    return this.Index;
                }
                if (this.ImageList != null)
                {
                    return this.ImageList.Images.IndexOfKey(this.Key);
                }
                return -1;
            }
        }

        public override System.Windows.Forms.ImageList ImageList
        {
            get
            {
                if (this.owner != null)
                {
                    return this.owner.ImageList;
                }
                return null;
            }
            set
            {
            }
        }

        public override int Index
        {
            get
            {
                return base.Index;
            }
            set
            {
                base.Index = value;
                this.useIntegerIndex = true;
            }
        }

        public override string Key
        {
            get
            {
                return base.Key;
            }
            set
            {
                base.Key = value;
                this.useIntegerIndex = false;
            }
        }
    }
}

