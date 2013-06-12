namespace System.Windows.Forms.Layout
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal class TableLayout : LayoutEngine
    {
        private static readonly int _containerInfoProperty = PropertyStore.CreateKey();
        private static readonly int _layoutInfoProperty = PropertyStore.CreateKey();
        private static string[] _propertiesWhichInvalidateCache;
        internal static readonly TableLayout Instance = new TableLayout();

        static TableLayout()
        {
            string[] strArray = new string[9];
            strArray[1] = PropertyNames.ChildIndex;
            strArray[2] = PropertyNames.Parent;
            strArray[3] = PropertyNames.Visible;
            strArray[4] = PropertyNames.Items;
            strArray[5] = PropertyNames.Rows;
            strArray[6] = PropertyNames.Columns;
            strArray[7] = PropertyNames.RowStyles;
            strArray[8] = PropertyNames.ColumnStyles;
            _propertiesWhichInvalidateCache = strArray;
        }

        private void AdvanceUntilFits(int maxColumns, ReservationGrid reservationGrid, LayoutInfo layoutInfo, out int colStop)
        {
            int rowStart = layoutInfo.RowStart;
            do
            {
                this.GetColStartAndStop(maxColumns, reservationGrid, layoutInfo, out colStop);
            }
            while (this.ScanRowForOverlap(maxColumns, reservationGrid, layoutInfo, colStop, layoutInfo.RowStart - rowStart));
        }

        private Size ApplyStyles(ContainerInfo containerInfo, Size proposedConstraints, bool measureOnly)
        {
            Size empty = Size.Empty;
            this.InitializeStrips(containerInfo.Columns, containerInfo.ColumnStyles);
            this.InitializeStrips(containerInfo.Rows, containerInfo.RowStyles);
            containerInfo.ChildHasColumnSpan = false;
            containerInfo.ChildHasRowSpan = false;
            foreach (LayoutInfo info in containerInfo.ChildrenInfo)
            {
                containerInfo.Columns[info.ColumnStart].IsStart = true;
                containerInfo.Rows[info.RowStart].IsStart = true;
                if (info.ColumnSpan > 1)
                {
                    containerInfo.ChildHasColumnSpan = true;
                }
                if (info.RowSpan > 1)
                {
                    containerInfo.ChildHasRowSpan = true;
                }
            }
            empty.Width = this.InflateColumns(containerInfo, proposedConstraints, measureOnly);
            int expandLastElementWidth = Math.Max(0, proposedConstraints.Width - empty.Width);
            empty.Height = this.InflateRows(containerInfo, proposedConstraints, expandLastElementWidth, measureOnly);
            return empty;
        }

        private void AssignRowsAndColumns(ContainerInfo containerInfo)
        {
            int maxColumns = containerInfo.MaxColumns;
            int maxRows = containerInfo.MaxRows;
            LayoutInfo[] childrenInfo = containerInfo.ChildrenInfo;
            int minRowsAndColumns = containerInfo.MinRowsAndColumns;
            int minColumns = containerInfo.MinColumns;
            int minRows = containerInfo.MinRows;
            TableLayoutPanelGrowStyle growStyle = containerInfo.GrowStyle;
            if (growStyle == TableLayoutPanelGrowStyle.FixedSize)
            {
                if (containerInfo.MinRowsAndColumns > (maxColumns * maxRows))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("TableLayoutPanelFullDesc"));
                }
                if ((minColumns > maxColumns) || (minRows > maxRows))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("TableLayoutPanelSpanDesc"));
                }
                maxRows = Math.Max(1, maxRows);
                maxColumns = Math.Max(1, maxColumns);
            }
            else if (growStyle == TableLayoutPanelGrowStyle.AddRows)
            {
                maxRows = 0;
            }
            else
            {
                maxColumns = 0;
            }
            if (maxColumns > 0)
            {
                this.xAssignRowsAndColumns(containerInfo, childrenInfo, maxColumns, (maxRows == 0) ? 0x7fffffff : maxRows, growStyle);
            }
            else if (maxRows > 0)
            {
                for (int i = Math.Max(Math.Max((int) Math.Ceiling((double) (((float) minRowsAndColumns) / ((float) maxRows))), minColumns), 1); !this.xAssignRowsAndColumns(containerInfo, childrenInfo, i, maxRows, growStyle); i++)
                {
                }
            }
            else
            {
                this.xAssignRowsAndColumns(containerInfo, childrenInfo, Math.Max(minColumns, 1), 0x7fffffff, growStyle);
            }
        }

        internal static void ClearCachedAssignments(ContainerInfo containerInfo)
        {
            containerInfo.Valid = false;
        }

        internal static TableLayoutSettings CreateSettings(IArrangedElement owner)
        {
            return new TableLayoutSettings(owner);
        }

        [Conditional("DEBUG_LAYOUT")]
        private void Debug_VerifyAssignmentsAreCurrent(IArrangedElement container, ContainerInfo containerInfo)
        {
        }

        [Conditional("DEBUG_LAYOUT")]
        private void Debug_VerifyNoOverlapping(IArrangedElement container)
        {
            ArrayList list = new ArrayList(container.Children.Count);
            ContainerInfo containerInfo = GetContainerInfo(container);
            Strip[] rows = containerInfo.Rows;
            Strip[] columns = containerInfo.Columns;
            foreach (IArrangedElement element in container.Children)
            {
                if (element.ParticipatesInLayout)
                {
                    list.Add(GetLayoutInfo(element));
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                LayoutInfo info2 = (LayoutInfo) list[i];
                Rectangle bounds = info2.Element.Bounds;
                new Rectangle(info2.ColumnStart, info2.RowStart, info2.ColumnSpan, info2.RowSpan);
                for (int j = i + 1; j < list.Count; j++)
                {
                    LayoutInfo info3 = (LayoutInfo) list[j];
                    Rectangle rectangle2 = info3.Element.Bounds;
                    new Rectangle(info3.ColumnStart, info3.RowStart, info3.ColumnSpan, info3.RowSpan);
                    if (LayoutUtils.IsIntersectHorizontally(bounds, rectangle2))
                    {
                        int columnStart = info2.ColumnStart;
                        while (columnStart < (info2.ColumnStart + info2.ColumnSpan))
                        {
                            columnStart++;
                        }
                        for (columnStart = info3.ColumnStart; columnStart < (info3.ColumnStart + info3.ColumnSpan); columnStart++)
                        {
                        }
                    }
                    if (LayoutUtils.IsIntersectVertically(bounds, rectangle2))
                    {
                        int rowStart = info2.RowStart;
                        while (rowStart < (info2.RowStart + info2.RowSpan))
                        {
                            rowStart++;
                        }
                        for (rowStart = info3.RowStart; rowStart < (info3.RowStart + info3.RowSpan); rowStart++)
                        {
                        }
                    }
                }
            }
        }

        private void DistributeSize(IList styles, Strip[] strips, int start, int stop, int min, int max, int cellBorderWidth)
        {
            this.xDistributeSize(styles, strips, start, stop, min, MinSizeProxy.GetInstance, cellBorderWidth);
            this.xDistributeSize(styles, strips, start, stop, max, MaxSizeProxy.GetInstance, cellBorderWidth);
        }

        private int DistributeStyles(int cellBorderWidth, IList styles, Strip[] strips, int maxSize, bool dontHonorConstraint)
        {
            int num = 0;
            float num2 = 0f;
            float num3 = 0f;
            float num4 = 0f;
            float num5 = 0f;
            bool flag = false;
            for (int i = 0; i < strips.Length; i++)
            {
                Strip strip = strips[i];
                if (i < styles.Count)
                {
                    TableLayoutStyle style = (TableLayoutStyle) styles[i];
                    switch (style.SizeType)
                    {
                        case SizeType.Absolute:
                            num5 += strip.MinSize;
                            goto Label_00A5;

                        case SizeType.Percent:
                            num3 += style.Size;
                            num4 += strip.MinSize;
                            goto Label_00A5;
                    }
                    num5 += strip.MinSize;
                    flag = true;
                }
                else
                {
                    flag = true;
                }
            Label_00A5:
                strip.MaxSize += cellBorderWidth;
                strip.MinSize += cellBorderWidth;
                strips[i] = strip;
                num += strip.MinSize;
            }
            int num7 = maxSize - num;
            if (num3 > 0f)
            {
                if (!dontHonorConstraint)
                {
                    if (num4 > (maxSize - num5))
                    {
                        num4 = Math.Max((float) 0f, (float) (maxSize - num5));
                    }
                    if (num7 > 0)
                    {
                        num4 += num7;
                    }
                    else if (num7 < 0)
                    {
                        num4 = (maxSize - num5) - (strips.Length * cellBorderWidth);
                        num7 = 0;
                    }
                    for (int j = 0; j < strips.Length; j++)
                    {
                        Strip strip2 = strips[j];
                        SizeType type = (j < styles.Count) ? ((TableLayoutStyle) styles[j]).SizeType : SizeType.AutoSize;
                        if (type == SizeType.Percent)
                        {
                            TableLayoutStyle style2 = (TableLayoutStyle) styles[j];
                            int num9 = (int) ((style2.Size * num4) / num3);
                            num -= strip2.MinSize;
                            num += num9 + cellBorderWidth;
                            strip2.MinSize = num9 + cellBorderWidth;
                            strips[j] = strip2;
                        }
                    }
                }
                else
                {
                    int num10 = 0;
                    for (int k = 0; k < strips.Length; k++)
                    {
                        Strip strip3 = strips[k];
                        SizeType type2 = (k < styles.Count) ? ((TableLayoutStyle) styles[k]).SizeType : SizeType.AutoSize;
                        if (type2 == SizeType.Percent)
                        {
                            TableLayoutStyle style3 = (TableLayoutStyle) styles[k];
                            int num12 = (int) Math.Round((double) ((strip3.MinSize * num3) / style3.Size));
                            num10 = Math.Max(num10, num12);
                            num -= strip3.MinSize;
                        }
                    }
                    num += num10;
                }
            }
            num7 = maxSize - num;
            if (flag && (num7 > 0))
            {
                if (num7 < num2)
                {
                    float single1 = ((float) num7) / num2;
                }
                num7 -= (int) Math.Ceiling((double) num2);
                for (int m = 0; m < strips.Length; m++)
                {
                    Strip strip4 = strips[m];
                    if (((m < styles.Count) ? ((TableLayoutStyle) styles[m]).SizeType : SizeType.AutoSize) == SizeType.AutoSize)
                    {
                        int num14 = Math.Min(strip4.MaxSize - strip4.MinSize, num7);
                        if (num14 > 0)
                        {
                            num += num14;
                            num7 -= num14;
                            strip4.MinSize += num14;
                            strips[m] = strip4;
                        }
                    }
                }
            }
            return num;
        }

        private void EnsureRowAndColumnAssignments(IArrangedElement container, ContainerInfo containerInfo, bool doNotCache)
        {
            if (!HasCachedAssignments(containerInfo) || doNotCache)
            {
                this.AssignRowsAndColumns(containerInfo);
            }
        }

        private void ExpandLastElement(ContainerInfo containerInfo, Size usedSpace, Size totalSpace)
        {
            Strip[] rows = containerInfo.Rows;
            Strip[] columns = containerInfo.Columns;
            if ((columns.Length != 0) && (totalSpace.Width > usedSpace.Width))
            {
                columns[columns.Length - 1].MinSize += totalSpace.Width - usedSpace.Width;
            }
            if ((rows.Length != 0) && (totalSpace.Height > usedSpace.Height))
            {
                rows[rows.Length - 1].MinSize += totalSpace.Height - usedSpace.Height;
            }
        }

        private void GetColStartAndStop(int maxColumns, ReservationGrid reservationGrid, LayoutInfo layoutInfo, out int colStop)
        {
            colStop = layoutInfo.ColumnStart + layoutInfo.ColumnSpan;
            if (colStop > maxColumns)
            {
                if (layoutInfo.ColumnStart != 0)
                {
                    layoutInfo.ColumnStart = 0;
                    layoutInfo.RowStart++;
                }
                colStop = Math.Min(layoutInfo.ColumnSpan, maxColumns);
            }
        }

        internal static ContainerInfo GetContainerInfo(IArrangedElement container)
        {
            ContainerInfo info = (ContainerInfo) container.Properties.GetObject(_containerInfoProperty);
            if (info == null)
            {
                info = new ContainerInfo(container);
                container.Properties.SetObject(_containerInfoProperty, info);
            }
            return info;
        }

        internal IArrangedElement GetControlFromPosition(IArrangedElement container, int column, int row)
        {
            if (row < 0)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "RowPosition", row.ToString(CultureInfo.CurrentCulture) }));
            }
            if (column < 0)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "ColumnPosition", column.ToString(CultureInfo.CurrentCulture) }));
            }
            ArrangedElementCollection children = container.Children;
            ContainerInfo containerInfo = GetContainerInfo(container);
            if ((children != null) && (children.Count != 0))
            {
                if (!containerInfo.Valid)
                {
                    this.EnsureRowAndColumnAssignments(container, containerInfo, true);
                }
                for (int i = 0; i < children.Count; i++)
                {
                    LayoutInfo layoutInfo = GetLayoutInfo(children[i]);
                    if (((layoutInfo.ColumnStart <= column) && (((layoutInfo.ColumnStart + layoutInfo.ColumnSpan) - 1) >= column)) && ((layoutInfo.RowStart <= row) && (((layoutInfo.RowStart + layoutInfo.RowSpan) - 1) >= row)))
                    {
                        return layoutInfo.Element;
                    }
                }
            }
            return null;
        }

        private Size GetElementSize(IArrangedElement element, Size proposedConstraints)
        {
            if (CommonProperties.GetAutoSize(element))
            {
                return element.GetPreferredSize(proposedConstraints);
            }
            return CommonProperties.GetSpecifiedBounds(element).Size;
        }

        internal static LayoutInfo GetLayoutInfo(IArrangedElement element)
        {
            LayoutInfo info = (LayoutInfo) element.Properties.GetObject(_layoutInfoProperty);
            if (info == null)
            {
                info = new LayoutInfo(element);
                SetLayoutInfo(element, info);
            }
            return info;
        }

        private static LayoutInfo GetNextLayoutInfo(LayoutInfo[] layoutInfo, ref int index, bool absolutelyPositioned)
        {
            for (int i = ++index; i < layoutInfo.Length; i++)
            {
                if (absolutelyPositioned == layoutInfo[i].IsAbsolutelyPositioned)
                {
                    index = i;
                    return layoutInfo[i];
                }
            }
            index = layoutInfo.Length;
            return null;
        }

        internal TableLayoutPanelCellPosition GetPositionFromControl(IArrangedElement container, IArrangedElement child)
        {
            if ((container == null) || (child == null))
            {
                return new TableLayoutPanelCellPosition(-1, -1);
            }
            ArrangedElementCollection children = container.Children;
            ContainerInfo containerInfo = GetContainerInfo(container);
            if ((children == null) || (children.Count == 0))
            {
                return new TableLayoutPanelCellPosition(-1, -1);
            }
            if (!containerInfo.Valid)
            {
                this.EnsureRowAndColumnAssignments(container, containerInfo, true);
            }
            LayoutInfo layoutInfo = GetLayoutInfo(child);
            return new TableLayoutPanelCellPosition(layoutInfo.ColumnStart, layoutInfo.RowStart);
        }

        internal override Size GetPreferredSize(IArrangedElement container, Size proposedConstraints)
        {
            ContainerInfo containerInfo = GetContainerInfo(container);
            bool isValid = false;
            float size = -1f;
            Size cachedPreferredSize = containerInfo.GetCachedPreferredSize(proposedConstraints, out isValid);
            if (isValid)
            {
                return cachedPreferredSize;
            }
            ContainerInfo info2 = new ContainerInfo(containerInfo);
            int cellBorderWidth = containerInfo.CellBorderWidth;
            if (((containerInfo.MaxColumns == 1) && (containerInfo.ColumnStyles.Count > 0)) && (containerInfo.ColumnStyles[0].SizeType == SizeType.Absolute))
            {
                Size size2 = container.DisplayRectangle.Size - new Size(cellBorderWidth * 2, cellBorderWidth * 2);
                size2.Width = Math.Max(size2.Width, 1);
                size2.Height = Math.Max(size2.Height, 1);
                size = containerInfo.ColumnStyles[0].Size;
                containerInfo.ColumnStyles[0].SetSize(Math.Max(size, (float) Math.Min(proposedConstraints.Width, size2.Width)));
            }
            this.EnsureRowAndColumnAssignments(container, info2, true);
            Size size3 = new Size(cellBorderWidth, cellBorderWidth);
            proposedConstraints -= size3;
            proposedConstraints.Width = Math.Max(proposedConstraints.Width, 1);
            proposedConstraints.Height = Math.Max(proposedConstraints.Height, 1);
            if (((info2.Columns != null) && (containerInfo.Columns != null)) && (info2.Columns.Length != containerInfo.Columns.Length))
            {
                ClearCachedAssignments(containerInfo);
            }
            if (((info2.Rows != null) && (containerInfo.Rows != null)) && (info2.Rows.Length != containerInfo.Rows.Length))
            {
                ClearCachedAssignments(containerInfo);
            }
            cachedPreferredSize = this.ApplyStyles(info2, proposedConstraints, true);
            if (size >= 0f)
            {
                containerInfo.ColumnStyles[0].SetSize(size);
            }
            return (cachedPreferredSize + size3);
        }

        internal static bool HasCachedAssignments(ContainerInfo containerInfo)
        {
            return containerInfo.Valid;
        }

        private int InflateColumns(ContainerInfo containerInfo, Size proposedConstraints, bool measureOnly)
        {
            bool dontHonorConstraint = measureOnly;
            LayoutInfo[] childrenInfo = containerInfo.ChildrenInfo;
            if (containerInfo.ChildHasColumnSpan)
            {
                Array.Sort(childrenInfo, ColumnSpanComparer.GetInstance);
            }
            if (dontHonorConstraint && (proposedConstraints.Width < 0x7fff))
            {
                TableLayoutPanel container = containerInfo.Container as TableLayoutPanel;
                if (((container != null) && (container.ParentInternal != null)) && (container.ParentInternal.LayoutEngine == DefaultLayout.Instance))
                {
                    if (((container.Dock == DockStyle.Top) || (container.Dock == DockStyle.Bottom)) || (container.Dock == DockStyle.Fill))
                    {
                        dontHonorConstraint = false;
                    }
                    if ((container.Anchor & (AnchorStyles.Right | AnchorStyles.Left)) == (AnchorStyles.Right | AnchorStyles.Left))
                    {
                        dontHonorConstraint = false;
                    }
                }
            }
            foreach (LayoutInfo info in childrenInfo)
            {
                IArrangedElement element = info.Element;
                int columnSpan = info.ColumnSpan;
                if ((columnSpan > 1) || !this.IsAbsolutelySized(info.ColumnStart, containerInfo.ColumnStyles))
                {
                    int min = 0;
                    int max = 0;
                    if (((columnSpan == 1) && (info.RowSpan == 1)) && this.IsAbsolutelySized(info.RowStart, containerInfo.RowStyles))
                    {
                        int size = (int) containerInfo.RowStyles[info.RowStart].Size;
                        min = this.GetElementSize(element, new Size(0, size)).Width;
                        max = min;
                    }
                    else
                    {
                        min = this.GetElementSize(element, new Size(1, 0)).Width;
                        max = this.GetElementSize(element, Size.Empty).Width;
                    }
                    Padding margin = CommonProperties.GetMargin(element);
                    min += margin.Horizontal;
                    max += margin.Horizontal;
                    int stop = Math.Min(info.ColumnStart + info.ColumnSpan, containerInfo.Columns.Length);
                    this.DistributeSize(containerInfo.ColumnStyles, containerInfo.Columns, info.ColumnStart, stop, min, max, containerInfo.CellBorderWidth);
                }
            }
            int num6 = this.DistributeStyles(containerInfo.CellBorderWidth, containerInfo.ColumnStyles, containerInfo.Columns, proposedConstraints.Width, dontHonorConstraint);
            if ((!dontHonorConstraint || (num6 <= proposedConstraints.Width)) || (proposedConstraints.Width <= 1))
            {
                return num6;
            }
            Strip[] columns = containerInfo.Columns;
            float num7 = 0f;
            int num8 = 0;
            TableLayoutStyleCollection columnStyles = containerInfo.ColumnStyles;
            for (int i = 0; i < columns.Length; i++)
            {
                Strip strip = columns[i];
                if (i < columnStyles.Count)
                {
                    TableLayoutStyle style = columnStyles[i];
                    if (style.SizeType == SizeType.Percent)
                    {
                        num7 += style.Size;
                        num8 += strip.MinSize;
                    }
                }
            }
            int num10 = num6 - proposedConstraints.Width;
            int num11 = Math.Min(num10, num8);
            for (int j = 0; j < columns.Length; j++)
            {
                if (j < columnStyles.Count)
                {
                    TableLayoutStyle style2 = columnStyles[j];
                    if (style2.SizeType == SizeType.Percent)
                    {
                        float num13 = style2.Size / num7;
                        columns[j].MinSize -= (int) (num13 * num11);
                    }
                }
            }
            return (num6 - num11);
        }

        private int InflateRows(ContainerInfo containerInfo, Size proposedConstraints, int expandLastElementWidth, bool measureOnly)
        {
            bool dontHonorConstraint = measureOnly;
            LayoutInfo[] childrenInfo = containerInfo.ChildrenInfo;
            if (containerInfo.ChildHasRowSpan)
            {
                Array.Sort(childrenInfo, RowSpanComparer.GetInstance);
            }
            bool hasMultiplePercentColumns = containerInfo.HasMultiplePercentColumns;
            if (dontHonorConstraint && (proposedConstraints.Height < 0x7fff))
            {
                TableLayoutPanel container = containerInfo.Container as TableLayoutPanel;
                if (((container != null) && (container.ParentInternal != null)) && (container.ParentInternal.LayoutEngine == DefaultLayout.Instance))
                {
                    if (((container.Dock == DockStyle.Left) || (container.Dock == DockStyle.Right)) || (container.Dock == DockStyle.Fill))
                    {
                        dontHonorConstraint = false;
                    }
                    if ((container.Anchor & (AnchorStyles.Bottom | AnchorStyles.Top)) == (AnchorStyles.Bottom | AnchorStyles.Top))
                    {
                        dontHonorConstraint = false;
                    }
                }
            }
            foreach (LayoutInfo info in childrenInfo)
            {
                IArrangedElement element = info.Element;
                if ((info.RowSpan > 1) || !this.IsAbsolutelySized(info.RowStart, containerInfo.RowStyles))
                {
                    int num2 = this.SumStrips(containerInfo.Columns, info.ColumnStart, info.ColumnSpan);
                    if ((!dontHonorConstraint && ((info.ColumnStart + info.ColumnSpan) >= containerInfo.MaxColumns)) && !hasMultiplePercentColumns)
                    {
                        num2 += expandLastElementWidth;
                    }
                    Padding margin = CommonProperties.GetMargin(element);
                    int min = this.GetElementSize(element, new Size(num2 - margin.Horizontal, 0)).Height + margin.Vertical;
                    int max = min;
                    int stop = Math.Min(info.RowStart + info.RowSpan, containerInfo.Rows.Length);
                    this.DistributeSize(containerInfo.RowStyles, containerInfo.Rows, info.RowStart, stop, min, max, containerInfo.CellBorderWidth);
                }
            }
            return this.DistributeStyles(containerInfo.CellBorderWidth, containerInfo.RowStyles, containerInfo.Rows, proposedConstraints.Height, dontHonorConstraint);
        }

        private void InitializeStrips(Strip[] strips, IList styles)
        {
            for (int i = 0; i < strips.Length; i++)
            {
                TableLayoutStyle style = (i < styles.Count) ? ((TableLayoutStyle) styles[i]) : null;
                Strip strip = strips[i];
                if ((style != null) && (style.SizeType == SizeType.Absolute))
                {
                    strip.MinSize = (int) Math.Round((double) ((TableLayoutStyle) styles[i]).Size);
                    strip.MaxSize = strip.MinSize;
                }
                else
                {
                    strip.MinSize = 0;
                    strip.MaxSize = 0;
                }
                strip.IsStart = false;
                strips[i] = strip;
            }
        }

        private bool IsAbsolutelySized(int index, IList styles)
        {
            return ((index < styles.Count) && (((TableLayoutStyle) styles[index]).SizeType == SizeType.Absolute));
        }

        private bool IsCursorPastInsertionPoint(LayoutInfo fixedLayoutInfo, int insertionRow, int insertionCol)
        {
            return ((fixedLayoutInfo.RowPosition < insertionRow) || ((fixedLayoutInfo.RowPosition == insertionRow) && (fixedLayoutInfo.ColumnPosition < insertionCol)));
        }

        private bool IsOverlappingWithReservationGrid(LayoutInfo fixedLayoutInfo, ReservationGrid reservationGrid, int currentRow)
        {
            if (fixedLayoutInfo.RowPosition < currentRow)
            {
                return true;
            }
            for (int i = fixedLayoutInfo.RowPosition - currentRow; i < ((fixedLayoutInfo.RowPosition - currentRow) + fixedLayoutInfo.RowSpan); i++)
            {
                for (int j = fixedLayoutInfo.ColumnPosition; j < (fixedLayoutInfo.ColumnPosition + fixedLayoutInfo.ColumnSpan); j++)
                {
                    if (reservationGrid.IsReserved(j, i))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal override bool LayoutCore(IArrangedElement container, LayoutEventArgs args)
        {
            this.ProcessSuspendedLayoutEventArgs(container, args);
            ContainerInfo containerInfo = GetContainerInfo(container);
            this.EnsureRowAndColumnAssignments(container, containerInfo, false);
            int cellBorderWidth = containerInfo.CellBorderWidth;
            Size proposedConstraints = container.DisplayRectangle.Size - new Size(cellBorderWidth, cellBorderWidth);
            proposedConstraints.Width = Math.Max(proposedConstraints.Width, 1);
            proposedConstraints.Height = Math.Max(proposedConstraints.Height, 1);
            Size usedSpace = this.ApplyStyles(containerInfo, proposedConstraints, false);
            this.ExpandLastElement(containerInfo, usedSpace, proposedConstraints);
            RectangleF displayRectangle = container.DisplayRectangle;
            displayRectangle.Inflate(-(((float) cellBorderWidth) / 2f), ((float) -cellBorderWidth) / 2f);
            this.SetElementBounds(containerInfo, displayRectangle);
            CommonProperties.SetLayoutBounds(containerInfo.Container, new Size(this.SumStrips(containerInfo.Columns, 0, containerInfo.Columns.Length), this.SumStrips(containerInfo.Rows, 0, containerInfo.Rows.Length)));
            return CommonProperties.GetAutoSize(container);
        }

        internal override void ProcessSuspendedLayoutEventArgs(IArrangedElement container, LayoutEventArgs args)
        {
            ContainerInfo containerInfo = GetContainerInfo(container);
            foreach (string str in _propertiesWhichInvalidateCache)
            {
                if (object.ReferenceEquals(args.AffectedProperty, str))
                {
                    ClearCachedAssignments(containerInfo);
                    return;
                }
            }
        }

        private bool ScanRowForOverlap(int maxColumns, ReservationGrid reservationGrid, LayoutInfo layoutInfo, int stopCol, int rowOffset)
        {
            for (int i = layoutInfo.ColumnStart; i < stopCol; i++)
            {
                if (reservationGrid.IsReserved(i, rowOffset))
                {
                    layoutInfo.ColumnStart = i + 1;
                    while ((layoutInfo.ColumnStart < maxColumns) && reservationGrid.IsReserved(layoutInfo.ColumnStart, rowOffset))
                    {
                        layoutInfo.ColumnStart++;
                    }
                    return true;
                }
            }
            return false;
        }

        private void SetElementBounds(ContainerInfo containerInfo, RectangleF displayRectF)
        {
            int cellBorderWidth = containerInfo.CellBorderWidth;
            float y = displayRectF.Y;
            int index = 0;
            int num4 = 0;
            bool flag = false;
            Rectangle.Truncate(displayRectF);
            if (containerInfo.Container is Control)
            {
                Control container = containerInfo.Container as Control;
                flag = container.RightToLeft == RightToLeft.Yes;
            }
            LayoutInfo[] childrenInfo = containerInfo.ChildrenInfo;
            float num5 = flag ? displayRectF.Right : displayRectF.X;
            Array.Sort(childrenInfo, PostAssignedPositionComparer.GetInstance);
            for (int i = 0; i < childrenInfo.Length; i++)
            {
                LayoutInfo info = childrenInfo[i];
                IArrangedElement element = info.Element;
                if (num4 != info.RowStart)
                {
                    while (num4 < info.RowStart)
                    {
                        y += containerInfo.Rows[num4].MinSize;
                        num4++;
                    }
                    num5 = flag ? displayRectF.Right : displayRectF.X;
                    index = 0;
                }
                while (index < info.ColumnStart)
                {
                    if (flag)
                    {
                        num5 -= containerInfo.Columns[index].MinSize;
                    }
                    else
                    {
                        num5 += containerInfo.Columns[index].MinSize;
                    }
                    index++;
                }
                int num7 = index + info.ColumnSpan;
                int num8 = 0;
                while ((index < num7) && (index < containerInfo.Columns.Length))
                {
                    num8 += containerInfo.Columns[index].MinSize;
                    index++;
                }
                if (flag)
                {
                    num5 -= num8;
                }
                int num9 = num4 + info.RowSpan;
                int num10 = 0;
                for (int j = num4; (j < num9) && (j < containerInfo.Rows.Length); j++)
                {
                    num10 += containerInfo.Rows[j].MinSize;
                }
                Rectangle rect = new Rectangle((int) (num5 + (((float) cellBorderWidth) / 2f)), (int) (y + (((float) cellBorderWidth) / 2f)), num8 - cellBorderWidth, num10 - cellBorderWidth);
                Padding margin = CommonProperties.GetMargin(element);
                if (flag)
                {
                    int right = margin.Right;
                    margin.Right = margin.Left;
                    margin.Left = right;
                }
                rect = LayoutUtils.DeflateRect(rect, margin);
                rect.Width = Math.Max(rect.Width, 1);
                rect.Height = Math.Max(rect.Height, 1);
                AnchorStyles unifiedAnchor = LayoutUtils.GetUnifiedAnchor(element);
                Rectangle bounds = LayoutUtils.AlignAndStretch(this.GetElementSize(element, rect.Size), rect, unifiedAnchor);
                bounds.Width = Math.Min(rect.Width, bounds.Width);
                bounds.Height = Math.Min(rect.Height, bounds.Height);
                if (flag)
                {
                    bounds.X = rect.X + (rect.Right - bounds.Right);
                }
                element.SetBounds(bounds, BoundsSpecified.None);
                if (!flag)
                {
                    num5 += num8;
                }
            }
        }

        internal static void SetLayoutInfo(IArrangedElement element, LayoutInfo value)
        {
            element.Properties.SetObject(_layoutInfoProperty, value);
        }

        internal int SumStrips(Strip[] strips, int start, int span)
        {
            int num = 0;
            for (int i = start; i < Math.Min(start + span, strips.Length); i++)
            {
                Strip strip = strips[i];
                num += strip.MinSize;
            }
            return num;
        }

        private bool xAssignRowsAndColumns(ContainerInfo containerInfo, LayoutInfo[] childrenInfo, int maxColumns, int maxRows, TableLayoutPanelGrowStyle growStyle)
        {
            int num = 0;
            int num2 = 0;
            ReservationGrid reservationGrid = new ReservationGrid();
            int currentRow = 0;
            int num4 = 0;
            int index = -1;
            int num6 = -1;
            LayoutInfo[] fixedChildrenInfo = containerInfo.FixedChildrenInfo;
            LayoutInfo fixedLayoutInfo = GetNextLayoutInfo(fixedChildrenInfo, ref index, true);
            LayoutInfo layoutInfo = GetNextLayoutInfo(childrenInfo, ref num6, false);
            while ((fixedLayoutInfo != null) || (layoutInfo != null))
            {
                int num8;
                int colStop = num4;
                if (layoutInfo != null)
                {
                    layoutInfo.RowStart = currentRow;
                    layoutInfo.ColumnStart = num4;
                    this.AdvanceUntilFits(maxColumns, reservationGrid, layoutInfo, out colStop);
                    if (layoutInfo.RowStart >= maxRows)
                    {
                        return false;
                    }
                }
                if ((layoutInfo != null) && ((fixedLayoutInfo == null) || (!this.IsCursorPastInsertionPoint(fixedLayoutInfo, layoutInfo.RowStart, colStop) && !this.IsOverlappingWithReservationGrid(fixedLayoutInfo, reservationGrid, currentRow))))
                {
                    for (int i = 0; i < (layoutInfo.RowStart - currentRow); i++)
                    {
                        reservationGrid.AdvanceRow();
                    }
                    currentRow = layoutInfo.RowStart;
                    num8 = Math.Min(currentRow + layoutInfo.RowSpan, maxRows);
                    reservationGrid.ReserveAll(layoutInfo, num8, colStop);
                    layoutInfo = GetNextLayoutInfo(childrenInfo, ref num6, false);
                    goto Label_020C;
                }
                if (num4 >= maxColumns)
                {
                    num4 = 0;
                    currentRow++;
                    reservationGrid.AdvanceRow();
                }
                fixedLayoutInfo.RowStart = Math.Min(fixedLayoutInfo.RowPosition, maxRows - 1);
                fixedLayoutInfo.ColumnStart = Math.Min(fixedLayoutInfo.ColumnPosition, maxColumns - 1);
                if (currentRow > fixedLayoutInfo.RowStart)
                {
                    fixedLayoutInfo.ColumnStart = num4;
                }
                else if (currentRow == fixedLayoutInfo.RowStart)
                {
                    fixedLayoutInfo.ColumnStart = Math.Max(fixedLayoutInfo.ColumnStart, num4);
                }
                fixedLayoutInfo.RowStart = Math.Max(fixedLayoutInfo.RowStart, currentRow);
                int num10 = 0;
                while (num10 < (fixedLayoutInfo.RowStart - currentRow))
                {
                    reservationGrid.AdvanceRow();
                    num10++;
                }
                this.AdvanceUntilFits(maxColumns, reservationGrid, fixedLayoutInfo, out colStop);
                if (fixedLayoutInfo.RowStart < maxRows)
                {
                    goto Label_01B0;
                }
                return false;
            Label_01A4:
                reservationGrid.AdvanceRow();
                num10++;
            Label_01B0:
                if (num10 < (fixedLayoutInfo.RowStart - currentRow))
                {
                    goto Label_01A4;
                }
                currentRow = fixedLayoutInfo.RowStart;
                colStop = Math.Min(fixedLayoutInfo.ColumnStart + fixedLayoutInfo.ColumnSpan, maxColumns);
                num8 = Math.Min(fixedLayoutInfo.RowStart + fixedLayoutInfo.RowSpan, maxRows);
                reservationGrid.ReserveAll(fixedLayoutInfo, num8, colStop);
                fixedLayoutInfo = GetNextLayoutInfo(fixedChildrenInfo, ref index, true);
            Label_020C:
                num4 = colStop;
                num2 = (num2 == 0x7fffffff) ? num8 : Math.Max(num2, num8);
                num = (num == 0x7fffffff) ? colStop : Math.Max(num, colStop);
            }
            if (growStyle == TableLayoutPanelGrowStyle.FixedSize)
            {
                num = maxColumns;
                num2 = maxRows;
            }
            else if (growStyle == TableLayoutPanelGrowStyle.AddRows)
            {
                num = maxColumns;
                num2 = Math.Max(containerInfo.MaxRows, num2);
            }
            else
            {
                num2 = (maxRows == 0x7fffffff) ? num2 : maxRows;
                num = Math.Max(containerInfo.MaxColumns, num);
            }
            if ((containerInfo.Rows == null) || (containerInfo.Rows.Length != num2))
            {
                containerInfo.Rows = new Strip[num2];
            }
            if ((containerInfo.Columns == null) || (containerInfo.Columns.Length != num))
            {
                containerInfo.Columns = new Strip[num];
            }
            containerInfo.Valid = true;
            return true;
        }

        private void xDistributeSize(IList styles, Strip[] strips, int start, int stop, int desiredLength, SizeProxy sizeProxy, int cellBorderWidth)
        {
            int num = 0;
            int num2 = 0;
            desiredLength -= cellBorderWidth * ((stop - start) - 1);
            desiredLength = Math.Max(0, desiredLength);
            for (int i = start; i < stop; i++)
            {
                sizeProxy.Strip = strips[i];
                if (!this.IsAbsolutelySized(i, styles) && (sizeProxy.Size == 0))
                {
                    num2++;
                }
                num += sizeProxy.Size;
            }
            int num4 = desiredLength - num;
            if (num4 > 0)
            {
                if (num2 != 0)
                {
                    int num8 = num4 / num2;
                    int num9 = 0;
                    for (int j = start; j < stop; j++)
                    {
                        sizeProxy.Strip = strips[j];
                        if (!this.IsAbsolutelySized(j, styles) && (sizeProxy.Size == 0))
                        {
                            num9++;
                            if (num9 == num2)
                            {
                                num8 = num4 - (num8 * (num2 - 1));
                            }
                            sizeProxy.Size += num8;
                            strips[j] = sizeProxy.Strip;
                        }
                    }
                }
                else
                {
                    int num5 = stop - 1;
                    while (num5 >= start)
                    {
                        if ((num5 < styles.Count) && (((TableLayoutStyle) styles[num5]).SizeType == SizeType.Percent))
                        {
                            break;
                        }
                        num5--;
                    }
                    if (num5 != (start - 1))
                    {
                        stop = num5 + 1;
                    }
                    for (int k = stop - 1; k >= start; k--)
                    {
                        if (!this.IsAbsolutelySized(k, styles))
                        {
                            sizeProxy.Strip = strips[k];
                            if (((k != (strips.Length - 1)) && !strips[k + 1].IsStart) && !this.IsAbsolutelySized(k + 1, styles))
                            {
                                sizeProxy.Strip = strips[k + 1];
                                int num7 = Math.Min(sizeProxy.Size, num4);
                                sizeProxy.Size -= num7;
                                strips[k + 1] = sizeProxy.Strip;
                                sizeProxy.Strip = strips[k];
                            }
                            sizeProxy.Size += num4;
                            strips[k] = sizeProxy.Strip;
                            return;
                        }
                    }
                }
            }
        }

        private class ColumnSpanComparer : TableLayout.SpanComparer
        {
            private static readonly TableLayout.ColumnSpanComparer instance = new TableLayout.ColumnSpanComparer();

            public override int GetSpan(TableLayout.LayoutInfo layoutInfo)
            {
                return layoutInfo.ColumnSpan;
            }

            public static TableLayout.ColumnSpanComparer GetInstance
            {
                get
                {
                    return instance;
                }
            }
        }

        internal sealed class ContainerInfo
        {
            private int _cellBorderWidth;
            private TableLayout.LayoutInfo[] _childInfo;
            private TableLayout.Strip[] _cols;
            private TableLayoutColumnStyleCollection _colStyles;
            private IArrangedElement _container;
            private int _countFixedChildren;
            private TableLayoutPanelGrowStyle _growStyle;
            private int _maxColumns;
            private int _maxRows;
            private int _minColumns;
            private int _minRows;
            private int _minRowsAndColumns;
            private TableLayout.Strip[] _rows;
            private TableLayoutRowStyleCollection _rowStyles;
            private BitVector32 _state;
            private static TableLayout.Strip[] emptyStrip = new TableLayout.Strip[0];
            private static readonly int stateChildHasColumnSpan = BitVector32.CreateMask(stateChildInfoValid);
            private static readonly int stateChildHasRowSpan = BitVector32.CreateMask(stateChildHasColumnSpan);
            private static readonly int stateChildInfoValid = BitVector32.CreateMask(stateValid);
            private static readonly int stateValid = BitVector32.CreateMask();

            public ContainerInfo(IArrangedElement container)
            {
                this._cols = emptyStrip;
                this._rows = emptyStrip;
                this._state = new BitVector32();
                this._container = container;
                this._growStyle = TableLayoutPanelGrowStyle.AddRows;
            }

            public ContainerInfo(TableLayout.ContainerInfo containerInfo)
            {
                this._cols = emptyStrip;
                this._rows = emptyStrip;
                this._state = new BitVector32();
                this._cellBorderWidth = containerInfo.CellBorderWidth;
                this._maxRows = containerInfo.MaxRows;
                this._maxColumns = containerInfo.MaxColumns;
                this._growStyle = containerInfo.GrowStyle;
                this._container = containerInfo.Container;
                this._rowStyles = containerInfo.RowStyles;
                this._colStyles = containerInfo.ColumnStyles;
            }

            public Size GetCachedPreferredSize(Size proposedContstraints, out bool isValid)
            {
                isValid = false;
                if ((proposedContstraints.Height == 0) || (proposedContstraints.Width == 0))
                {
                    Size size = CommonProperties.xGetPreferredSizeCache(this.Container);
                    if (!size.IsEmpty)
                    {
                        isValid = true;
                        return size;
                    }
                }
                return Size.Empty;
            }

            public int CellBorderWidth
            {
                get
                {
                    return this._cellBorderWidth;
                }
                set
                {
                    this._cellBorderWidth = value;
                }
            }

            public bool ChildHasColumnSpan
            {
                get
                {
                    return this._state[stateChildHasColumnSpan];
                }
                set
                {
                    this._state[stateChildHasColumnSpan] = value;
                }
            }

            public bool ChildHasRowSpan
            {
                get
                {
                    return this._state[stateChildHasRowSpan];
                }
                set
                {
                    this._state[stateChildHasRowSpan] = value;
                }
            }

            public bool ChildInfoValid
            {
                get
                {
                    return this._state[stateChildInfoValid];
                }
            }

            public TableLayout.LayoutInfo[] ChildrenInfo
            {
                get
                {
                    if (!this._state[stateChildInfoValid])
                    {
                        this._countFixedChildren = 0;
                        this._minRowsAndColumns = 0;
                        this._minColumns = 0;
                        this._minRows = 0;
                        ArrangedElementCollection children = this.Container.Children;
                        TableLayout.LayoutInfo[] sourceArray = new TableLayout.LayoutInfo[children.Count];
                        int num = 0;
                        int num2 = 0;
                        for (int i = 0; i < children.Count; i++)
                        {
                            IArrangedElement element = children[i];
                            if (!element.ParticipatesInLayout)
                            {
                                num++;
                            }
                            else
                            {
                                TableLayout.LayoutInfo layoutInfo = TableLayout.GetLayoutInfo(element);
                                if (layoutInfo.IsAbsolutelyPositioned)
                                {
                                    this._countFixedChildren++;
                                }
                                sourceArray[num2++] = layoutInfo;
                                this._minRowsAndColumns += layoutInfo.RowSpan * layoutInfo.ColumnSpan;
                                if (layoutInfo.IsAbsolutelyPositioned)
                                {
                                    this._minColumns = Math.Max(this._minColumns, layoutInfo.ColumnPosition + layoutInfo.ColumnSpan);
                                    this._minRows = Math.Max(this._minRows, layoutInfo.RowPosition + layoutInfo.RowSpan);
                                }
                            }
                        }
                        if (num > 0)
                        {
                            TableLayout.LayoutInfo[] destinationArray = new TableLayout.LayoutInfo[sourceArray.Length - num];
                            Array.Copy(sourceArray, destinationArray, destinationArray.Length);
                            this._childInfo = destinationArray;
                        }
                        else
                        {
                            this._childInfo = sourceArray;
                        }
                        this._state[stateChildInfoValid] = true;
                    }
                    if (this._childInfo != null)
                    {
                        return this._childInfo;
                    }
                    return new TableLayout.LayoutInfo[0];
                }
            }

            public TableLayout.Strip[] Columns
            {
                get
                {
                    return this._cols;
                }
                set
                {
                    this._cols = value;
                }
            }

            public TableLayoutColumnStyleCollection ColumnStyles
            {
                get
                {
                    if (this._colStyles == null)
                    {
                        this._colStyles = new TableLayoutColumnStyleCollection(this._container);
                    }
                    return this._colStyles;
                }
                set
                {
                    this._colStyles = value;
                    if (this._colStyles != null)
                    {
                        this._colStyles.EnsureOwnership(this._container);
                    }
                }
            }

            public IArrangedElement Container
            {
                get
                {
                    return this._container;
                }
            }

            public TableLayout.LayoutInfo[] FixedChildrenInfo
            {
                get
                {
                    TableLayout.LayoutInfo[] array = new TableLayout.LayoutInfo[this._countFixedChildren];
                    if (this.HasChildWithAbsolutePositioning)
                    {
                        int num = 0;
                        for (int i = 0; i < this._childInfo.Length; i++)
                        {
                            if (this._childInfo[i].IsAbsolutelyPositioned)
                            {
                                array[num++] = this._childInfo[i];
                            }
                        }
                        Array.Sort(array, TableLayout.PreAssignedPositionComparer.GetInstance);
                    }
                    return array;
                }
            }

            public TableLayoutPanelGrowStyle GrowStyle
            {
                get
                {
                    return this._growStyle;
                }
                set
                {
                    if (this._growStyle != value)
                    {
                        this._growStyle = value;
                        this.Valid = false;
                    }
                }
            }

            public bool HasChildWithAbsolutePositioning
            {
                get
                {
                    return (this._countFixedChildren > 0);
                }
            }

            public bool HasMultiplePercentColumns
            {
                get
                {
                    if (this._colStyles != null)
                    {
                        bool flag = false;
                        foreach (ColumnStyle style in (IEnumerable) this._colStyles)
                        {
                            if (style.SizeType == SizeType.Percent)
                            {
                                if (flag)
                                {
                                    return true;
                                }
                                flag = true;
                            }
                        }
                    }
                    return false;
                }
            }

            public int MaxColumns
            {
                get
                {
                    return this._maxColumns;
                }
                set
                {
                    if (this._maxColumns != value)
                    {
                        this._maxColumns = value;
                        this.Valid = false;
                    }
                }
            }

            public int MaxRows
            {
                get
                {
                    return this._maxRows;
                }
                set
                {
                    if (this._maxRows != value)
                    {
                        this._maxRows = value;
                        this.Valid = false;
                    }
                }
            }

            public int MinColumns
            {
                get
                {
                    return this._minColumns;
                }
            }

            public int MinRows
            {
                get
                {
                    return this._minRows;
                }
            }

            public int MinRowsAndColumns
            {
                get
                {
                    return this._minRowsAndColumns;
                }
            }

            public TableLayout.Strip[] Rows
            {
                get
                {
                    return this._rows;
                }
                set
                {
                    this._rows = value;
                }
            }

            public TableLayoutRowStyleCollection RowStyles
            {
                get
                {
                    if (this._rowStyles == null)
                    {
                        this._rowStyles = new TableLayoutRowStyleCollection(this._container);
                    }
                    return this._rowStyles;
                }
                set
                {
                    this._rowStyles = value;
                    if (this._rowStyles != null)
                    {
                        this._rowStyles.EnsureOwnership(this._container);
                    }
                }
            }

            public bool Valid
            {
                get
                {
                    return this._state[stateValid];
                }
                set
                {
                    this._state[stateValid] = value;
                    if (!this._state[stateValid])
                    {
                        this._state[stateChildInfoValid] = false;
                    }
                }
            }
        }

        internal sealed class LayoutInfo
        {
            private int _colPos = -1;
            private int _columnSpan = 1;
            private int _columnStart = -1;
            private IArrangedElement _element;
            private int _rowPos = -1;
            private int _rowSpan = 1;
            private int _rowStart = -1;

            public LayoutInfo(IArrangedElement element)
            {
                this._element = element;
            }

            internal int ColumnPosition
            {
                get
                {
                    return this._colPos;
                }
                set
                {
                    this._colPos = value;
                }
            }

            internal int ColumnSpan
            {
                get
                {
                    return this._columnSpan;
                }
                set
                {
                    this._columnSpan = value;
                }
            }

            internal int ColumnStart
            {
                get
                {
                    return this._columnStart;
                }
                set
                {
                    this._columnStart = value;
                }
            }

            internal IArrangedElement Element
            {
                get
                {
                    return this._element;
                }
            }

            internal bool IsAbsolutelyPositioned
            {
                get
                {
                    return ((this._rowPos >= 0) && (this._colPos >= 0));
                }
            }

            internal int RowPosition
            {
                get
                {
                    return this._rowPos;
                }
                set
                {
                    this._rowPos = value;
                }
            }

            internal int RowSpan
            {
                get
                {
                    return this._rowSpan;
                }
                set
                {
                    this._rowSpan = value;
                }
            }

            internal int RowStart
            {
                get
                {
                    return this._rowStart;
                }
                set
                {
                    this._rowStart = value;
                }
            }
        }

        private class MaxSizeProxy : TableLayout.SizeProxy
        {
            private static readonly TableLayout.MaxSizeProxy instance = new TableLayout.MaxSizeProxy();

            public static TableLayout.MaxSizeProxy GetInstance
            {
                get
                {
                    return instance;
                }
            }

            public override int Size
            {
                get
                {
                    return this.strip.MaxSize;
                }
                set
                {
                    this.strip.MaxSize = value;
                }
            }
        }

        private class MinSizeProxy : TableLayout.SizeProxy
        {
            private static readonly TableLayout.MinSizeProxy instance = new TableLayout.MinSizeProxy();

            public static TableLayout.MinSizeProxy GetInstance
            {
                get
                {
                    return instance;
                }
            }

            public override int Size
            {
                get
                {
                    return this.strip.MinSize;
                }
                set
                {
                    this.strip.MinSize = value;
                }
            }
        }

        private class PostAssignedPositionComparer : IComparer
        {
            private static readonly TableLayout.PostAssignedPositionComparer instance = new TableLayout.PostAssignedPositionComparer();

            public int Compare(object x, object y)
            {
                TableLayout.LayoutInfo info = (TableLayout.LayoutInfo) x;
                TableLayout.LayoutInfo info2 = (TableLayout.LayoutInfo) y;
                if (info.RowStart < info2.RowStart)
                {
                    return -1;
                }
                if (info.RowStart > info2.RowStart)
                {
                    return 1;
                }
                if (info.ColumnStart < info2.ColumnStart)
                {
                    return -1;
                }
                if (info.ColumnStart > info2.ColumnStart)
                {
                    return 1;
                }
                return 0;
            }

            public static TableLayout.PostAssignedPositionComparer GetInstance
            {
                get
                {
                    return instance;
                }
            }
        }

        private class PreAssignedPositionComparer : IComparer
        {
            private static readonly TableLayout.PreAssignedPositionComparer instance = new TableLayout.PreAssignedPositionComparer();

            public int Compare(object x, object y)
            {
                TableLayout.LayoutInfo info = (TableLayout.LayoutInfo) x;
                TableLayout.LayoutInfo info2 = (TableLayout.LayoutInfo) y;
                if (info.RowPosition < info2.RowPosition)
                {
                    return -1;
                }
                if (info.RowPosition > info2.RowPosition)
                {
                    return 1;
                }
                if (info.ColumnPosition < info2.ColumnPosition)
                {
                    return -1;
                }
                if (info.ColumnPosition > info2.ColumnPosition)
                {
                    return 1;
                }
                return 0;
            }

            public static TableLayout.PreAssignedPositionComparer GetInstance
            {
                get
                {
                    return instance;
                }
            }
        }

        private sealed class ReservationGrid
        {
            private int _numColumns = 1;
            private ArrayList _rows = new ArrayList();

            public void AdvanceRow()
            {
                if (this._rows.Count > 0)
                {
                    this._rows.RemoveAt(0);
                }
            }

            public bool IsReserved(int column, int rowOffset)
            {
                if (rowOffset >= this._rows.Count)
                {
                    return false;
                }
                if (column >= ((BitArray) this._rows[rowOffset]).Length)
                {
                    return false;
                }
                return ((BitArray) this._rows[rowOffset])[column];
            }

            public void Reserve(int column, int rowOffset)
            {
                while (rowOffset >= this._rows.Count)
                {
                    this._rows.Add(new BitArray(this._numColumns));
                }
                if (column >= ((BitArray) this._rows[rowOffset]).Length)
                {
                    ((BitArray) this._rows[rowOffset]).Length = column + 1;
                    if (column >= this._numColumns)
                    {
                        this._numColumns = column + 1;
                    }
                }
                ((BitArray) this._rows[rowOffset])[column] = true;
            }

            public void ReserveAll(TableLayout.LayoutInfo layoutInfo, int rowStop, int colStop)
            {
                for (int i = 1; i < (rowStop - layoutInfo.RowStart); i++)
                {
                    for (int j = layoutInfo.ColumnStart; j < colStop; j++)
                    {
                        this.Reserve(j, i);
                    }
                }
            }
        }

        private class RowSpanComparer : TableLayout.SpanComparer
        {
            private static readonly TableLayout.RowSpanComparer instance = new TableLayout.RowSpanComparer();

            public override int GetSpan(TableLayout.LayoutInfo layoutInfo)
            {
                return layoutInfo.RowSpan;
            }

            public static TableLayout.RowSpanComparer GetInstance
            {
                get
                {
                    return instance;
                }
            }
        }

        private abstract class SizeProxy
        {
            protected System.Windows.Forms.Layout.TableLayout.Strip strip;

            protected SizeProxy()
            {
            }

            public abstract int Size { get; set; }

            public System.Windows.Forms.Layout.TableLayout.Strip Strip
            {
                get
                {
                    return this.strip;
                }
                set
                {
                    this.strip = value;
                }
            }
        }

        private abstract class SpanComparer : IComparer
        {
            protected SpanComparer()
            {
            }

            public int Compare(object x, object y)
            {
                TableLayout.LayoutInfo layoutInfo = (TableLayout.LayoutInfo) x;
                TableLayout.LayoutInfo info2 = (TableLayout.LayoutInfo) y;
                return (this.GetSpan(layoutInfo) - this.GetSpan(info2));
            }

            public abstract int GetSpan(TableLayout.LayoutInfo layoutInfo);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Strip
        {
            private int _maxSize;
            private int _minSize;
            private bool _isStart;
            public int MinSize
            {
                get
                {
                    return this._minSize;
                }
                set
                {
                    this._minSize = value;
                }
            }
            public int MaxSize
            {
                get
                {
                    return this._maxSize;
                }
                set
                {
                    this._maxSize = value;
                }
            }
            public bool IsStart
            {
                get
                {
                    return this._isStart;
                }
                set
                {
                    this._isStart = value;
                }
            }
        }
    }
}

