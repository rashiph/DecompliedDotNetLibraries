namespace System.Windows.Forms.Layout
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal class DefaultLayout : LayoutEngine
    {
        private static readonly int _cachedBoundsProperty = PropertyStore.CreateKey();
        private static readonly int _layoutInfoProperty = PropertyStore.CreateKey();
        internal static readonly DefaultLayout Instance = new DefaultLayout();

        private static void ApplyCachedBounds(IArrangedElement container)
        {
            if (CommonProperties.GetAutoSize(container))
            {
                Rectangle displayRectangle = container.DisplayRectangle;
                if ((displayRectangle.Width == 0) || (displayRectangle.Height == 0))
                {
                    ClearCachedBounds(container);
                    return;
                }
            }
            IDictionary dictionary = (IDictionary) container.Properties.GetObject(_cachedBoundsProperty);
            if (dictionary != null)
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    IArrangedElement key = (IArrangedElement) entry.Key;
                    Rectangle bounds = (Rectangle) entry.Value;
                    key.SetBounds(bounds, BoundsSpecified.None);
                }
                ClearCachedBounds(container);
            }
        }

        private static void ClearCachedBounds(IArrangedElement container)
        {
            container.Properties.SetObject(_cachedBoundsProperty, null);
        }

        public static AnchorStyles GetAnchor(IArrangedElement element)
        {
            return CommonProperties.xGetAnchor(element);
        }

        private static Rectangle GetAnchorDestination(IArrangedElement element, Rectangle displayRect, bool measureOnly)
        {
            AnchorInfo anchorInfo = GetAnchorInfo(element);
            int num = anchorInfo.Left + displayRect.X;
            int num2 = anchorInfo.Top + displayRect.Y;
            int num3 = anchorInfo.Right + displayRect.X;
            int num4 = anchorInfo.Bottom + displayRect.Y;
            AnchorStyles anchor = GetAnchor(element);
            if (IsAnchored(anchor, AnchorStyles.Right))
            {
                num3 += displayRect.Width;
                if (!IsAnchored(anchor, AnchorStyles.Left))
                {
                    num += displayRect.Width;
                }
            }
            else if (!IsAnchored(anchor, AnchorStyles.Left))
            {
                num3 += displayRect.Width / 2;
                num += displayRect.Width / 2;
            }
            if (IsAnchored(anchor, AnchorStyles.Bottom))
            {
                num4 += displayRect.Height;
                if (!IsAnchored(anchor, AnchorStyles.Top))
                {
                    num2 += displayRect.Height;
                }
            }
            else if (!IsAnchored(anchor, AnchorStyles.Top))
            {
                num4 += displayRect.Height / 2;
                num2 += displayRect.Height / 2;
            }
            if (!measureOnly)
            {
                if (num3 < num)
                {
                    num3 = num;
                }
                if (num4 < num2)
                {
                    num4 = num2;
                }
            }
            else
            {
                Rectangle cachedBounds = GetCachedBounds(element);
                if (((num3 < num) || (cachedBounds.Width != element.Bounds.Width)) || (cachedBounds.X != element.Bounds.X))
                {
                    if (cachedBounds != element.Bounds)
                    {
                        num = Math.Max(Math.Abs(num), Math.Abs(cachedBounds.Left));
                    }
                    num3 = (num + Math.Max(element.Bounds.Width, cachedBounds.Width)) + Math.Abs(num3);
                }
                else
                {
                    num = (num > 0) ? num : element.Bounds.Left;
                    num3 = (num3 > 0) ? num3 : (element.Bounds.Right + Math.Abs(num3));
                }
                if (((num4 < num2) || (cachedBounds.Height != element.Bounds.Height)) || (cachedBounds.Y != element.Bounds.Y))
                {
                    if (cachedBounds != element.Bounds)
                    {
                        num2 = Math.Max(Math.Abs(num2), Math.Abs(cachedBounds.Top));
                    }
                    num4 = (num2 + Math.Max(element.Bounds.Height, cachedBounds.Height)) + Math.Abs(num4);
                }
                else
                {
                    num2 = (num2 > 0) ? num2 : element.Bounds.Top;
                    num4 = (num4 > 0) ? num4 : (element.Bounds.Bottom + Math.Abs(num4));
                }
            }
            return new Rectangle(num, num2, num3 - num, num4 - num2);
        }

        private static AnchorInfo GetAnchorInfo(IArrangedElement element)
        {
            return (AnchorInfo) element.Properties.GetObject(_layoutInfoProperty);
        }

        private static Size GetAnchorPreferredSize(IArrangedElement container)
        {
            Size empty = Size.Empty;
            for (int i = container.Children.Count - 1; i >= 0; i--)
            {
                IArrangedElement element = container.Children[i];
                if (!CommonProperties.GetNeedsDockLayout(element) && element.ParticipatesInLayout)
                {
                    AnchorStyles anchor = GetAnchor(element);
                    Padding margin = CommonProperties.GetMargin(element);
                    Rectangle rectangle = LayoutUtils.InflateRect(GetCachedBounds(element), margin);
                    if (IsAnchored(anchor, AnchorStyles.Left) && !IsAnchored(anchor, AnchorStyles.Right))
                    {
                        empty.Width = Math.Max(empty.Width, rectangle.Right);
                    }
                    if (!IsAnchored(anchor, AnchorStyles.Bottom))
                    {
                        empty.Height = Math.Max(empty.Height, rectangle.Bottom);
                    }
                    if (IsAnchored(anchor, AnchorStyles.Right))
                    {
                        Rectangle rectangle2 = GetAnchorDestination(element, Rectangle.Empty, true);
                        if (rectangle2.Width < 0)
                        {
                            empty.Width = Math.Max(empty.Width, rectangle.Right + rectangle2.Width);
                        }
                        else
                        {
                            empty.Width = Math.Max(empty.Width, rectangle2.Right);
                        }
                    }
                    if (IsAnchored(anchor, AnchorStyles.Bottom))
                    {
                        Rectangle rectangle3 = GetAnchorDestination(element, Rectangle.Empty, true);
                        if (rectangle3.Height < 0)
                        {
                            empty.Height = Math.Max(empty.Height, rectangle.Bottom + rectangle3.Height);
                        }
                        else
                        {
                            empty.Height = Math.Max(empty.Height, rectangle3.Bottom);
                        }
                    }
                }
            }
            return empty;
        }

        private static Rectangle GetCachedBounds(IArrangedElement element)
        {
            if (element.Container != null)
            {
                IDictionary dictionary = (IDictionary) element.Container.Properties.GetObject(_cachedBoundsProperty);
                if (dictionary != null)
                {
                    object obj2 = dictionary[element];
                    if (obj2 != null)
                    {
                        return (Rectangle) obj2;
                    }
                }
            }
            return element.Bounds;
        }

        public static DockStyle GetDock(IArrangedElement element)
        {
            return CommonProperties.xGetDock(element);
        }

        private static Rectangle GetGrowthBounds(IArrangedElement element, Size newSize)
        {
            GrowthDirection growthDirection = GetGrowthDirection(element);
            Rectangle cachedBounds = GetCachedBounds(element);
            Point location = cachedBounds.Location;
            if ((growthDirection & GrowthDirection.Left) != GrowthDirection.None)
            {
                location.X -= newSize.Width - cachedBounds.Width;
            }
            if ((growthDirection & GrowthDirection.Upward) != GrowthDirection.None)
            {
                location.Y -= newSize.Height - cachedBounds.Height;
            }
            return new Rectangle(location, newSize);
        }

        private static GrowthDirection GetGrowthDirection(IArrangedElement element)
        {
            AnchorStyles anchor = GetAnchor(element);
            GrowthDirection none = GrowthDirection.None;
            if (((anchor & AnchorStyles.Right) != AnchorStyles.None) && ((anchor & AnchorStyles.Left) == AnchorStyles.None))
            {
                none |= GrowthDirection.Left;
            }
            else
            {
                none |= GrowthDirection.Right;
            }
            if (((anchor & AnchorStyles.Bottom) != AnchorStyles.None) && ((anchor & AnchorStyles.Top) == AnchorStyles.None))
            {
                return (none | GrowthDirection.Upward);
            }
            return (none | GrowthDirection.Downward);
        }

        private static Size GetHorizontalDockedSize(IArrangedElement element, Size remainingSize, bool measureOnly)
        {
            Size size = xGetDockedSize(element, remainingSize, new Size(1, remainingSize.Height), measureOnly);
            if (!measureOnly)
            {
                size.Height = remainingSize.Height;
                return size;
            }
            size.Height = Math.Max(size.Height, remainingSize.Height);
            return size;
        }

        internal override Size GetPreferredSize(IArrangedElement container, Size proposedBounds)
        {
            Size size;
            xLayout(container, true, out size);
            return size;
        }

        private static Size GetVerticalDockedSize(IArrangedElement element, Size remainingSize, bool measureOnly)
        {
            Size size = xGetDockedSize(element, remainingSize, new Size(remainingSize.Width, 1), measureOnly);
            if (!measureOnly)
            {
                size.Width = remainingSize.Width;
                return size;
            }
            size.Width = Math.Max(size.Width, remainingSize.Width);
            return size;
        }

        private static bool HasCachedBounds(IArrangedElement container)
        {
            return ((container != null) && (container.Properties.GetObject(_cachedBoundsProperty) != null));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal override void InitLayoutCore(IArrangedElement element, BoundsSpecified specified)
        {
            if ((specified != BoundsSpecified.None) && CommonProperties.GetNeedsAnchorLayout(element))
            {
                UpdateAnchorInfo(element);
            }
        }

        public static bool IsAnchored(AnchorStyles anchor, AnchorStyles desiredAnchor)
        {
            return ((anchor & desiredAnchor) == desiredAnchor);
        }

        private static void LayoutAnchoredControls(IArrangedElement container)
        {
            Rectangle displayRectangle = container.DisplayRectangle;
            if (!CommonProperties.GetAutoSize(container) || ((displayRectangle.Width != 0) && (displayRectangle.Height != 0)))
            {
                ArrangedElementCollection children = container.Children;
                for (int i = children.Count - 1; i >= 0; i--)
                {
                    IArrangedElement element = children[i];
                    if (CommonProperties.GetNeedsAnchorLayout(element))
                    {
                        SetCachedBounds(element, GetAnchorDestination(element, displayRectangle, false));
                    }
                }
            }
        }

        private static void LayoutAutoSizedControls(IArrangedElement container)
        {
            ArrangedElementCollection children = container.Children;
            for (int i = children.Count - 1; i >= 0; i--)
            {
                IArrangedElement element = children[i];
                if (CommonProperties.xGetAutoSizedAndAnchored(element))
                {
                    Rectangle cachedBounds = GetCachedBounds(element);
                    AnchorStyles anchor = GetAnchor(element);
                    Size maxSize = LayoutUtils.MaxSize;
                    if ((anchor & (AnchorStyles.Right | AnchorStyles.Left)) == (AnchorStyles.Right | AnchorStyles.Left))
                    {
                        maxSize.Width = cachedBounds.Width;
                    }
                    if ((anchor & (AnchorStyles.Bottom | AnchorStyles.Top)) == (AnchorStyles.Bottom | AnchorStyles.Top))
                    {
                        maxSize.Height = cachedBounds.Height;
                    }
                    Size preferredSize = element.GetPreferredSize(maxSize);
                    Rectangle bounds = cachedBounds;
                    if (CommonProperties.GetAutoSizeMode(element) == AutoSizeMode.GrowAndShrink)
                    {
                        bounds = GetGrowthBounds(element, preferredSize);
                    }
                    else if ((cachedBounds.Width < preferredSize.Width) || (cachedBounds.Height < preferredSize.Height))
                    {
                        Size newSize = LayoutUtils.UnionSizes(cachedBounds.Size, preferredSize);
                        bounds = GetGrowthBounds(element, newSize);
                    }
                    if (bounds != cachedBounds)
                    {
                        SetCachedBounds(element, bounds);
                    }
                }
            }
        }

        internal override bool LayoutCore(IArrangedElement container, LayoutEventArgs args)
        {
            Size size;
            return xLayout(container, false, out size);
        }

        private static Size LayoutDockedControls(IArrangedElement container, bool measureOnly)
        {
            Rectangle remainingBounds = measureOnly ? Rectangle.Empty : container.DisplayRectangle;
            Size empty = Size.Empty;
            IArrangedElement element = null;
            ArrangedElementCollection children = container.Children;
            for (int i = children.Count - 1; i >= 0; i--)
            {
                Size size6;
                IArrangedElement element2 = children[i];
                if (CommonProperties.GetNeedsDockLayout(element2))
                {
                    switch (GetDock(element2))
                    {
                        case DockStyle.Top:
                        {
                            Size size2 = GetVerticalDockedSize(element2, remainingBounds.Size, measureOnly);
                            Rectangle rectangle2 = new Rectangle(remainingBounds.X, remainingBounds.Y, size2.Width, size2.Height);
                            xLayoutDockedControl(element2, rectangle2, measureOnly, ref empty, ref remainingBounds);
                            remainingBounds.Y += element2.Bounds.Height;
                            remainingBounds.Height -= element2.Bounds.Height;
                            break;
                        }
                        case DockStyle.Bottom:
                        {
                            Size size3 = GetVerticalDockedSize(element2, remainingBounds.Size, measureOnly);
                            Rectangle rectangle3 = new Rectangle(remainingBounds.X, remainingBounds.Bottom - size3.Height, size3.Width, size3.Height);
                            xLayoutDockedControl(element2, rectangle3, measureOnly, ref empty, ref remainingBounds);
                            remainingBounds.Height -= element2.Bounds.Height;
                            break;
                        }
                        case DockStyle.Left:
                        {
                            Size size4 = GetHorizontalDockedSize(element2, remainingBounds.Size, measureOnly);
                            Rectangle rectangle4 = new Rectangle(remainingBounds.X, remainingBounds.Y, size4.Width, size4.Height);
                            xLayoutDockedControl(element2, rectangle4, measureOnly, ref empty, ref remainingBounds);
                            remainingBounds.X += element2.Bounds.Width;
                            remainingBounds.Width -= element2.Bounds.Width;
                            break;
                        }
                        case DockStyle.Right:
                        {
                            Size size5 = GetHorizontalDockedSize(element2, remainingBounds.Size, measureOnly);
                            Rectangle rectangle5 = new Rectangle(remainingBounds.Right - size5.Width, remainingBounds.Y, size5.Width, size5.Height);
                            xLayoutDockedControl(element2, rectangle5, measureOnly, ref empty, ref remainingBounds);
                            remainingBounds.Width -= element2.Bounds.Width;
                            break;
                        }
                        case DockStyle.Fill:
                            if (!(element2 is MdiClient))
                            {
                                goto Label_025B;
                            }
                            element = element2;
                            break;
                    }
                }
                goto Label_0295;
            Label_025B:
                size6 = remainingBounds.Size;
                Rectangle newElementBounds = new Rectangle(remainingBounds.X, remainingBounds.Y, size6.Width, size6.Height);
                xLayoutDockedControl(element2, newElementBounds, measureOnly, ref empty, ref remainingBounds);
            Label_0295:
                if (element != null)
                {
                    SetCachedBounds(element, remainingBounds);
                }
            }
            return empty;
        }

        public static void SetAnchor(IArrangedElement container, IArrangedElement element, AnchorStyles value)
        {
            AnchorStyles anchor = GetAnchor(element);
            if (anchor != value)
            {
                if (CommonProperties.GetNeedsDockLayout(element))
                {
                    SetDock(element, DockStyle.None);
                }
                CommonProperties.xSetAnchor(element, value);
                if (CommonProperties.GetNeedsAnchorLayout(element))
                {
                    UpdateAnchorInfo(element);
                }
                else
                {
                    SetAnchorInfo(element, null);
                }
                if (element.Container != null)
                {
                    bool flag = IsAnchored(anchor, AnchorStyles.Right) && !IsAnchored(value, AnchorStyles.Right);
                    bool flag2 = IsAnchored(anchor, AnchorStyles.Bottom) && !IsAnchored(value, AnchorStyles.Bottom);
                    if ((element.Container.Container != null) && (flag || flag2))
                    {
                        LayoutTransaction.DoLayout(element.Container.Container, element, PropertyNames.Anchor);
                    }
                    LayoutTransaction.DoLayout(element.Container, element, PropertyNames.Anchor);
                }
            }
        }

        private static void SetAnchorInfo(IArrangedElement element, AnchorInfo value)
        {
            element.Properties.SetObject(_layoutInfoProperty, value);
        }

        private static void SetCachedBounds(IArrangedElement element, Rectangle bounds)
        {
            if (bounds != GetCachedBounds(element))
            {
                IDictionary dictionary = (IDictionary) element.Container.Properties.GetObject(_cachedBoundsProperty);
                if (dictionary == null)
                {
                    dictionary = new HybridDictionary();
                    element.Container.Properties.SetObject(_cachedBoundsProperty, dictionary);
                }
                dictionary[element] = bounds;
            }
        }

        public static void SetDock(IArrangedElement element, DockStyle value)
        {
            if (GetDock(element) != value)
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 5))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(DockStyle));
                }
                bool needsDockLayout = CommonProperties.GetNeedsDockLayout(element);
                CommonProperties.xSetDock(element, value);
                using (new LayoutTransaction(element.Container as Control, element, PropertyNames.Dock))
                {
                    if (value == DockStyle.None)
                    {
                        if (needsDockLayout)
                        {
                            element.SetBounds(CommonProperties.GetSpecifiedBounds(element), BoundsSpecified.None);
                            UpdateAnchorInfo(element);
                        }
                    }
                    else
                    {
                        element.SetBounds(CommonProperties.GetSpecifiedBounds(element), BoundsSpecified.All);
                    }
                }
            }
        }

        private static void UpdateAnchorInfo(IArrangedElement element)
        {
            AnchorInfo anchorInfo = GetAnchorInfo(element);
            if (anchorInfo == null)
            {
                anchorInfo = new AnchorInfo();
                SetAnchorInfo(element, anchorInfo);
            }
            if (CommonProperties.GetNeedsAnchorLayout(element) && (element.Container != null))
            {
                GetCachedBounds(element);
                anchorInfo.Left = element.Bounds.Left;
                anchorInfo.Top = element.Bounds.Top;
                anchorInfo.Right = element.Bounds.Right;
                anchorInfo.Bottom = element.Bounds.Bottom;
                Rectangle displayRectangle = element.Container.DisplayRectangle;
                int width = displayRectangle.Width;
                int height = displayRectangle.Height;
                anchorInfo.Left -= displayRectangle.X;
                anchorInfo.Top -= displayRectangle.Y;
                anchorInfo.Right -= displayRectangle.X;
                anchorInfo.Bottom -= displayRectangle.Y;
                AnchorStyles anchor = GetAnchor(element);
                if (IsAnchored(anchor, AnchorStyles.Right))
                {
                    anchorInfo.Right -= width;
                    if (!IsAnchored(anchor, AnchorStyles.Left))
                    {
                        anchorInfo.Left -= width;
                    }
                }
                else if (!IsAnchored(anchor, AnchorStyles.Left))
                {
                    anchorInfo.Right -= width / 2;
                    anchorInfo.Left -= width / 2;
                }
                if (IsAnchored(anchor, AnchorStyles.Bottom))
                {
                    anchorInfo.Bottom -= height;
                    if (!IsAnchored(anchor, AnchorStyles.Top))
                    {
                        anchorInfo.Top -= height;
                    }
                }
                else if (!IsAnchored(anchor, AnchorStyles.Top))
                {
                    anchorInfo.Bottom -= height / 2;
                    anchorInfo.Top -= height / 2;
                }
            }
        }

        private static Size xGetDockedSize(IArrangedElement element, Size remainingSize, Size constraints, bool measureOnly)
        {
            if (CommonProperties.GetAutoSize(element))
            {
                return element.GetPreferredSize(constraints);
            }
            return element.Bounds.Size;
        }

        private static bool xLayout(IArrangedElement container, bool measureOnly, out Size preferredSize)
        {
            ArrangedElementCollection children = container.Children;
            preferredSize = new Size(-7103, -7105);
            if (measureOnly || (children.Count != 0))
            {
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                for (int i = children.Count - 1; i >= 0; i--)
                {
                    IArrangedElement element = children[i];
                    if (CommonProperties.GetNeedsDockAndAnchorLayout(element))
                    {
                        if (!flag && CommonProperties.GetNeedsDockLayout(element))
                        {
                            flag = true;
                        }
                        if (!flag2 && CommonProperties.GetNeedsAnchorLayout(element))
                        {
                            flag2 = true;
                        }
                        if (!flag3 && CommonProperties.xGetAutoSizedAndAnchored(element))
                        {
                            flag3 = true;
                        }
                    }
                }
                Size empty = Size.Empty;
                Size b = Size.Empty;
                if (flag)
                {
                    empty = LayoutDockedControls(container, measureOnly);
                }
                if (flag2 && !measureOnly)
                {
                    LayoutAnchoredControls(container);
                }
                if (flag3)
                {
                    LayoutAutoSizedControls(container);
                }
                if (!measureOnly)
                {
                    ApplyCachedBounds(container);
                }
                else
                {
                    b = GetAnchorPreferredSize(container);
                    Padding padding = Padding.Empty;
                    Control control = container as Control;
                    if (control != null)
                    {
                        padding = control.Padding;
                    }
                    else
                    {
                        padding = CommonProperties.GetPadding(container, Padding.Empty);
                    }
                    b.Width -= padding.Left;
                    b.Height -= padding.Top;
                    ClearCachedBounds(container);
                    preferredSize = LayoutUtils.UnionSizes(empty, b);
                }
            }
            return CommonProperties.GetAutoSize(container);
        }

        private static void xLayoutDockedControl(IArrangedElement element, Rectangle newElementBounds, bool measureOnly, ref Size preferredSize, ref Rectangle remainingBounds)
        {
            if (measureOnly)
            {
                Size proposedSize = new Size(Math.Max(0, newElementBounds.Width - remainingBounds.Width), Math.Max(0, newElementBounds.Height - remainingBounds.Height));
                DockStyle dock = GetDock(element);
                switch (dock)
                {
                    case DockStyle.Top:
                    case DockStyle.Bottom:
                        proposedSize.Width = 0;
                        break;
                }
                if ((dock == DockStyle.Left) || (dock == DockStyle.Right))
                {
                    proposedSize.Height = 0;
                }
                if (dock != DockStyle.Fill)
                {
                    preferredSize += proposedSize;
                    remainingBounds.Size += proposedSize;
                }
                else if ((dock == DockStyle.Fill) && CommonProperties.GetAutoSize(element))
                {
                    Size size2 = element.GetPreferredSize(proposedSize);
                    remainingBounds.Size += size2;
                    preferredSize += size2;
                }
            }
            else
            {
                element.SetBounds(newElementBounds, BoundsSpecified.None);
            }
        }

        private sealed class AnchorInfo
        {
            public int Bottom;
            public int Left;
            public int Right;
            public int Top;
        }

        [Flags]
        private enum GrowthDirection
        {
            Downward = 2,
            Left = 4,
            None = 0,
            Right = 8,
            Upward = 1
        }
    }
}

