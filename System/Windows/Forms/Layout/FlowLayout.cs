namespace System.Windows.Forms.Layout
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal class FlowLayout : LayoutEngine
    {
        private static readonly int _flowDirectionProperty = PropertyStore.CreateKey();
        private static readonly int _wrapContentsProperty = PropertyStore.CreateKey();
        internal static readonly FlowLayout Instance = new FlowLayout();

        private static ContainerProxy CreateContainerProxy(IArrangedElement container, FlowDirection flowDirection)
        {
            switch (flowDirection)
            {
                case FlowDirection.TopDown:
                    return new TopDownProxy(container);

                case FlowDirection.RightToLeft:
                    return new RightToLeftProxy(container);

                case FlowDirection.BottomUp:
                    return new BottomUpProxy(container);
            }
            return new ContainerProxy(container);
        }

        internal static FlowLayoutSettings CreateSettings(IArrangedElement owner)
        {
            return new FlowLayoutSettings(owner);
        }

        [Conditional("DEBUG_VERIFY_ALIGNMENT")]
        private void Debug_VerifyAlignment(IArrangedElement container, FlowDirection flowDirection)
        {
        }

        public static FlowDirection GetFlowDirection(IArrangedElement container)
        {
            return (FlowDirection) container.Properties.GetInteger(_flowDirectionProperty);
        }

        internal override Size GetPreferredSize(IArrangedElement container, Size proposedConstraints)
        {
            Rectangle displayRect = new Rectangle(new Point(0, 0), proposedConstraints);
            Size size = this.xLayout(container, displayRect, true);
            if ((size.Width <= proposedConstraints.Width) && (size.Height <= proposedConstraints.Height))
            {
                return size;
            }
            displayRect.Size = size;
            return this.xLayout(container, displayRect, true);
        }

        public static bool GetWrapContents(IArrangedElement container)
        {
            return (container.Properties.GetInteger(_wrapContentsProperty) == 0);
        }

        internal override bool LayoutCore(IArrangedElement container, LayoutEventArgs args)
        {
            CommonProperties.SetLayoutBounds(container, this.xLayout(container, container.DisplayRectangle, false));
            return CommonProperties.GetAutoSize(container);
        }

        private void LayoutRow(ContainerProxy containerProxy, ElementProxy elementProxy, int startIndex, int endIndex, Rectangle rowBounds)
        {
            int num;
            this.xLayoutRow(containerProxy, elementProxy, startIndex, endIndex, rowBounds, out num, false);
        }

        private Size MeasureRow(ContainerProxy containerProxy, ElementProxy elementProxy, int startIndex, Rectangle displayRectangle, out int breakIndex)
        {
            return this.xLayoutRow(containerProxy, elementProxy, startIndex, containerProxy.Container.Children.Count, displayRectangle, out breakIndex, true);
        }

        public static void SetFlowDirection(IArrangedElement container, FlowDirection value)
        {
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
            {
                throw new InvalidEnumArgumentException("value", (int) value, typeof(FlowDirection));
            }
            container.Properties.SetInteger(_flowDirectionProperty, (int) value);
            LayoutTransaction.DoLayout(container, container, PropertyNames.FlowDirection);
        }

        public static void SetWrapContents(IArrangedElement container, bool value)
        {
            container.Properties.SetInteger(_wrapContentsProperty, value ? 0 : 1);
            LayoutTransaction.DoLayout(container, container, PropertyNames.WrapContents);
        }

        private Size xLayout(IArrangedElement container, Rectangle displayRect, bool measureOnly)
        {
            int num2;
            FlowDirection flowDirection = GetFlowDirection(container);
            bool wrapContents = GetWrapContents(container);
            ContainerProxy containerProxy = CreateContainerProxy(container, flowDirection);
            containerProxy.DisplayRect = displayRect;
            displayRect = containerProxy.DisplayRect;
            ElementProxy elementProxy = containerProxy.ElementProxy;
            Size empty = Size.Empty;
            if (!wrapContents)
            {
                displayRect.Width = 0x7fffffff - displayRect.X;
            }
            for (int i = 0; i < container.Children.Count; i = num2)
            {
                Size size2 = Size.Empty;
                Rectangle displayRectangle = new Rectangle(displayRect.X, displayRect.Y, displayRect.Width, displayRect.Height - empty.Height);
                size2 = this.MeasureRow(containerProxy, elementProxy, i, displayRectangle, out num2);
                if (!measureOnly)
                {
                    Rectangle rowBounds = new Rectangle(displayRect.X, empty.Height + displayRect.Y, size2.Width, size2.Height);
                    this.LayoutRow(containerProxy, elementProxy, i, num2, rowBounds);
                }
                empty.Width = Math.Max(empty.Width, size2.Width);
                empty.Height += size2.Height;
            }
            if (container.Children.Count != 0)
            {
            }
            return LayoutUtils.FlipSizeIf((flowDirection == FlowDirection.TopDown) || (GetFlowDirection(container) == FlowDirection.BottomUp), empty);
        }

        private Size xLayoutRow(ContainerProxy containerProxy, ElementProxy elementProxy, int startIndex, int endIndex, Rectangle rowBounds, out int breakIndex, bool measureOnly)
        {
            Point location = rowBounds.Location;
            Size empty = Size.Empty;
            int num = 0;
            breakIndex = startIndex;
            bool wrapContents = GetWrapContents(containerProxy.Container);
            bool flag2 = false;
            ArrangedElementCollection children = containerProxy.Container.Children;
            int num2 = startIndex;
            while (num2 < endIndex)
            {
                elementProxy.Element = children[num2];
                if (elementProxy.ParticipatesInLayout)
                {
                    Size preferredSize;
                    if (elementProxy.AutoSize)
                    {
                        Size b = new Size(0x7fffffff, rowBounds.Height - elementProxy.Margin.Size.Height);
                        if (num2 == startIndex)
                        {
                            b.Width = (rowBounds.Width - empty.Width) - elementProxy.Margin.Size.Width;
                        }
                        b = LayoutUtils.UnionSizes(new Size(1, 1), b);
                        preferredSize = elementProxy.GetPreferredSize(b);
                    }
                    else
                    {
                        preferredSize = elementProxy.SpecifiedSize;
                        if (elementProxy.Stretches)
                        {
                            preferredSize.Height = 0;
                        }
                        if (preferredSize.Height < elementProxy.MinimumSize.Height)
                        {
                            preferredSize.Height = elementProxy.MinimumSize.Height;
                        }
                    }
                    Size size4 = preferredSize + elementProxy.Margin.Size;
                    if (!measureOnly)
                    {
                        Rectangle rect = new Rectangle(location, new Size(size4.Width, rowBounds.Height));
                        rect = LayoutUtils.DeflateRect(rect, elementProxy.Margin);
                        AnchorStyles anchorStyles = elementProxy.AnchorStyles;
                        containerProxy.Bounds = LayoutUtils.AlignAndStretch(preferredSize, rect, anchorStyles);
                    }
                    location.X += size4.Width;
                    if ((num > 0) && (location.X > rowBounds.Right))
                    {
                        return empty;
                    }
                    empty.Width = location.X - rowBounds.X;
                    empty.Height = Math.Max(empty.Height, size4.Height);
                    if (wrapContents)
                    {
                        if (flag2)
                        {
                            return empty;
                        }
                        if (((num2 + 1) < endIndex) && CommonProperties.GetFlowBreak(elementProxy.Element))
                        {
                            if (num == 0)
                            {
                                flag2 = true;
                            }
                            else
                            {
                                breakIndex++;
                                return empty;
                            }
                        }
                    }
                    num++;
                }
                num2++;
                breakIndex++;
            }
            return empty;
        }

        private class BottomUpProxy : FlowLayout.ContainerProxy
        {
            public BottomUpProxy(IArrangedElement container) : base(container)
            {
            }

            public override Rectangle Bounds
            {
                set
                {
                    base.Bounds = base.RTLTranslateNoMarginSwap(value);
                }
            }

            protected override bool IsVertical
            {
                get
                {
                    return true;
                }
            }
        }

        private class ContainerProxy
        {
            private IArrangedElement _container;
            private Rectangle _displayRect;
            private System.Windows.Forms.Layout.FlowLayout.ElementProxy _elementProxy;
            private bool _isContainerRTL;

            public ContainerProxy(IArrangedElement container)
            {
                this._container = container;
                this._isContainerRTL = false;
                if (this._container is Control)
                {
                    this._isContainerRTL = ((Control) this._container).RightToLeft == RightToLeft.Yes;
                }
            }

            protected Rectangle RTLTranslateNoMarginSwap(Rectangle bounds)
            {
                Rectangle rectangle = bounds;
                rectangle.X = (((this.DisplayRect.Right - bounds.X) - bounds.Width) + this.ElementProxy.Margin.Left) - this.ElementProxy.Margin.Right;
                FlowLayoutPanel container = this.Container as FlowLayoutPanel;
                if (container != null)
                {
                    Point autoScrollPosition = container.AutoScrollPosition;
                    if (!(autoScrollPosition != Point.Empty))
                    {
                        return rectangle;
                    }
                    Point point2 = new Point(rectangle.X, rectangle.Y);
                    if (this.IsVertical)
                    {
                        point2.Offset(autoScrollPosition.Y, 0);
                    }
                    else
                    {
                        point2.Offset(autoScrollPosition.X, 0);
                    }
                    rectangle.Location = point2;
                }
                return rectangle;
            }

            public virtual Rectangle Bounds
            {
                set
                {
                    if (this.IsContainerRTL)
                    {
                        if (this.IsVertical)
                        {
                            value.Y = this.DisplayRect.Bottom - value.Bottom;
                        }
                        else
                        {
                            value.X = this.DisplayRect.Right - value.Right;
                        }
                        FlowLayoutPanel container = this.Container as FlowLayoutPanel;
                        if (container != null)
                        {
                            Point autoScrollPosition = container.AutoScrollPosition;
                            if (autoScrollPosition != Point.Empty)
                            {
                                Point point2 = new Point(value.X, value.Y);
                                if (this.IsVertical)
                                {
                                    point2.Offset(0, autoScrollPosition.X);
                                }
                                else
                                {
                                    point2.Offset(autoScrollPosition.X, 0);
                                }
                                value.Location = point2;
                            }
                        }
                    }
                    this.ElementProxy.Bounds = value;
                }
            }

            public IArrangedElement Container
            {
                get
                {
                    return this._container;
                }
            }

            public Rectangle DisplayRect
            {
                get
                {
                    return this._displayRect;
                }
                set
                {
                    if (this._displayRect != value)
                    {
                        this._displayRect = LayoutUtils.FlipRectangleIf(this.IsVertical, value);
                    }
                }
            }

            public System.Windows.Forms.Layout.FlowLayout.ElementProxy ElementProxy
            {
                get
                {
                    if (this._elementProxy == null)
                    {
                        this._elementProxy = this.IsVertical ? new FlowLayout.VerticalElementProxy() : new System.Windows.Forms.Layout.FlowLayout.ElementProxy();
                    }
                    return this._elementProxy;
                }
            }

            protected bool IsContainerRTL
            {
                get
                {
                    return this._isContainerRTL;
                }
            }

            protected virtual bool IsVertical
            {
                get
                {
                    return false;
                }
            }
        }

        private class ElementProxy
        {
            private IArrangedElement _element;

            public virtual Size GetPreferredSize(Size proposedSize)
            {
                return this._element.GetPreferredSize(proposedSize);
            }

            public virtual System.Windows.Forms.AnchorStyles AnchorStyles
            {
                get
                {
                    System.Windows.Forms.AnchorStyles unifiedAnchor = LayoutUtils.GetUnifiedAnchor(this.Element);
                    bool flag = (unifiedAnchor & (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Top)) == (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Top);
                    bool flag2 = (unifiedAnchor & System.Windows.Forms.AnchorStyles.Top) != System.Windows.Forms.AnchorStyles.None;
                    bool flag3 = (unifiedAnchor & System.Windows.Forms.AnchorStyles.Bottom) != System.Windows.Forms.AnchorStyles.None;
                    if (flag)
                    {
                        return (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Top);
                    }
                    if (flag2)
                    {
                        return System.Windows.Forms.AnchorStyles.Top;
                    }
                    if (flag3)
                    {
                        return System.Windows.Forms.AnchorStyles.Bottom;
                    }
                    return System.Windows.Forms.AnchorStyles.None;
                }
            }

            public bool AutoSize
            {
                get
                {
                    return CommonProperties.GetAutoSize(this._element);
                }
            }

            public virtual Rectangle Bounds
            {
                set
                {
                    this._element.SetBounds(value, BoundsSpecified.None);
                }
            }

            public IArrangedElement Element
            {
                get
                {
                    return this._element;
                }
                set
                {
                    this._element = value;
                }
            }

            public virtual Padding Margin
            {
                get
                {
                    return CommonProperties.GetMargin(this.Element);
                }
            }

            public virtual Size MinimumSize
            {
                get
                {
                    return CommonProperties.GetMinimumSize(this.Element, Size.Empty);
                }
            }

            public bool ParticipatesInLayout
            {
                get
                {
                    return this._element.ParticipatesInLayout;
                }
            }

            public virtual Size SpecifiedSize
            {
                get
                {
                    return CommonProperties.GetSpecifiedBounds(this._element).Size;
                }
            }

            public bool Stretches
            {
                get
                {
                    System.Windows.Forms.AnchorStyles anchorStyles = this.AnchorStyles;
                    return (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Top) & anchorStyles) == (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Top));
                }
            }
        }

        private class RightToLeftProxy : FlowLayout.ContainerProxy
        {
            public RightToLeftProxy(IArrangedElement container) : base(container)
            {
            }

            public override Rectangle Bounds
            {
                set
                {
                    base.Bounds = base.RTLTranslateNoMarginSwap(value);
                }
            }
        }

        private class TopDownProxy : FlowLayout.ContainerProxy
        {
            public TopDownProxy(IArrangedElement container) : base(container)
            {
            }

            protected override bool IsVertical
            {
                get
                {
                    return true;
                }
            }
        }

        private class VerticalElementProxy : FlowLayout.ElementProxy
        {
            public override Size GetPreferredSize(Size proposedSize)
            {
                return LayoutUtils.FlipSize(base.GetPreferredSize(LayoutUtils.FlipSize(proposedSize)));
            }

            public override System.Windows.Forms.AnchorStyles AnchorStyles
            {
                get
                {
                    System.Windows.Forms.AnchorStyles unifiedAnchor = LayoutUtils.GetUnifiedAnchor(base.Element);
                    bool flag = (unifiedAnchor & (System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Left)) == (System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Left);
                    bool flag2 = (unifiedAnchor & System.Windows.Forms.AnchorStyles.Left) != System.Windows.Forms.AnchorStyles.None;
                    bool flag3 = (unifiedAnchor & System.Windows.Forms.AnchorStyles.Right) != System.Windows.Forms.AnchorStyles.None;
                    if (flag)
                    {
                        return (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Top);
                    }
                    if (flag2)
                    {
                        return System.Windows.Forms.AnchorStyles.Top;
                    }
                    if (flag3)
                    {
                        return System.Windows.Forms.AnchorStyles.Bottom;
                    }
                    return System.Windows.Forms.AnchorStyles.None;
                }
            }

            public override Rectangle Bounds
            {
                set
                {
                    base.Bounds = LayoutUtils.FlipRectangle(value);
                }
            }

            public override Padding Margin
            {
                get
                {
                    return LayoutUtils.FlipPadding(base.Margin);
                }
            }

            public override Size MinimumSize
            {
                get
                {
                    return LayoutUtils.FlipSize(base.MinimumSize);
                }
            }

            public override Size SpecifiedSize
            {
                get
                {
                    return LayoutUtils.FlipSize(base.SpecifiedSize);
                }
            }
        }
    }
}

