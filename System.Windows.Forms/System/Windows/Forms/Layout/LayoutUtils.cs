namespace System.Windows.Forms.Layout
{
    using System;
    using System.Collections;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal class LayoutUtils
    {
        public const ContentAlignment AnyBottom = (ContentAlignment.BottomRight | ContentAlignment.BottomCenter | ContentAlignment.BottomLeft);
        public const ContentAlignment AnyCenter = (ContentAlignment.BottomCenter | ContentAlignment.MiddleCenter | ContentAlignment.TopCenter);
        public const ContentAlignment AnyLeft = (ContentAlignment.BottomLeft | ContentAlignment.MiddleLeft | ContentAlignment.TopLeft);
        public const ContentAlignment AnyMiddle = (ContentAlignment.MiddleRight | ContentAlignment.MiddleCenter | ContentAlignment.MiddleLeft);
        public const ContentAlignment AnyRight = (ContentAlignment.BottomRight | ContentAlignment.MiddleRight | ContentAlignment.TopRight);
        public const ContentAlignment AnyTop = (ContentAlignment.TopRight | ContentAlignment.TopCenter | ContentAlignment.TopLeft);
        private static readonly AnchorStyles[] dockingToAnchor = new AnchorStyles[] { (AnchorStyles.Left | AnchorStyles.Top), (AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top), (AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom), (AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top), (AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top), (AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top) };
        public const AnchorStyles HorizontalAnchorStyles = (AnchorStyles.Right | AnchorStyles.Left);
        public static readonly Size InvalidSize = new Size(-2147483648, -2147483648);
        public static readonly Rectangle MaxRectangle = new Rectangle(0, 0, 0x7fffffff, 0x7fffffff);
        public static readonly Size MaxSize = new Size(0x7fffffff, 0x7fffffff);
        public static readonly string TestString = "j^";
        public const AnchorStyles VerticalAnchorStyles = (AnchorStyles.Bottom | AnchorStyles.Top);

        public static Size AddAlignedRegion(Size textSize, Size imageSize, TextImageRelation relation)
        {
            return AddAlignedRegionCore(textSize, imageSize, IsVerticalRelation(relation));
        }

        public static Size AddAlignedRegionCore(Size currentSize, Size contentSize, bool vertical)
        {
            if (vertical)
            {
                currentSize.Width = Math.Max(currentSize.Width, contentSize.Width);
                currentSize.Height += contentSize.Height;
                return currentSize;
            }
            currentSize.Width += contentSize.Width;
            currentSize.Height = Math.Max(currentSize.Height, contentSize.Height);
            return currentSize;
        }

        public static Rectangle Align(Size alignThis, Rectangle withinThis, ContentAlignment align)
        {
            return VAlign(alignThis, HAlign(alignThis, withinThis, align), align);
        }

        public static Rectangle Align(Size alignThis, Rectangle withinThis, AnchorStyles anchorStyles)
        {
            return VAlign(alignThis, HAlign(alignThis, withinThis, anchorStyles), anchorStyles);
        }

        public static Rectangle AlignAndStretch(Size fitThis, Rectangle withinThis, AnchorStyles anchorStyles)
        {
            return Align(Stretch(fitThis, withinThis.Size, anchorStyles), withinThis, anchorStyles);
        }

        public static bool AreWidthAndHeightLarger(Size size1, Size size2)
        {
            return ((size1.Width >= size2.Width) && (size1.Height >= size2.Height));
        }

        public static Padding ClampNegativePaddingToZero(Padding padding)
        {
            if (padding.All < 0)
            {
                padding.Left = Math.Max(0, padding.Left);
                padding.Top = Math.Max(0, padding.Top);
                padding.Right = Math.Max(0, padding.Right);
                padding.Bottom = Math.Max(0, padding.Bottom);
            }
            return padding;
        }

        public static int ContentAlignmentToIndex(ContentAlignment alignment)
        {
            int num = xContentAlignmentToIndex(((int) alignment) & 15);
            int num2 = xContentAlignmentToIndex((((int) alignment) >> 4) & 15);
            int num3 = xContentAlignmentToIndex((((int) alignment) >> 8) & 15);
            int num4 = (((((num2 != 0) ? 4 : 0) | ((num3 != 0) ? 8 : 0)) | num) | num2) | num3;
            num4--;
            return num4;
        }

        public static Size ConvertZeroToUnbounded(Size size)
        {
            if (size.Width == 0)
            {
                size.Width = 0x7fffffff;
            }
            if (size.Height == 0)
            {
                size.Height = 0x7fffffff;
            }
            return size;
        }

        public static Rectangle DeflateRect(Rectangle rect, Padding padding)
        {
            rect.X += padding.Left;
            rect.Y += padding.Top;
            rect.Width -= padding.Horizontal;
            rect.Height -= padding.Vertical;
            return rect;
        }

        public static void ExpandRegionsToFillBounds(Rectangle bounds, AnchorStyles region1Align, ref Rectangle region1, ref Rectangle region2)
        {
            switch (region1Align)
            {
                case AnchorStyles.Top:
                    region1 = SubstituteSpecifiedBounds(bounds, region1, AnchorStyles.Bottom);
                    region2 = SubstituteSpecifiedBounds(bounds, region2, AnchorStyles.Top);
                    return;

                case AnchorStyles.Bottom:
                    region1 = SubstituteSpecifiedBounds(bounds, region1, AnchorStyles.Top);
                    region2 = SubstituteSpecifiedBounds(bounds, region2, AnchorStyles.Bottom);
                    break;

                case (AnchorStyles.Bottom | AnchorStyles.Top):
                    break;

                case AnchorStyles.Left:
                    region1 = SubstituteSpecifiedBounds(bounds, region1, AnchorStyles.Right);
                    region2 = SubstituteSpecifiedBounds(bounds, region2, AnchorStyles.Left);
                    return;

                case AnchorStyles.Right:
                    region1 = SubstituteSpecifiedBounds(bounds, region1, AnchorStyles.Left);
                    region2 = SubstituteSpecifiedBounds(bounds, region2, AnchorStyles.Right);
                    return;

                default:
                    return;
            }
        }

        public static Padding FlipPadding(Padding padding)
        {
            if (padding.All == -1)
            {
                int top = padding.Top;
                padding.Top = padding.Left;
                padding.Left = top;
                top = padding.Bottom;
                padding.Bottom = padding.Right;
                padding.Right = top;
            }
            return padding;
        }

        public static Point FlipPoint(Point point)
        {
            int x = point.X;
            point.X = point.Y;
            point.Y = x;
            return point;
        }

        public static Rectangle FlipRectangle(Rectangle rect)
        {
            rect.Location = FlipPoint(rect.Location);
            rect.Size = FlipSize(rect.Size);
            return rect;
        }

        public static Rectangle FlipRectangleIf(bool condition, Rectangle rect)
        {
            if (!condition)
            {
                return rect;
            }
            return FlipRectangle(rect);
        }

        public static Size FlipSize(Size size)
        {
            int width = size.Width;
            size.Width = size.Height;
            size.Height = width;
            return size;
        }

        public static Size FlipSizeIf(bool condition, Size size)
        {
            if (!condition)
            {
                return size;
            }
            return FlipSize(size);
        }

        private static AnchorStyles GetOppositeAnchor(AnchorStyles anchor)
        {
            AnchorStyles none = AnchorStyles.None;
            if (anchor != AnchorStyles.None)
            {
                for (int i = 1; i <= 8; i = i << 1)
                {
                    switch ((anchor & i))
                    {
                        case AnchorStyles.Top:
                            none |= AnchorStyles.Bottom;
                            break;

                        case AnchorStyles.Bottom:
                            none |= AnchorStyles.Top;
                            break;

                        case AnchorStyles.Left:
                            none |= AnchorStyles.Right;
                            break;

                        case AnchorStyles.Right:
                            none |= AnchorStyles.Left;
                            break;
                    }
                }
            }
            return none;
        }

        public static TextImageRelation GetOppositeTextImageRelation(TextImageRelation relation)
        {
            return (TextImageRelation) GetOppositeAnchor((AnchorStyles) relation);
        }

        internal static AnchorStyles GetUnifiedAnchor(IArrangedElement element)
        {
            DockStyle dock = DefaultLayout.GetDock(element);
            if (dock != DockStyle.None)
            {
                return dockingToAnchor[(int) dock];
            }
            return DefaultLayout.GetAnchor(element);
        }

        private static Rectangle HAlign(Size alignThis, Rectangle withinThis, ContentAlignment align)
        {
            if ((align & (ContentAlignment.BottomRight | ContentAlignment.MiddleRight | ContentAlignment.TopRight)) != ((ContentAlignment) 0))
            {
                withinThis.X += withinThis.Width - alignThis.Width;
            }
            else if ((align & (ContentAlignment.BottomCenter | ContentAlignment.MiddleCenter | ContentAlignment.TopCenter)) != ((ContentAlignment) 0))
            {
                withinThis.X += (withinThis.Width - alignThis.Width) / 2;
            }
            withinThis.Width = alignThis.Width;
            return withinThis;
        }

        public static Rectangle HAlign(Size alignThis, Rectangle withinThis, AnchorStyles anchorStyles)
        {
            if ((anchorStyles & AnchorStyles.Right) != AnchorStyles.None)
            {
                withinThis.X += withinThis.Width - alignThis.Width;
            }
            else if ((anchorStyles == AnchorStyles.None) || ((anchorStyles & (AnchorStyles.Right | AnchorStyles.Left)) == AnchorStyles.None))
            {
                withinThis.X += (withinThis.Width - alignThis.Width) / 2;
            }
            withinThis.Width = alignThis.Width;
            return withinThis;
        }

        public static Rectangle InflateRect(Rectangle rect, Padding padding)
        {
            rect.X -= padding.Left;
            rect.Y -= padding.Top;
            rect.Width += padding.Horizontal;
            rect.Height += padding.Vertical;
            return rect;
        }

        public static Size IntersectSizes(Size a, Size b)
        {
            return new Size(Math.Min(a.Width, b.Width), Math.Min(a.Height, b.Height));
        }

        public static bool IsHorizontalAlignment(ContentAlignment align)
        {
            return !IsVerticalAlignment(align);
        }

        public static bool IsHorizontalRelation(TextImageRelation relation)
        {
            return ((relation & (TextImageRelation.TextBeforeImage | TextImageRelation.ImageBeforeText)) != TextImageRelation.Overlay);
        }

        public static bool IsIntersectHorizontally(Rectangle rect1, Rectangle rect2)
        {
            if (!rect1.IntersectsWith(rect2))
            {
                return false;
            }
            return (((rect1.X <= rect2.X) && ((rect1.X + rect1.Width) >= (rect2.X + rect2.Width))) || ((rect2.X <= rect1.X) && ((rect2.X + rect2.Width) >= (rect1.X + rect1.Width))));
        }

        public static bool IsIntersectVertically(Rectangle rect1, Rectangle rect2)
        {
            if (!rect1.IntersectsWith(rect2))
            {
                return false;
            }
            return (((rect1.Y <= rect2.Y) && ((rect1.Y + rect1.Width) >= (rect2.Y + rect2.Width))) || ((rect2.Y <= rect1.Y) && ((rect2.Y + rect2.Width) >= (rect1.Y + rect1.Width))));
        }

        public static bool IsVerticalAlignment(ContentAlignment align)
        {
            return ((align & (ContentAlignment.BottomCenter | ContentAlignment.TopCenter)) != ((ContentAlignment) 0));
        }

        public static bool IsVerticalRelation(TextImageRelation relation)
        {
            return ((relation & (TextImageRelation.TextAboveImage | TextImageRelation.ImageAboveText)) != TextImageRelation.Overlay);
        }

        public static bool IsZeroWidthOrHeight(Rectangle rectangle)
        {
            if (rectangle.Width != 0)
            {
                return (rectangle.Height == 0);
            }
            return true;
        }

        public static bool IsZeroWidthOrHeight(Size size)
        {
            if (size.Width != 0)
            {
                return (size.Height == 0);
            }
            return true;
        }

        public static Size OldGetLargestStringSizeInCollection(Font font, ICollection objects)
        {
            Size empty = Size.Empty;
            if (objects != null)
            {
                foreach (object obj2 in objects)
                {
                    Size size2 = TextRenderer.MeasureText(obj2.ToString(), font, new Size(0x7fff, 0x7fff), TextFormatFlags.SingleLine);
                    empty.Width = Math.Max(empty.Width, size2.Width);
                    empty.Height = Math.Max(empty.Height, size2.Height);
                }
            }
            return empty;
        }

        public static Rectangle RTLTranslate(Rectangle bounds, Rectangle withinBounds)
        {
            bounds.X = withinBounds.Width - bounds.Right;
            return bounds;
        }

        public static void SplitRegion(Rectangle bounds, Size specifiedContent, AnchorStyles region1Align, out Rectangle region1, out Rectangle region2)
        {
            region1 = region2 = bounds;
            switch (region1Align)
            {
                case AnchorStyles.Top:
                    region1.Height = specifiedContent.Height;
                    region2.Y += specifiedContent.Height;
                    region2.Height -= specifiedContent.Height;
                    return;

                case AnchorStyles.Bottom:
                    region1.Y += bounds.Height - specifiedContent.Height;
                    region1.Height = specifiedContent.Height;
                    region2.Height -= specifiedContent.Height;
                    break;

                case (AnchorStyles.Bottom | AnchorStyles.Top):
                    break;

                case AnchorStyles.Left:
                    region1.Width = specifiedContent.Width;
                    region2.X += specifiedContent.Width;
                    region2.Width -= specifiedContent.Width;
                    return;

                case AnchorStyles.Right:
                    region1.X += bounds.Width - specifiedContent.Width;
                    region1.Width = specifiedContent.Width;
                    region2.Width -= specifiedContent.Width;
                    return;

                default:
                    return;
            }
        }

        public static Size Stretch(Size stretchThis, Size withinThis, AnchorStyles anchorStyles)
        {
            Size size = new Size(((anchorStyles & (AnchorStyles.Right | AnchorStyles.Left)) == (AnchorStyles.Right | AnchorStyles.Left)) ? withinThis.Width : stretchThis.Width, ((anchorStyles & (AnchorStyles.Bottom | AnchorStyles.Top)) == (AnchorStyles.Bottom | AnchorStyles.Top)) ? withinThis.Height : stretchThis.Height);
            if (size.Width > withinThis.Width)
            {
                size.Width = withinThis.Width;
            }
            if (size.Height > withinThis.Height)
            {
                size.Height = withinThis.Height;
            }
            return size;
        }

        public static Size SubAlignedRegion(Size currentSize, Size contentSize, TextImageRelation relation)
        {
            return SubAlignedRegionCore(currentSize, contentSize, IsVerticalRelation(relation));
        }

        public static Size SubAlignedRegionCore(Size currentSize, Size contentSize, bool vertical)
        {
            if (vertical)
            {
                currentSize.Height -= contentSize.Height;
                return currentSize;
            }
            currentSize.Width -= contentSize.Width;
            return currentSize;
        }

        private static Rectangle SubstituteSpecifiedBounds(Rectangle originalBounds, Rectangle substitutionBounds, AnchorStyles specified)
        {
            int left = ((specified & AnchorStyles.Left) != AnchorStyles.None) ? substitutionBounds.Left : originalBounds.Left;
            int top = ((specified & AnchorStyles.Top) != AnchorStyles.None) ? substitutionBounds.Top : originalBounds.Top;
            int right = ((specified & AnchorStyles.Right) != AnchorStyles.None) ? substitutionBounds.Right : originalBounds.Right;
            int bottom = ((specified & AnchorStyles.Bottom) != AnchorStyles.None) ? substitutionBounds.Bottom : originalBounds.Bottom;
            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        public static Size UnionSizes(Size a, Size b)
        {
            return new Size(Math.Max(a.Width, b.Width), Math.Max(a.Height, b.Height));
        }

        public static Rectangle VAlign(Size alignThis, Rectangle withinThis, ContentAlignment align)
        {
            if ((align & (ContentAlignment.BottomRight | ContentAlignment.BottomCenter | ContentAlignment.BottomLeft)) != ((ContentAlignment) 0))
            {
                withinThis.Y += withinThis.Height - alignThis.Height;
            }
            else if ((align & (ContentAlignment.MiddleRight | ContentAlignment.MiddleCenter | ContentAlignment.MiddleLeft)) != ((ContentAlignment) 0))
            {
                withinThis.Y += (withinThis.Height - alignThis.Height) / 2;
            }
            withinThis.Height = alignThis.Height;
            return withinThis;
        }

        public static Rectangle VAlign(Size alignThis, Rectangle withinThis, AnchorStyles anchorStyles)
        {
            if ((anchorStyles & AnchorStyles.Bottom) != AnchorStyles.None)
            {
                withinThis.Y += withinThis.Height - alignThis.Height;
            }
            else if ((anchorStyles == AnchorStyles.None) || ((anchorStyles & (AnchorStyles.Bottom | AnchorStyles.Top)) == AnchorStyles.None))
            {
                withinThis.Y += (withinThis.Height - alignThis.Height) / 2;
            }
            withinThis.Height = alignThis.Height;
            return withinThis;
        }

        private static byte xContentAlignmentToIndex(int threeBitFlag)
        {
            return ((threeBitFlag == 4) ? ((byte) 3) : ((byte) threeBitFlag));
        }

        public sealed class MeasureTextCache
        {
            private const int MaxCacheSize = 6;
            private int nextCacheEntry = -1;
            private PreferredSizeCache[] sizeCacheList;
            private Size unconstrainedPreferredSize = LayoutUtils.InvalidSize;

            public Size GetTextSize(string text, Font font, Size proposedConstraints, TextFormatFlags flags)
            {
                if (!this.TextRequiresWordBreak(text, font, proposedConstraints, flags))
                {
                    return this.unconstrainedPreferredSize;
                }
                if (this.sizeCacheList == null)
                {
                    this.sizeCacheList = new PreferredSizeCache[6];
                }
                foreach (PreferredSizeCache cache in this.sizeCacheList)
                {
                    if (cache.ConstrainingSize == proposedConstraints)
                    {
                        return cache.PreferredSize;
                    }
                    if ((cache.ConstrainingSize.Width == proposedConstraints.Width) && (cache.PreferredSize.Height <= proposedConstraints.Height))
                    {
                        return cache.PreferredSize;
                    }
                }
                Size preferredSize = TextRenderer.MeasureText(text, font, proposedConstraints, flags);
                this.nextCacheEntry = (this.nextCacheEntry + 1) % 6;
                this.sizeCacheList[this.nextCacheEntry] = new PreferredSizeCache(proposedConstraints, preferredSize);
                return preferredSize;
            }

            private Size GetUnconstrainedSize(string text, Font font, TextFormatFlags flags)
            {
                if (this.unconstrainedPreferredSize == LayoutUtils.InvalidSize)
                {
                    flags &= ~TextFormatFlags.WordBreak;
                    this.unconstrainedPreferredSize = TextRenderer.MeasureText(text, font, LayoutUtils.MaxSize, flags);
                }
                return this.unconstrainedPreferredSize;
            }

            public void InvalidateCache()
            {
                this.unconstrainedPreferredSize = LayoutUtils.InvalidSize;
                this.sizeCacheList = null;
            }

            public bool TextRequiresWordBreak(string text, Font font, Size size, TextFormatFlags flags)
            {
                return (this.GetUnconstrainedSize(text, font, flags).Width > size.Width);
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct PreferredSizeCache
            {
                public Size ConstrainingSize;
                public Size PreferredSize;
                public PreferredSizeCache(Size constrainingSize, Size preferredSize)
                {
                    this.ConstrainingSize = constrainingSize;
                    this.PreferredSize = preferredSize;
                }
            }
        }
    }
}

