namespace System.Windows.Forms.Layout
{
    using System;
    using System.Collections.Specialized;
    using System.Drawing;
    using System.Runtime;
    using System.Windows.Forms;

    internal class CommonProperties
    {
        private static readonly BitVector32.Section _anchorNeverShrinksSection = BitVector32.CreateSection(1, _BoxStretchInternalSection);
        private static readonly BitVector32.Section _autoSizeModeSection = BitVector32.CreateSection(1, _selfAutoSizingSection);
        private static readonly BitVector32.Section _autoSizeSection = BitVector32.CreateSection(1, _dockModeSection);
        private static readonly BitVector32.Section _BoxStretchInternalSection = BitVector32.CreateSection(3, _autoSizeSection);
        private static readonly BitVector32.Section _dockAndAnchorNeedsLayoutSection = BitVector32.CreateSection(0x7f);
        private static readonly BitVector32.Section _dockAndAnchorSection = BitVector32.CreateSection(15);
        private static readonly BitVector32.Section _dockModeSection = BitVector32.CreateSection(1, _dockAndAnchorSection);
        private static readonly BitVector32.Section _flowBreakSection = BitVector32.CreateSection(1, _anchorNeverShrinksSection);
        private static readonly int _layoutBoundsProperty = PropertyStore.CreateKey();
        private static readonly int _layoutStateProperty = PropertyStore.CreateKey();
        private static readonly int _marginProperty = PropertyStore.CreateKey();
        private static readonly int _maximumSizeProperty = PropertyStore.CreateKey();
        private static readonly int _minimumSizeProperty = PropertyStore.CreateKey();
        private static readonly int _paddingProperty = PropertyStore.CreateKey();
        private static readonly int _preferredSizeCacheProperty = PropertyStore.CreateKey();
        private static readonly BitVector32.Section _selfAutoSizingSection = BitVector32.CreateSection(1, _flowBreakSection);
        private static readonly int _specifiedBoundsProperty = PropertyStore.CreateKey();
        internal const ContentAlignment DefaultAlignment = ContentAlignment.TopLeft;
        internal const AnchorStyles DefaultAnchor = (AnchorStyles.Left | AnchorStyles.Top);
        internal const bool DefaultAutoSize = false;
        internal const DockStyle DefaultDock = DockStyle.None;
        internal static readonly Padding DefaultMargin = new Padding(3);
        internal static readonly Size DefaultMaximumSize = new Size(0, 0);
        internal static readonly Size DefaultMinimumSize = new Size(0, 0);

        internal static void ClearMaximumSize(IArrangedElement element)
        {
            if (element.Properties.ContainsObject(_maximumSizeProperty))
            {
                element.Properties.RemoveObject(_maximumSizeProperty);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal static bool GetAutoSize(IArrangedElement element)
        {
            int num = GetLayoutState(element)[_autoSizeSection];
            return (num != 0);
        }

        internal static AutoSizeMode GetAutoSizeMode(IArrangedElement element)
        {
            if (GetLayoutState(element)[_autoSizeModeSection] != 0)
            {
                return AutoSizeMode.GrowAndShrink;
            }
            return AutoSizeMode.GrowOnly;
        }

        internal static bool GetFlowBreak(IArrangedElement element)
        {
            int num = GetLayoutState(element)[_flowBreakSection];
            return (num == 1);
        }

        internal static Size GetLayoutBounds(IArrangedElement element)
        {
            bool flag;
            Size size = element.Properties.GetSize(_layoutBoundsProperty, out flag);
            if (flag)
            {
                return size;
            }
            return Size.Empty;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal static BitVector32 GetLayoutState(IArrangedElement element)
        {
            return new BitVector32(element.Properties.GetInteger(_layoutStateProperty));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal static Padding GetMargin(IArrangedElement element)
        {
            bool flag;
            Padding padding = element.Properties.GetPadding(_marginProperty, out flag);
            if (flag)
            {
                return padding;
            }
            return DefaultMargin;
        }

        internal static Size GetMaximumSize(IArrangedElement element, Size defaultMaximumSize)
        {
            bool flag;
            Size size = element.Properties.GetSize(_maximumSizeProperty, out flag);
            if (flag)
            {
                return size;
            }
            return defaultMaximumSize;
        }

        internal static Size GetMinimumSize(IArrangedElement element, Size defaultMinimumSize)
        {
            bool flag;
            Size size = element.Properties.GetSize(_minimumSizeProperty, out flag);
            if (flag)
            {
                return size;
            }
            return defaultMinimumSize;
        }

        internal static bool GetNeedsAnchorLayout(IArrangedElement element)
        {
            BitVector32 layoutState = GetLayoutState(element);
            return ((layoutState[_dockAndAnchorNeedsLayoutSection] != 0) && (layoutState[_dockModeSection] == 0));
        }

        internal static bool GetNeedsDockAndAnchorLayout(IArrangedElement element)
        {
            return (GetLayoutState(element)[_dockAndAnchorNeedsLayoutSection] != 0);
        }

        internal static bool GetNeedsDockLayout(IArrangedElement element)
        {
            return ((GetLayoutState(element)[_dockModeSection] == 1) && element.ParticipatesInLayout);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal static Padding GetPadding(IArrangedElement element, Padding defaultPadding)
        {
            bool flag;
            Padding padding = element.Properties.GetPadding(_paddingProperty, out flag);
            if (flag)
            {
                return padding;
            }
            return defaultPadding;
        }

        internal static bool GetSelfAutoSizeInDefaultLayout(IArrangedElement element)
        {
            int num = GetLayoutState(element)[_selfAutoSizingSection];
            return (num == 1);
        }

        internal static Rectangle GetSpecifiedBounds(IArrangedElement element)
        {
            bool flag;
            Rectangle rectangle = element.Properties.GetRectangle(_specifiedBoundsProperty, out flag);
            if (flag && (rectangle != LayoutUtils.MaxRectangle))
            {
                return rectangle;
            }
            return element.Bounds;
        }

        internal static bool HasLayoutBounds(IArrangedElement element)
        {
            bool flag;
            element.Properties.GetSize(_layoutBoundsProperty, out flag);
            return flag;
        }

        internal static void ResetPadding(IArrangedElement element)
        {
            if (element.Properties.GetObject(_paddingProperty) != null)
            {
                element.Properties.RemoveObject(_paddingProperty);
            }
        }

        internal static void SetAutoSize(IArrangedElement element, bool value)
        {
            BitVector32 layoutState = GetLayoutState(element);
            layoutState[_autoSizeSection] = value ? 1 : 0;
            SetLayoutState(element, layoutState);
            if (!value)
            {
                element.SetBounds(GetSpecifiedBounds(element), BoundsSpecified.None);
            }
        }

        internal static void SetAutoSizeMode(IArrangedElement element, AutoSizeMode mode)
        {
            BitVector32 layoutState = GetLayoutState(element);
            layoutState[_autoSizeModeSection] = (mode == AutoSizeMode.GrowAndShrink) ? 1 : 0;
            SetLayoutState(element, layoutState);
        }

        internal static void SetFlowBreak(IArrangedElement element, bool value)
        {
            BitVector32 layoutState = GetLayoutState(element);
            layoutState[_flowBreakSection] = value ? 1 : 0;
            SetLayoutState(element, layoutState);
            LayoutTransaction.DoLayout(element.Container, element, PropertyNames.FlowBreak);
        }

        internal static void SetLayoutBounds(IArrangedElement element, Size value)
        {
            element.Properties.SetSize(_layoutBoundsProperty, value);
        }

        internal static void SetLayoutState(IArrangedElement element, BitVector32 state)
        {
            element.Properties.SetInteger(_layoutStateProperty, state.Data);
        }

        internal static void SetMargin(IArrangedElement element, Padding value)
        {
            element.Properties.SetPadding(_marginProperty, value);
            LayoutTransaction.DoLayout(element.Container, element, PropertyNames.Margin);
        }

        internal static void SetMaximumSize(IArrangedElement element, Size value)
        {
            element.Properties.SetSize(_maximumSizeProperty, value);
            Rectangle bounds = element.Bounds;
            bounds.Width = Math.Min(bounds.Width, value.Width);
            bounds.Height = Math.Min(bounds.Height, value.Height);
            element.SetBounds(bounds, BoundsSpecified.Size);
            LayoutTransaction.DoLayout(element.Container, element, PropertyNames.MaximumSize);
        }

        internal static void SetMinimumSize(IArrangedElement element, Size value)
        {
            element.Properties.SetSize(_minimumSizeProperty, value);
            using (new LayoutTransaction(element.Container as Control, element, PropertyNames.MinimumSize))
            {
                Rectangle bounds = element.Bounds;
                bounds.Width = Math.Max(bounds.Width, value.Width);
                bounds.Height = Math.Max(bounds.Height, value.Height);
                element.SetBounds(bounds, BoundsSpecified.Size);
            }
        }

        internal static void SetPadding(IArrangedElement element, Padding value)
        {
            value = LayoutUtils.ClampNegativePaddingToZero(value);
            element.Properties.SetPadding(_paddingProperty, value);
        }

        internal static void SetSelfAutoSizeInDefaultLayout(IArrangedElement element, bool value)
        {
            BitVector32 layoutState = GetLayoutState(element);
            layoutState[_selfAutoSizingSection] = value ? 1 : 0;
            SetLayoutState(element, layoutState);
        }

        internal static bool ShouldSelfSize(IArrangedElement element)
        {
            return (!GetAutoSize(element) || (((element.Container is Control) && (((Control) element.Container).LayoutEngine is DefaultLayout)) && GetSelfAutoSizeInDefaultLayout(element)));
        }

        internal static void UpdateSpecifiedBounds(IArrangedElement element, int x, int y, int width, int height)
        {
            Rectangle rectangle = new Rectangle(x, y, width, height);
            element.Properties.SetRectangle(_specifiedBoundsProperty, rectangle);
        }

        internal static void UpdateSpecifiedBounds(IArrangedElement element, int x, int y, int width, int height, BoundsSpecified specified)
        {
            Rectangle specifiedBounds = GetSpecifiedBounds(element);
            bool flag = ((specified & BoundsSpecified.X) == BoundsSpecified.None) & (x != specifiedBounds.X);
            bool flag2 = ((specified & BoundsSpecified.Y) == BoundsSpecified.None) & (y != specifiedBounds.Y);
            bool flag3 = ((specified & BoundsSpecified.Width) == BoundsSpecified.None) & (width != specifiedBounds.Width);
            bool flag4 = ((specified & BoundsSpecified.Height) == BoundsSpecified.None) & (height != specifiedBounds.Height);
            if (((flag | flag2) | flag3) | flag4)
            {
                if (!flag)
                {
                    specifiedBounds.X = x;
                }
                if (!flag2)
                {
                    specifiedBounds.Y = y;
                }
                if (!flag3)
                {
                    specifiedBounds.Width = width;
                }
                if (!flag4)
                {
                    specifiedBounds.Height = height;
                }
                element.Properties.SetRectangle(_specifiedBoundsProperty, specifiedBounds);
            }
            else if (element.Properties.ContainsObject(_specifiedBoundsProperty))
            {
                element.Properties.SetRectangle(_specifiedBoundsProperty, LayoutUtils.MaxRectangle);
            }
        }

        internal static void xClearAllPreferredSizeCaches(IArrangedElement start)
        {
            xClearPreferredSizeCache(start);
            ArrangedElementCollection children = start.Children;
            for (int i = 0; i < children.Count; i++)
            {
                xClearAllPreferredSizeCaches(children[i]);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal static void xClearPreferredSizeCache(IArrangedElement element)
        {
            element.Properties.SetSize(_preferredSizeCacheProperty, LayoutUtils.InvalidSize);
        }

        internal static AnchorStyles xGetAnchor(IArrangedElement element)
        {
            BitVector32 layoutState = GetLayoutState(element);
            AnchorStyles anchor = (AnchorStyles) layoutState[_dockAndAnchorSection];
            return ((layoutState[_dockModeSection] == 0) ? xTranslateAnchorValue(anchor) : (AnchorStyles.Left | AnchorStyles.Top));
        }

        internal static bool xGetAutoSizedAndAnchored(IArrangedElement element)
        {
            BitVector32 layoutState = GetLayoutState(element);
            if (layoutState[_selfAutoSizingSection] != 0)
            {
                return false;
            }
            return ((layoutState[_autoSizeSection] != 0) && (layoutState[_dockModeSection] == 0));
        }

        internal static DockStyle xGetDock(IArrangedElement element)
        {
            BitVector32 layoutState = GetLayoutState(element);
            DockStyle style = (DockStyle) layoutState[_dockAndAnchorSection];
            DockAnchorMode mode = (DockAnchorMode) layoutState[_dockModeSection];
            return ((mode == DockAnchorMode.Dock) ? style : DockStyle.None);
        }

        internal static Size xGetPreferredSizeCache(IArrangedElement element)
        {
            bool flag;
            Size size = element.Properties.GetSize(_preferredSizeCacheProperty, out flag);
            if (flag && (size != LayoutUtils.InvalidSize))
            {
                return size;
            }
            return Size.Empty;
        }

        internal static void xSetAnchor(IArrangedElement element, AnchorStyles value)
        {
            BitVector32 layoutState = GetLayoutState(element);
            layoutState[_dockAndAnchorSection] = (int) xTranslateAnchorValue(value);
            layoutState[_dockModeSection] = 0;
            SetLayoutState(element, layoutState);
        }

        internal static void xSetDock(IArrangedElement element, DockStyle value)
        {
            BitVector32 layoutState = GetLayoutState(element);
            layoutState[_dockAndAnchorSection] = (int) value;
            layoutState[_dockModeSection] = (value == DockStyle.None) ? 0 : 1;
            SetLayoutState(element, layoutState);
        }

        internal static void xSetPreferredSizeCache(IArrangedElement element, Size value)
        {
            element.Properties.SetSize(_preferredSizeCacheProperty, value);
        }

        private static AnchorStyles xTranslateAnchorValue(AnchorStyles anchor)
        {
            AnchorStyles styles = anchor;
            if (styles != AnchorStyles.None)
            {
                if (styles == (AnchorStyles.Left | AnchorStyles.Top))
                {
                    return AnchorStyles.None;
                }
                return anchor;
            }
            return (AnchorStyles.Left | AnchorStyles.Top);
        }

        private enum DockAnchorMode
        {
            Anchor,
            Dock
        }
    }
}

