namespace System.Windows.Forms.PropertyGridInternal
{
    using System;
    using System.Collections;
    using System.Drawing;
    using System.Windows.Forms;

    internal class CategoryGridEntry : GridEntry
    {
        private Brush backBrush;
        private static Hashtable categoryStates;
        internal string name;

        public CategoryGridEntry(PropertyGrid ownerGrid, GridEntry peParent, string name, GridEntry[] childGridEntries) : base(ownerGrid, peParent)
        {
            this.name = name;
            if (categoryStates == null)
            {
                categoryStates = new Hashtable();
            }
            lock (categoryStates)
            {
                if (!categoryStates.ContainsKey(name))
                {
                    categoryStates.Add(name, true);
                }
            }
            this.IsExpandable = true;
            for (int i = 0; i < childGridEntries.Length; i++)
            {
                childGridEntries[i].ParentGridEntry = this;
            }
            base.ChildCollection = new GridEntryCollection(this, childGridEntries);
            lock (categoryStates)
            {
                this.InternalExpanded = (bool) categoryStates[name];
            }
            this.SetFlag(0x40, true);
        }

        protected override bool CreateChildren(bool diffOldChildren)
        {
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.backBrush != null)
                {
                    this.backBrush.Dispose();
                    this.backBrush = null;
                }
                if (base.ChildCollection != null)
                {
                    base.ChildCollection = null;
                }
            }
            base.Dispose(disposing);
        }

        public override void DisposeChildren()
        {
        }

        protected override Brush GetBackgroundBrush(Graphics g)
        {
            return this.GridEntryHost.GetLineBrush(g);
        }

        public override object GetChildValueOwner(GridEntry childEntry)
        {
            return this.ParentGridEntry.GetChildValueOwner(childEntry);
        }

        public override string GetPropertyTextValue(object o)
        {
            return "";
        }

        public override string GetTestingInfo()
        {
            string str = "object = (";
            return ((str + base.FullLabel) + "), Category = (" + this.PropertyLabel + ")");
        }

        internal override bool NotifyChildValue(GridEntry pe, int type)
        {
            return base.parentPE.NotifyChildValue(pe, type);
        }

        public override void PaintLabel(Graphics g, Rectangle rect, Rectangle clipRect, bool selected, bool paintFullLabel)
        {
            base.PaintLabel(g, rect, clipRect, false, true);
            if (selected && base.hasFocus)
            {
                bool boldFont = (this.Flags & 0x40) != 0;
                Font f = base.GetFont(boldFont);
                int num = base.GetLabelTextWidth(this.PropertyLabel, g, f);
                int x = this.PropertyLabelIndent - 2;
                Rectangle rectangle = new Rectangle(x, rect.Y, num + 3, rect.Height - 1);
                ControlPaint.DrawFocusRectangle(g, rectangle);
            }
            if (base.parentPE.GetChildIndex(this) > 0)
            {
                g.DrawLine(SystemPens.Control, (int) (rect.X - 1), (int) (rect.Y - 1), (int) (rect.Width + 2), (int) (rect.Y - 1));
            }
        }

        public override void PaintValue(object val, Graphics g, Rectangle rect, Rectangle clipRect, GridEntry.PaintValueFlags paintFlags)
        {
            base.PaintValue(val, g, rect, clipRect, paintFlags & ~GridEntry.PaintValueFlags.DrawSelected);
            if (base.parentPE.GetChildIndex(this) > 0)
            {
                g.DrawLine(SystemPens.Control, (int) (rect.X - 2), (int) (rect.Y - 1), (int) (rect.Width + 1), (int) (rect.Y - 1));
            }
        }

        public override bool Expandable
        {
            get
            {
                return !this.GetFlagSet(0x80000);
            }
        }

        public override System.Windows.Forms.GridItemType GridItemType
        {
            get
            {
                return System.Windows.Forms.GridItemType.Category;
            }
        }

        internal override bool HasValue
        {
            get
            {
                return false;
            }
        }

        public override string HelpKeyword
        {
            get
            {
                return null;
            }
        }

        internal override bool InternalExpanded
        {
            set
            {
                base.InternalExpanded = value;
                lock (categoryStates)
                {
                    categoryStates[this.name] = value;
                }
            }
        }

        protected override Color LabelTextColor
        {
            get
            {
                return base.ownerGrid.CategoryForeColor;
            }
        }

        public override int PropertyDepth
        {
            get
            {
                return (base.PropertyDepth - 1);
            }
        }

        public override string PropertyLabel
        {
            get
            {
                return this.name;
            }
        }

        internal override int PropertyLabelIndent
        {
            get
            {
                PropertyGridView gridEntryHost = this.GridEntryHost;
                return (((1 + gridEntryHost.GetOutlineIconSize()) + 5) + (base.PropertyDepth * gridEntryHost.GetDefaultOutlineIndent()));
            }
        }

        public override System.Type PropertyType
        {
            get
            {
                return typeof(void);
            }
        }
    }
}

