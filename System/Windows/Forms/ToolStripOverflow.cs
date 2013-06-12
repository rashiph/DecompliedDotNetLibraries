namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms.Layout;

    [ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class ToolStripOverflow : ToolStripDropDown, IArrangedElement, IComponent, IDisposable
    {
        private ToolStripOverflowButton ownerItem;
        internal static readonly TraceSwitch PopupLayoutDebug;

        public ToolStripOverflow(ToolStripItem parentItem) : base(parentItem)
        {
            if (parentItem == null)
            {
                throw new ArgumentNullException("parentItem");
            }
            this.ownerItem = parentItem as ToolStripOverflowButton;
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new ToolStripOverflowAccessibleObject(this);
        }

        public override Size GetPreferredSize(Size constrainingSize)
        {
            constrainingSize.Width = 200;
            return base.GetPreferredSize(constrainingSize);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            if ((this.ParentToolStrip != null) && this.ParentToolStrip.IsInDesignMode)
            {
                if (FlowLayout.GetFlowDirection(this) != FlowDirection.TopDown)
                {
                    FlowLayout.SetFlowDirection(this, FlowDirection.TopDown);
                }
                if (FlowLayout.GetWrapContents(this))
                {
                    FlowLayout.SetWrapContents(this, false);
                }
            }
            else
            {
                if (FlowLayout.GetFlowDirection(this) != FlowDirection.LeftToRight)
                {
                    FlowLayout.SetFlowDirection(this, FlowDirection.LeftToRight);
                }
                if (!FlowLayout.GetWrapContents(this))
                {
                    FlowLayout.SetWrapContents(this, true);
                }
            }
            base.OnLayout(e);
        }

        protected override void SetDisplayedItems()
        {
            Size empty = Size.Empty;
            for (int i = 0; i < this.DisplayedItems.Count; i++)
            {
                ToolStripItem item = this.DisplayedItems[i];
                if (((IArrangedElement) item).ParticipatesInLayout)
                {
                    base.HasVisibleItems = true;
                    empty = LayoutUtils.UnionSizes(empty, item.Bounds.Size);
                }
            }
            base.SetLargestItemSize(empty);
        }

        void IArrangedElement.SetBounds(Rectangle bounds, BoundsSpecified specified)
        {
            this.SetBoundsCore(bounds.X, bounds.Y, bounds.Width, bounds.Height, specified);
        }

        protected internal override ToolStripItemCollection DisplayedItems
        {
            get
            {
                if (this.ParentToolStrip != null)
                {
                    return this.ParentToolStrip.OverflowItems;
                }
                return new ToolStripItemCollection(null, false);
            }
        }

        public override ToolStripItemCollection Items
        {
            get
            {
                return new ToolStripItemCollection(null, false, true);
            }
        }

        public override System.Windows.Forms.Layout.LayoutEngine LayoutEngine
        {
            get
            {
                return FlowLayout.Instance;
            }
        }

        private ToolStrip ParentToolStrip
        {
            get
            {
                if (this.ownerItem != null)
                {
                    return this.ownerItem.ParentToolStrip;
                }
                return null;
            }
        }

        ArrangedElementCollection IArrangedElement.Children
        {
            get
            {
                return this.DisplayedItems;
            }
        }

        IArrangedElement IArrangedElement.Container
        {
            get
            {
                return this.ParentInternal;
            }
        }

        bool IArrangedElement.ParticipatesInLayout
        {
            get
            {
                return base.GetState(2);
            }
        }

        PropertyStore IArrangedElement.Properties
        {
            get
            {
                return base.Properties;
            }
        }

        private class ToolStripOverflowAccessibleObject : ToolStrip.ToolStripAccessibleObject
        {
            public ToolStripOverflowAccessibleObject(ToolStripOverflow owner) : base(owner)
            {
            }

            public override AccessibleObject GetChild(int index)
            {
                return ((ToolStripOverflow) base.Owner).DisplayedItems[index].AccessibilityObject;
            }

            public override int GetChildCount()
            {
                return ((ToolStripOverflow) base.Owner).DisplayedItems.Count;
            }
        }
    }
}

