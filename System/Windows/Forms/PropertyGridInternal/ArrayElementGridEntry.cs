namespace System.Windows.Forms.PropertyGridInternal
{
    using System;
    using System.Globalization;
    using System.Windows.Forms;

    internal class ArrayElementGridEntry : GridEntry
    {
        protected int index;

        public ArrayElementGridEntry(PropertyGrid ownerGrid, GridEntry peParent, int index) : base(ownerGrid, peParent)
        {
            this.index = index;
            this.SetFlag(0x100, ((peParent.Flags & 0x100) != 0) || peParent.ForceReadOnly);
        }

        public override System.Windows.Forms.GridItemType GridItemType
        {
            get
            {
                return System.Windows.Forms.GridItemType.ArrayValue;
            }
        }

        public override bool IsValueEditable
        {
            get
            {
                return this.ParentGridEntry.IsValueEditable;
            }
        }

        public override string PropertyLabel
        {
            get
            {
                return ("[" + this.index.ToString(CultureInfo.CurrentCulture) + "]");
            }
        }

        public override System.Type PropertyType
        {
            get
            {
                return base.parentPE.PropertyType.GetElementType();
            }
        }

        public override object PropertyValue
        {
            get
            {
                return ((Array) this.GetValueOwner()).GetValue(this.index);
            }
            set
            {
                ((Array) this.GetValueOwner()).SetValue(value, this.index);
            }
        }

        public override bool ShouldRenderReadOnly
        {
            get
            {
                return this.ParentGridEntry.ShouldRenderReadOnly;
            }
        }
    }
}

