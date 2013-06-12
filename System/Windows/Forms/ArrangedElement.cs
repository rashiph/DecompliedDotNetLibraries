namespace System.Windows.Forms
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms.Layout;

    internal abstract class ArrangedElement : Component, IArrangedElement, IComponent, IDisposable
    {
        private Rectangle bounds = Rectangle.Empty;
        private IArrangedElement parent;
        private static readonly int PropControlsCollection = PropertyStore.CreateKey();
        private PropertyStore propertyStore = new PropertyStore();
        private Control spacer = new Control();
        private BitVector32 state = new BitVector32();
        private static readonly int stateDisposing = BitVector32.CreateMask(stateVisible);
        private static readonly int stateLocked = BitVector32.CreateMask(stateDisposing);
        private static readonly int stateVisible = BitVector32.CreateMask();
        private int suspendCount;

        internal ArrangedElement()
        {
            this.Padding = this.DefaultPadding;
            this.Margin = this.DefaultMargin;
            this.state[stateVisible] = true;
        }

        protected abstract ArrangedElementCollection GetChildren();
        protected abstract IArrangedElement GetContainer();
        public virtual Size GetPreferredSize(Size constrainingSize)
        {
            return (this.LayoutEngine.GetPreferredSize(this, constrainingSize - this.Padding.Size) + this.Padding.Size);
        }

        protected virtual void OnBoundsChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            this.PerformLayout(this, PropertyNames.Size);
        }

        protected virtual void OnLayout(LayoutEventArgs e)
        {
            this.LayoutEngine.Layout(this, e);
        }

        public virtual void PerformLayout(IArrangedElement container, string propertyName)
        {
            if (this.suspendCount <= 0)
            {
                this.OnLayout(new LayoutEventArgs(container, propertyName));
            }
        }

        public void SetBounds(Rectangle bounds, BoundsSpecified specified)
        {
            this.SetBoundsCore(bounds, specified);
        }

        protected virtual void SetBoundsCore(Rectangle bounds, BoundsSpecified specified)
        {
            if (bounds != this.bounds)
            {
                Rectangle oldBounds = this.bounds;
                this.bounds = bounds;
                this.OnBoundsChanged(oldBounds, bounds);
            }
        }

        public Rectangle Bounds
        {
            get
            {
                return this.bounds;
            }
        }

        protected virtual System.Windows.Forms.Padding DefaultMargin
        {
            get
            {
                return System.Windows.Forms.Padding.Empty;
            }
        }

        protected virtual System.Windows.Forms.Padding DefaultPadding
        {
            get
            {
                return System.Windows.Forms.Padding.Empty;
            }
        }

        public virtual Rectangle DisplayRectangle
        {
            get
            {
                return this.Bounds;
            }
        }

        public abstract System.Windows.Forms.Layout.LayoutEngine LayoutEngine { get; }

        public System.Windows.Forms.Padding Margin
        {
            get
            {
                return CommonProperties.GetMargin(this);
            }
            set
            {
                value = LayoutUtils.ClampNegativePaddingToZero(value);
                if (this.Margin != value)
                {
                    CommonProperties.SetMargin(this, value);
                }
            }
        }

        public virtual System.Windows.Forms.Padding Padding
        {
            get
            {
                return CommonProperties.GetPadding(this, this.DefaultPadding);
            }
            set
            {
                value = LayoutUtils.ClampNegativePaddingToZero(value);
                if (this.Padding != value)
                {
                    CommonProperties.SetPadding(this, value);
                }
            }
        }

        public virtual IArrangedElement Parent
        {
            get
            {
                return this.parent;
            }
            set
            {
                this.parent = value;
            }
        }

        public virtual bool ParticipatesInLayout
        {
            get
            {
                return this.Visible;
            }
        }

        private PropertyStore Properties
        {
            get
            {
                return this.propertyStore;
            }
        }

        ArrangedElementCollection IArrangedElement.Children
        {
            get
            {
                return this.GetChildren();
            }
        }

        IArrangedElement IArrangedElement.Container
        {
            get
            {
                return this.GetContainer();
            }
        }

        PropertyStore IArrangedElement.Properties
        {
            get
            {
                return this.Properties;
            }
        }

        public virtual bool Visible
        {
            get
            {
                return this.state[stateVisible];
            }
            set
            {
                if (this.state[stateVisible] != value)
                {
                    this.state[stateVisible] = value;
                    if (this.Parent != null)
                    {
                        LayoutTransaction.DoLayout(this.Parent, this, PropertyNames.Visible);
                    }
                }
            }
        }
    }
}

