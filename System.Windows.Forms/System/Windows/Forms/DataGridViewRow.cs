namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;

    [TypeConverter(typeof(DataGridViewRowConverter))]
    public class DataGridViewRow : DataGridViewBand
    {
        internal const int defaultMinRowThickness = 3;
        private const DataGridViewAutoSizeRowCriteriaInternal invalidDataGridViewAutoSizeRowCriteriaInternalMask = ~(DataGridViewAutoSizeRowCriteriaInternal.AllColumns | DataGridViewAutoSizeRowCriteriaInternal.Header);
        private static readonly int PropRowAccessibilityObject = PropertyStore.CreateKey();
        private static readonly int PropRowErrorText = PropertyStore.CreateKey();
        private DataGridViewCellCollection rowCells;
        private static System.Type rowType = typeof(DataGridViewRow);

        public DataGridViewRow()
        {
            base.bandIsRow = true;
            base.MinimumThickness = 3;
            base.Thickness = Control.DefaultFont.Height + 9;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public virtual DataGridViewAdvancedBorderStyle AdjustRowHeaderBorderStyle(DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStyleInput, DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStylePlaceholder, bool singleVerticalBorderAdded, bool singleHorizontalBorderAdded, bool isFirstDisplayedRow, bool isLastVisibleRow)
        {
            if ((base.DataGridView == null) || !base.DataGridView.ApplyVisualStylesToHeaderCells)
            {
                switch (dataGridViewAdvancedBorderStyleInput.All)
                {
                    case DataGridViewAdvancedCellBorderStyle.Single:
                        if (isFirstDisplayedRow && !base.DataGridView.ColumnHeadersVisible)
                        {
                            return dataGridViewAdvancedBorderStyleInput;
                        }
                        dataGridViewAdvancedBorderStylePlaceholder.LeftInternal = DataGridViewAdvancedCellBorderStyle.Single;
                        dataGridViewAdvancedBorderStylePlaceholder.TopInternal = DataGridViewAdvancedCellBorderStyle.None;
                        dataGridViewAdvancedBorderStylePlaceholder.BottomInternal = DataGridViewAdvancedCellBorderStyle.Single;
                        dataGridViewAdvancedBorderStylePlaceholder.RightInternal = DataGridViewAdvancedCellBorderStyle.Single;
                        return dataGridViewAdvancedBorderStylePlaceholder;

                    case DataGridViewAdvancedCellBorderStyle.Inset:
                        if (!isFirstDisplayedRow || !singleHorizontalBorderAdded)
                        {
                            return dataGridViewAdvancedBorderStyleInput;
                        }
                        dataGridViewAdvancedBorderStylePlaceholder.LeftInternal = DataGridViewAdvancedCellBorderStyle.Inset;
                        dataGridViewAdvancedBorderStylePlaceholder.TopInternal = DataGridViewAdvancedCellBorderStyle.InsetDouble;
                        dataGridViewAdvancedBorderStylePlaceholder.BottomInternal = DataGridViewAdvancedCellBorderStyle.Inset;
                        dataGridViewAdvancedBorderStylePlaceholder.RightInternal = DataGridViewAdvancedCellBorderStyle.Inset;
                        return dataGridViewAdvancedBorderStylePlaceholder;

                    case DataGridViewAdvancedCellBorderStyle.InsetDouble:
                        if ((base.DataGridView == null) || !base.DataGridView.RightToLeftInternal)
                        {
                            dataGridViewAdvancedBorderStylePlaceholder.LeftInternal = DataGridViewAdvancedCellBorderStyle.InsetDouble;
                            dataGridViewAdvancedBorderStylePlaceholder.RightInternal = DataGridViewAdvancedCellBorderStyle.Inset;
                        }
                        else
                        {
                            dataGridViewAdvancedBorderStylePlaceholder.LeftInternal = DataGridViewAdvancedCellBorderStyle.Inset;
                            dataGridViewAdvancedBorderStylePlaceholder.RightInternal = DataGridViewAdvancedCellBorderStyle.InsetDouble;
                        }
                        if (isFirstDisplayedRow)
                        {
                            dataGridViewAdvancedBorderStylePlaceholder.TopInternal = base.DataGridView.ColumnHeadersVisible ? DataGridViewAdvancedCellBorderStyle.Inset : DataGridViewAdvancedCellBorderStyle.InsetDouble;
                        }
                        else
                        {
                            dataGridViewAdvancedBorderStylePlaceholder.TopInternal = DataGridViewAdvancedCellBorderStyle.Inset;
                        }
                        dataGridViewAdvancedBorderStylePlaceholder.BottomInternal = DataGridViewAdvancedCellBorderStyle.Inset;
                        return dataGridViewAdvancedBorderStylePlaceholder;

                    case DataGridViewAdvancedCellBorderStyle.Outset:
                        if (!isFirstDisplayedRow || !singleHorizontalBorderAdded)
                        {
                            return dataGridViewAdvancedBorderStyleInput;
                        }
                        dataGridViewAdvancedBorderStylePlaceholder.LeftInternal = DataGridViewAdvancedCellBorderStyle.Outset;
                        dataGridViewAdvancedBorderStylePlaceholder.TopInternal = DataGridViewAdvancedCellBorderStyle.OutsetDouble;
                        dataGridViewAdvancedBorderStylePlaceholder.BottomInternal = DataGridViewAdvancedCellBorderStyle.Outset;
                        dataGridViewAdvancedBorderStylePlaceholder.RightInternal = DataGridViewAdvancedCellBorderStyle.Outset;
                        return dataGridViewAdvancedBorderStylePlaceholder;

                    case DataGridViewAdvancedCellBorderStyle.OutsetDouble:
                        if ((base.DataGridView == null) || !base.DataGridView.RightToLeftInternal)
                        {
                            dataGridViewAdvancedBorderStylePlaceholder.LeftInternal = DataGridViewAdvancedCellBorderStyle.OutsetDouble;
                            dataGridViewAdvancedBorderStylePlaceholder.RightInternal = DataGridViewAdvancedCellBorderStyle.Outset;
                        }
                        else
                        {
                            dataGridViewAdvancedBorderStylePlaceholder.LeftInternal = DataGridViewAdvancedCellBorderStyle.Outset;
                            dataGridViewAdvancedBorderStylePlaceholder.RightInternal = DataGridViewAdvancedCellBorderStyle.OutsetDouble;
                        }
                        if (isFirstDisplayedRow)
                        {
                            dataGridViewAdvancedBorderStylePlaceholder.TopInternal = base.DataGridView.ColumnHeadersVisible ? DataGridViewAdvancedCellBorderStyle.Outset : DataGridViewAdvancedCellBorderStyle.OutsetDouble;
                        }
                        else
                        {
                            dataGridViewAdvancedBorderStylePlaceholder.TopInternal = DataGridViewAdvancedCellBorderStyle.Outset;
                        }
                        dataGridViewAdvancedBorderStylePlaceholder.BottomInternal = DataGridViewAdvancedCellBorderStyle.Outset;
                        return dataGridViewAdvancedBorderStylePlaceholder;

                    case DataGridViewAdvancedCellBorderStyle.OutsetPartial:
                        if ((base.DataGridView == null) || !base.DataGridView.RightToLeftInternal)
                        {
                            dataGridViewAdvancedBorderStylePlaceholder.LeftInternal = DataGridViewAdvancedCellBorderStyle.OutsetDouble;
                            dataGridViewAdvancedBorderStylePlaceholder.RightInternal = DataGridViewAdvancedCellBorderStyle.Outset;
                        }
                        else
                        {
                            dataGridViewAdvancedBorderStylePlaceholder.LeftInternal = DataGridViewAdvancedCellBorderStyle.Outset;
                            dataGridViewAdvancedBorderStylePlaceholder.RightInternal = DataGridViewAdvancedCellBorderStyle.OutsetDouble;
                        }
                        if (isFirstDisplayedRow)
                        {
                            dataGridViewAdvancedBorderStylePlaceholder.TopInternal = base.DataGridView.ColumnHeadersVisible ? DataGridViewAdvancedCellBorderStyle.Outset : DataGridViewAdvancedCellBorderStyle.OutsetDouble;
                        }
                        else
                        {
                            dataGridViewAdvancedBorderStylePlaceholder.TopInternal = DataGridViewAdvancedCellBorderStyle.OutsetPartial;
                        }
                        dataGridViewAdvancedBorderStylePlaceholder.BottomInternal = isLastVisibleRow ? DataGridViewAdvancedCellBorderStyle.Outset : DataGridViewAdvancedCellBorderStyle.OutsetPartial;
                        return dataGridViewAdvancedBorderStylePlaceholder;
                }
                return dataGridViewAdvancedBorderStyleInput;
            }
            switch (dataGridViewAdvancedBorderStyleInput.All)
            {
                case DataGridViewAdvancedCellBorderStyle.Single:
                    if (!isFirstDisplayedRow || base.DataGridView.ColumnHeadersVisible)
                    {
                        dataGridViewAdvancedBorderStylePlaceholder.TopInternal = DataGridViewAdvancedCellBorderStyle.None;
                    }
                    else
                    {
                        dataGridViewAdvancedBorderStylePlaceholder.TopInternal = DataGridViewAdvancedCellBorderStyle.Single;
                    }
                    dataGridViewAdvancedBorderStylePlaceholder.LeftInternal = DataGridViewAdvancedCellBorderStyle.Single;
                    dataGridViewAdvancedBorderStylePlaceholder.RightInternal = DataGridViewAdvancedCellBorderStyle.Single;
                    dataGridViewAdvancedBorderStylePlaceholder.BottomInternal = DataGridViewAdvancedCellBorderStyle.None;
                    return dataGridViewAdvancedBorderStylePlaceholder;

                case DataGridViewAdvancedCellBorderStyle.Inset:
                    if (!isFirstDisplayedRow || base.DataGridView.ColumnHeadersVisible)
                    {
                        dataGridViewAdvancedBorderStylePlaceholder.TopInternal = DataGridViewAdvancedCellBorderStyle.None;
                        break;
                    }
                    dataGridViewAdvancedBorderStylePlaceholder.TopInternal = DataGridViewAdvancedCellBorderStyle.Inset;
                    break;

                case DataGridViewAdvancedCellBorderStyle.InsetDouble:
                    if (!isFirstDisplayedRow || base.DataGridView.ColumnHeadersVisible)
                    {
                        dataGridViewAdvancedBorderStylePlaceholder.TopInternal = DataGridViewAdvancedCellBorderStyle.None;
                    }
                    else
                    {
                        dataGridViewAdvancedBorderStylePlaceholder.TopInternal = DataGridViewAdvancedCellBorderStyle.InsetDouble;
                    }
                    if ((base.DataGridView != null) && base.DataGridView.RightToLeftInternal)
                    {
                        dataGridViewAdvancedBorderStylePlaceholder.LeftInternal = DataGridViewAdvancedCellBorderStyle.Inset;
                    }
                    else
                    {
                        dataGridViewAdvancedBorderStylePlaceholder.LeftInternal = DataGridViewAdvancedCellBorderStyle.InsetDouble;
                    }
                    dataGridViewAdvancedBorderStylePlaceholder.RightInternal = DataGridViewAdvancedCellBorderStyle.Inset;
                    dataGridViewAdvancedBorderStylePlaceholder.BottomInternal = DataGridViewAdvancedCellBorderStyle.None;
                    return dataGridViewAdvancedBorderStylePlaceholder;

                case DataGridViewAdvancedCellBorderStyle.Outset:
                    if (!isFirstDisplayedRow || base.DataGridView.ColumnHeadersVisible)
                    {
                        dataGridViewAdvancedBorderStylePlaceholder.TopInternal = DataGridViewAdvancedCellBorderStyle.None;
                    }
                    else
                    {
                        dataGridViewAdvancedBorderStylePlaceholder.TopInternal = DataGridViewAdvancedCellBorderStyle.Outset;
                    }
                    dataGridViewAdvancedBorderStylePlaceholder.LeftInternal = DataGridViewAdvancedCellBorderStyle.Outset;
                    dataGridViewAdvancedBorderStylePlaceholder.RightInternal = DataGridViewAdvancedCellBorderStyle.Outset;
                    dataGridViewAdvancedBorderStylePlaceholder.BottomInternal = DataGridViewAdvancedCellBorderStyle.None;
                    return dataGridViewAdvancedBorderStylePlaceholder;

                case DataGridViewAdvancedCellBorderStyle.OutsetDouble:
                    if (!isFirstDisplayedRow || base.DataGridView.ColumnHeadersVisible)
                    {
                        dataGridViewAdvancedBorderStylePlaceholder.TopInternal = DataGridViewAdvancedCellBorderStyle.None;
                    }
                    else
                    {
                        dataGridViewAdvancedBorderStylePlaceholder.TopInternal = DataGridViewAdvancedCellBorderStyle.OutsetDouble;
                    }
                    if ((base.DataGridView != null) && base.DataGridView.RightToLeftInternal)
                    {
                        dataGridViewAdvancedBorderStylePlaceholder.LeftInternal = DataGridViewAdvancedCellBorderStyle.Outset;
                    }
                    else
                    {
                        dataGridViewAdvancedBorderStylePlaceholder.LeftInternal = DataGridViewAdvancedCellBorderStyle.OutsetDouble;
                    }
                    dataGridViewAdvancedBorderStylePlaceholder.RightInternal = DataGridViewAdvancedCellBorderStyle.Outset;
                    dataGridViewAdvancedBorderStylePlaceholder.BottomInternal = DataGridViewAdvancedCellBorderStyle.None;
                    return dataGridViewAdvancedBorderStylePlaceholder;

                case DataGridViewAdvancedCellBorderStyle.OutsetPartial:
                    if (!isFirstDisplayedRow || base.DataGridView.ColumnHeadersVisible)
                    {
                        dataGridViewAdvancedBorderStylePlaceholder.TopInternal = DataGridViewAdvancedCellBorderStyle.None;
                    }
                    else
                    {
                        dataGridViewAdvancedBorderStylePlaceholder.TopInternal = DataGridViewAdvancedCellBorderStyle.OutsetDouble;
                    }
                    if ((base.DataGridView != null) && base.DataGridView.RightToLeftInternal)
                    {
                        dataGridViewAdvancedBorderStylePlaceholder.LeftInternal = DataGridViewAdvancedCellBorderStyle.Outset;
                    }
                    else
                    {
                        dataGridViewAdvancedBorderStylePlaceholder.LeftInternal = DataGridViewAdvancedCellBorderStyle.OutsetDouble;
                    }
                    dataGridViewAdvancedBorderStylePlaceholder.RightInternal = DataGridViewAdvancedCellBorderStyle.Outset;
                    dataGridViewAdvancedBorderStylePlaceholder.BottomInternal = DataGridViewAdvancedCellBorderStyle.None;
                    return dataGridViewAdvancedBorderStylePlaceholder;

                default:
                    return dataGridViewAdvancedBorderStyleInput;
            }
            dataGridViewAdvancedBorderStylePlaceholder.LeftInternal = DataGridViewAdvancedCellBorderStyle.Inset;
            dataGridViewAdvancedBorderStylePlaceholder.RightInternal = DataGridViewAdvancedCellBorderStyle.Inset;
            dataGridViewAdvancedBorderStylePlaceholder.BottomInternal = DataGridViewAdvancedCellBorderStyle.None;
            return dataGridViewAdvancedBorderStylePlaceholder;
        }

        private void BuildInheritedRowHeaderCellStyle(DataGridViewCellStyle inheritedCellStyle)
        {
            DataGridViewCellStyle style = null;
            if (this.HeaderCell.HasStyle)
            {
                style = this.HeaderCell.Style;
            }
            DataGridViewCellStyle rowHeadersDefaultCellStyle = base.DataGridView.RowHeadersDefaultCellStyle;
            DataGridViewCellStyle defaultCellStyle = base.DataGridView.DefaultCellStyle;
            if ((style != null) && !style.BackColor.IsEmpty)
            {
                inheritedCellStyle.BackColor = style.BackColor;
            }
            else if (!rowHeadersDefaultCellStyle.BackColor.IsEmpty)
            {
                inheritedCellStyle.BackColor = rowHeadersDefaultCellStyle.BackColor;
            }
            else
            {
                inheritedCellStyle.BackColor = defaultCellStyle.BackColor;
            }
            if ((style != null) && !style.ForeColor.IsEmpty)
            {
                inheritedCellStyle.ForeColor = style.ForeColor;
            }
            else if (!rowHeadersDefaultCellStyle.ForeColor.IsEmpty)
            {
                inheritedCellStyle.ForeColor = rowHeadersDefaultCellStyle.ForeColor;
            }
            else
            {
                inheritedCellStyle.ForeColor = defaultCellStyle.ForeColor;
            }
            if ((style != null) && !style.SelectionBackColor.IsEmpty)
            {
                inheritedCellStyle.SelectionBackColor = style.SelectionBackColor;
            }
            else if (!rowHeadersDefaultCellStyle.SelectionBackColor.IsEmpty)
            {
                inheritedCellStyle.SelectionBackColor = rowHeadersDefaultCellStyle.SelectionBackColor;
            }
            else
            {
                inheritedCellStyle.SelectionBackColor = defaultCellStyle.SelectionBackColor;
            }
            if ((style != null) && !style.SelectionForeColor.IsEmpty)
            {
                inheritedCellStyle.SelectionForeColor = style.SelectionForeColor;
            }
            else if (!rowHeadersDefaultCellStyle.SelectionForeColor.IsEmpty)
            {
                inheritedCellStyle.SelectionForeColor = rowHeadersDefaultCellStyle.SelectionForeColor;
            }
            else
            {
                inheritedCellStyle.SelectionForeColor = defaultCellStyle.SelectionForeColor;
            }
            if ((style != null) && (style.Font != null))
            {
                inheritedCellStyle.Font = style.Font;
            }
            else if (rowHeadersDefaultCellStyle.Font != null)
            {
                inheritedCellStyle.Font = rowHeadersDefaultCellStyle.Font;
            }
            else
            {
                inheritedCellStyle.Font = defaultCellStyle.Font;
            }
            if ((style != null) && !style.IsNullValueDefault)
            {
                inheritedCellStyle.NullValue = style.NullValue;
            }
            else if (!rowHeadersDefaultCellStyle.IsNullValueDefault)
            {
                inheritedCellStyle.NullValue = rowHeadersDefaultCellStyle.NullValue;
            }
            else
            {
                inheritedCellStyle.NullValue = defaultCellStyle.NullValue;
            }
            if ((style != null) && !style.IsDataSourceNullValueDefault)
            {
                inheritedCellStyle.DataSourceNullValue = style.DataSourceNullValue;
            }
            else if (!rowHeadersDefaultCellStyle.IsDataSourceNullValueDefault)
            {
                inheritedCellStyle.DataSourceNullValue = rowHeadersDefaultCellStyle.DataSourceNullValue;
            }
            else
            {
                inheritedCellStyle.DataSourceNullValue = defaultCellStyle.DataSourceNullValue;
            }
            if ((style != null) && (style.Format.Length != 0))
            {
                inheritedCellStyle.Format = style.Format;
            }
            else if (rowHeadersDefaultCellStyle.Format.Length != 0)
            {
                inheritedCellStyle.Format = rowHeadersDefaultCellStyle.Format;
            }
            else
            {
                inheritedCellStyle.Format = defaultCellStyle.Format;
            }
            if ((style != null) && !style.IsFormatProviderDefault)
            {
                inheritedCellStyle.FormatProvider = style.FormatProvider;
            }
            else if (!rowHeadersDefaultCellStyle.IsFormatProviderDefault)
            {
                inheritedCellStyle.FormatProvider = rowHeadersDefaultCellStyle.FormatProvider;
            }
            else
            {
                inheritedCellStyle.FormatProvider = defaultCellStyle.FormatProvider;
            }
            if ((style != null) && (style.Alignment != DataGridViewContentAlignment.NotSet))
            {
                inheritedCellStyle.AlignmentInternal = style.Alignment;
            }
            else if ((rowHeadersDefaultCellStyle != null) && (rowHeadersDefaultCellStyle.Alignment != DataGridViewContentAlignment.NotSet))
            {
                inheritedCellStyle.AlignmentInternal = rowHeadersDefaultCellStyle.Alignment;
            }
            else
            {
                inheritedCellStyle.AlignmentInternal = defaultCellStyle.Alignment;
            }
            if ((style != null) && (style.WrapMode != DataGridViewTriState.NotSet))
            {
                inheritedCellStyle.WrapModeInternal = style.WrapMode;
            }
            else if ((rowHeadersDefaultCellStyle != null) && (rowHeadersDefaultCellStyle.WrapMode != DataGridViewTriState.NotSet))
            {
                inheritedCellStyle.WrapModeInternal = rowHeadersDefaultCellStyle.WrapMode;
            }
            else
            {
                inheritedCellStyle.WrapModeInternal = defaultCellStyle.WrapMode;
            }
            if ((style != null) && (style.Tag != null))
            {
                inheritedCellStyle.Tag = style.Tag;
            }
            else if (rowHeadersDefaultCellStyle.Tag != null)
            {
                inheritedCellStyle.Tag = rowHeadersDefaultCellStyle.Tag;
            }
            else
            {
                inheritedCellStyle.Tag = defaultCellStyle.Tag;
            }
            if ((style != null) && (style.Padding != Padding.Empty))
            {
                inheritedCellStyle.PaddingInternal = style.Padding;
            }
            else if (rowHeadersDefaultCellStyle.Padding != Padding.Empty)
            {
                inheritedCellStyle.PaddingInternal = rowHeadersDefaultCellStyle.Padding;
            }
            else
            {
                inheritedCellStyle.PaddingInternal = defaultCellStyle.Padding;
            }
        }

        private void BuildInheritedRowStyle(int rowIndex, DataGridViewCellStyle inheritedRowStyle)
        {
            DataGridViewCellStyle style = null;
            if (base.HasDefaultCellStyle)
            {
                style = this.DefaultCellStyle;
            }
            DataGridViewCellStyle defaultCellStyle = base.DataGridView.DefaultCellStyle;
            DataGridViewCellStyle rowsDefaultCellStyle = base.DataGridView.RowsDefaultCellStyle;
            DataGridViewCellStyle alternatingRowsDefaultCellStyle = base.DataGridView.AlternatingRowsDefaultCellStyle;
            if ((style != null) && !style.BackColor.IsEmpty)
            {
                inheritedRowStyle.BackColor = style.BackColor;
            }
            else if (!rowsDefaultCellStyle.BackColor.IsEmpty && (((rowIndex % 2) == 0) || alternatingRowsDefaultCellStyle.BackColor.IsEmpty))
            {
                inheritedRowStyle.BackColor = rowsDefaultCellStyle.BackColor;
            }
            else if (((rowIndex % 2) == 1) && !alternatingRowsDefaultCellStyle.BackColor.IsEmpty)
            {
                inheritedRowStyle.BackColor = alternatingRowsDefaultCellStyle.BackColor;
            }
            else
            {
                inheritedRowStyle.BackColor = defaultCellStyle.BackColor;
            }
            if ((style != null) && !style.ForeColor.IsEmpty)
            {
                inheritedRowStyle.ForeColor = style.ForeColor;
            }
            else if (!rowsDefaultCellStyle.ForeColor.IsEmpty && (((rowIndex % 2) == 0) || alternatingRowsDefaultCellStyle.ForeColor.IsEmpty))
            {
                inheritedRowStyle.ForeColor = rowsDefaultCellStyle.ForeColor;
            }
            else if (((rowIndex % 2) == 1) && !alternatingRowsDefaultCellStyle.ForeColor.IsEmpty)
            {
                inheritedRowStyle.ForeColor = alternatingRowsDefaultCellStyle.ForeColor;
            }
            else
            {
                inheritedRowStyle.ForeColor = defaultCellStyle.ForeColor;
            }
            if ((style != null) && !style.SelectionBackColor.IsEmpty)
            {
                inheritedRowStyle.SelectionBackColor = style.SelectionBackColor;
            }
            else if (!rowsDefaultCellStyle.SelectionBackColor.IsEmpty && (((rowIndex % 2) == 0) || alternatingRowsDefaultCellStyle.SelectionBackColor.IsEmpty))
            {
                inheritedRowStyle.SelectionBackColor = rowsDefaultCellStyle.SelectionBackColor;
            }
            else if (((rowIndex % 2) == 1) && !alternatingRowsDefaultCellStyle.SelectionBackColor.IsEmpty)
            {
                inheritedRowStyle.SelectionBackColor = alternatingRowsDefaultCellStyle.SelectionBackColor;
            }
            else
            {
                inheritedRowStyle.SelectionBackColor = defaultCellStyle.SelectionBackColor;
            }
            if ((style != null) && !style.SelectionForeColor.IsEmpty)
            {
                inheritedRowStyle.SelectionForeColor = style.SelectionForeColor;
            }
            else if (!rowsDefaultCellStyle.SelectionForeColor.IsEmpty && (((rowIndex % 2) == 0) || alternatingRowsDefaultCellStyle.SelectionForeColor.IsEmpty))
            {
                inheritedRowStyle.SelectionForeColor = rowsDefaultCellStyle.SelectionForeColor;
            }
            else if (((rowIndex % 2) == 1) && !alternatingRowsDefaultCellStyle.SelectionForeColor.IsEmpty)
            {
                inheritedRowStyle.SelectionForeColor = alternatingRowsDefaultCellStyle.SelectionForeColor;
            }
            else
            {
                inheritedRowStyle.SelectionForeColor = defaultCellStyle.SelectionForeColor;
            }
            if ((style != null) && (style.Font != null))
            {
                inheritedRowStyle.Font = style.Font;
            }
            else if ((rowsDefaultCellStyle.Font != null) && (((rowIndex % 2) == 0) || (alternatingRowsDefaultCellStyle.Font == null)))
            {
                inheritedRowStyle.Font = rowsDefaultCellStyle.Font;
            }
            else if (((rowIndex % 2) == 1) && (alternatingRowsDefaultCellStyle.Font != null))
            {
                inheritedRowStyle.Font = alternatingRowsDefaultCellStyle.Font;
            }
            else
            {
                inheritedRowStyle.Font = defaultCellStyle.Font;
            }
            if ((style != null) && !style.IsNullValueDefault)
            {
                inheritedRowStyle.NullValue = style.NullValue;
            }
            else if (!rowsDefaultCellStyle.IsNullValueDefault && (((rowIndex % 2) == 0) || alternatingRowsDefaultCellStyle.IsNullValueDefault))
            {
                inheritedRowStyle.NullValue = rowsDefaultCellStyle.NullValue;
            }
            else if (((rowIndex % 2) == 1) && !alternatingRowsDefaultCellStyle.IsNullValueDefault)
            {
                inheritedRowStyle.NullValue = alternatingRowsDefaultCellStyle.NullValue;
            }
            else
            {
                inheritedRowStyle.NullValue = defaultCellStyle.NullValue;
            }
            if ((style != null) && !style.IsDataSourceNullValueDefault)
            {
                inheritedRowStyle.DataSourceNullValue = style.DataSourceNullValue;
            }
            else if (!rowsDefaultCellStyle.IsDataSourceNullValueDefault && (((rowIndex % 2) == 0) || alternatingRowsDefaultCellStyle.IsDataSourceNullValueDefault))
            {
                inheritedRowStyle.DataSourceNullValue = rowsDefaultCellStyle.DataSourceNullValue;
            }
            else if (((rowIndex % 2) == 1) && !alternatingRowsDefaultCellStyle.IsDataSourceNullValueDefault)
            {
                inheritedRowStyle.DataSourceNullValue = alternatingRowsDefaultCellStyle.DataSourceNullValue;
            }
            else
            {
                inheritedRowStyle.DataSourceNullValue = defaultCellStyle.DataSourceNullValue;
            }
            if ((style != null) && (style.Format.Length != 0))
            {
                inheritedRowStyle.Format = style.Format;
            }
            else if ((rowsDefaultCellStyle.Format.Length != 0) && (((rowIndex % 2) == 0) || (alternatingRowsDefaultCellStyle.Format.Length == 0)))
            {
                inheritedRowStyle.Format = rowsDefaultCellStyle.Format;
            }
            else if (((rowIndex % 2) == 1) && (alternatingRowsDefaultCellStyle.Format.Length != 0))
            {
                inheritedRowStyle.Format = alternatingRowsDefaultCellStyle.Format;
            }
            else
            {
                inheritedRowStyle.Format = defaultCellStyle.Format;
            }
            if ((style != null) && !style.IsFormatProviderDefault)
            {
                inheritedRowStyle.FormatProvider = style.FormatProvider;
            }
            else if (!rowsDefaultCellStyle.IsFormatProviderDefault && (((rowIndex % 2) == 0) || alternatingRowsDefaultCellStyle.IsFormatProviderDefault))
            {
                inheritedRowStyle.FormatProvider = rowsDefaultCellStyle.FormatProvider;
            }
            else if (((rowIndex % 2) == 1) && !alternatingRowsDefaultCellStyle.IsFormatProviderDefault)
            {
                inheritedRowStyle.FormatProvider = alternatingRowsDefaultCellStyle.FormatProvider;
            }
            else
            {
                inheritedRowStyle.FormatProvider = defaultCellStyle.FormatProvider;
            }
            if ((style != null) && (style.Alignment != DataGridViewContentAlignment.NotSet))
            {
                inheritedRowStyle.AlignmentInternal = style.Alignment;
            }
            else if ((rowsDefaultCellStyle.Alignment != DataGridViewContentAlignment.NotSet) && (((rowIndex % 2) == 0) || (alternatingRowsDefaultCellStyle.Alignment == DataGridViewContentAlignment.NotSet)))
            {
                inheritedRowStyle.AlignmentInternal = rowsDefaultCellStyle.Alignment;
            }
            else if (((rowIndex % 2) == 1) && (alternatingRowsDefaultCellStyle.Alignment != DataGridViewContentAlignment.NotSet))
            {
                inheritedRowStyle.AlignmentInternal = alternatingRowsDefaultCellStyle.Alignment;
            }
            else
            {
                inheritedRowStyle.AlignmentInternal = defaultCellStyle.Alignment;
            }
            if ((style != null) && (style.WrapMode != DataGridViewTriState.NotSet))
            {
                inheritedRowStyle.WrapModeInternal = style.WrapMode;
            }
            else if ((rowsDefaultCellStyle.WrapMode != DataGridViewTriState.NotSet) && (((rowIndex % 2) == 0) || (alternatingRowsDefaultCellStyle.WrapMode == DataGridViewTriState.NotSet)))
            {
                inheritedRowStyle.WrapModeInternal = rowsDefaultCellStyle.WrapMode;
            }
            else if (((rowIndex % 2) == 1) && (alternatingRowsDefaultCellStyle.WrapMode != DataGridViewTriState.NotSet))
            {
                inheritedRowStyle.WrapModeInternal = alternatingRowsDefaultCellStyle.WrapMode;
            }
            else
            {
                inheritedRowStyle.WrapModeInternal = defaultCellStyle.WrapMode;
            }
            if ((style != null) && (style.Tag != null))
            {
                inheritedRowStyle.Tag = style.Tag;
            }
            else if ((rowsDefaultCellStyle.Tag != null) && (((rowIndex % 2) == 0) || (alternatingRowsDefaultCellStyle.Tag == null)))
            {
                inheritedRowStyle.Tag = rowsDefaultCellStyle.Tag;
            }
            else if (((rowIndex % 2) == 1) && (alternatingRowsDefaultCellStyle.Tag != null))
            {
                inheritedRowStyle.Tag = alternatingRowsDefaultCellStyle.Tag;
            }
            else
            {
                inheritedRowStyle.Tag = defaultCellStyle.Tag;
            }
            if ((style != null) && (style.Padding != Padding.Empty))
            {
                inheritedRowStyle.PaddingInternal = style.Padding;
            }
            else if ((rowsDefaultCellStyle.Padding != Padding.Empty) && (((rowIndex % 2) == 0) || (alternatingRowsDefaultCellStyle.Padding == Padding.Empty)))
            {
                inheritedRowStyle.PaddingInternal = rowsDefaultCellStyle.Padding;
            }
            else if (((rowIndex % 2) == 1) && (alternatingRowsDefaultCellStyle.Padding != Padding.Empty))
            {
                inheritedRowStyle.PaddingInternal = alternatingRowsDefaultCellStyle.Padding;
            }
            else
            {
                inheritedRowStyle.PaddingInternal = defaultCellStyle.Padding;
            }
        }

        public override object Clone()
        {
            DataGridViewRow row;
            System.Type type = base.GetType();
            if (type == rowType)
            {
                row = new DataGridViewRow();
            }
            else
            {
                row = (DataGridViewRow) Activator.CreateInstance(type);
            }
            if (row != null)
            {
                base.CloneInternal(row);
                if (this.HasErrorText)
                {
                    row.ErrorText = this.ErrorTextInternal;
                }
                if (base.HasHeaderCell)
                {
                    row.HeaderCell = (DataGridViewRowHeaderCell) this.HeaderCell.Clone();
                }
                row.CloneCells(this);
            }
            return row;
        }

        private void CloneCells(DataGridViewRow rowTemplate)
        {
            int count = rowTemplate.Cells.Count;
            if (count > 0)
            {
                DataGridViewCell[] dataGridViewCells = new DataGridViewCell[count];
                for (int i = 0; i < count; i++)
                {
                    DataGridViewCell cell = rowTemplate.Cells[i];
                    dataGridViewCells[i] = (DataGridViewCell) cell.Clone();
                }
                this.Cells.AddRange(dataGridViewCells);
            }
        }

        protected virtual AccessibleObject CreateAccessibilityInstance()
        {
            return new DataGridViewRowAccessibleObject(this);
        }

        public void CreateCells(DataGridView dataGridView)
        {
            if (dataGridView == null)
            {
                throw new ArgumentNullException("dataGridView");
            }
            if (base.DataGridView != null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_RowAlreadyBelongsToDataGridView"));
            }
            DataGridViewCellCollection cells = this.Cells;
            cells.Clear();
            foreach (DataGridViewColumn column in dataGridView.Columns)
            {
                if (column.CellTemplate == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_AColumnHasNoCellTemplate"));
                }
                DataGridViewCell dataGridViewCell = (DataGridViewCell) column.CellTemplate.Clone();
                cells.Add(dataGridViewCell);
            }
        }

        public void CreateCells(DataGridView dataGridView, params object[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            this.CreateCells(dataGridView);
            this.SetValuesInternal(values);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual DataGridViewCellCollection CreateCellsInstance()
        {
            return new DataGridViewCellCollection(this);
        }

        internal void DetachFromDataGridView()
        {
            if (base.DataGridView != null)
            {
                base.DataGridViewInternal = null;
                base.IndexInternal = -1;
                if (base.HasHeaderCell)
                {
                    this.HeaderCell.DataGridViewInternal = null;
                }
                foreach (DataGridViewCell cell in this.Cells)
                {
                    cell.DataGridViewInternal = null;
                    if (cell.Selected)
                    {
                        cell.SelectedInternal = false;
                    }
                }
                if (this.Selected)
                {
                    base.SelectedInternal = false;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected internal virtual void DrawFocus(Graphics graphics, Rectangle clipBounds, Rectangle bounds, int rowIndex, DataGridViewElementStates rowState, DataGridViewCellStyle cellStyle, bool cellsPaintSelectionBackground)
        {
            Color selectionBackColor;
            if (base.DataGridView == null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_RowDoesNotYetBelongToDataGridView"));
            }
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            if (cellStyle == null)
            {
                throw new ArgumentNullException("cellStyle");
            }
            if (cellsPaintSelectionBackground && ((rowState & DataGridViewElementStates.Selected) != DataGridViewElementStates.None))
            {
                selectionBackColor = cellStyle.SelectionBackColor;
            }
            else
            {
                selectionBackColor = cellStyle.BackColor;
            }
            ControlPaint.DrawFocusRectangle(graphics, bounds, Color.Empty, selectionBackColor);
        }

        public System.Windows.Forms.ContextMenuStrip GetContextMenuStrip(int rowIndex)
        {
            System.Windows.Forms.ContextMenuStrip contextMenuStripInternal = base.ContextMenuStripInternal;
            if (base.DataGridView == null)
            {
                return contextMenuStripInternal;
            }
            if (rowIndex == -1)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidOperationOnSharedRow"));
            }
            if ((rowIndex < 0) || (rowIndex >= base.DataGridView.Rows.Count))
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            if (!base.DataGridView.VirtualMode && (base.DataGridView.DataSource == null))
            {
                return contextMenuStripInternal;
            }
            return base.DataGridView.OnRowContextMenuStripNeeded(rowIndex, contextMenuStripInternal);
        }

        internal bool GetDisplayed(int rowIndex)
        {
            return ((this.GetState(rowIndex) & DataGridViewElementStates.Displayed) != DataGridViewElementStates.None);
        }

        public string GetErrorText(int rowIndex)
        {
            string errorTextInternal = this.ErrorTextInternal;
            if (base.DataGridView == null)
            {
                return errorTextInternal;
            }
            if (rowIndex == -1)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidOperationOnSharedRow"));
            }
            if ((rowIndex < 0) || (rowIndex >= base.DataGridView.Rows.Count))
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            if ((string.IsNullOrEmpty(errorTextInternal) && (base.DataGridView.DataSource != null)) && (rowIndex != base.DataGridView.NewRowIndex))
            {
                errorTextInternal = base.DataGridView.DataConnection.GetError(rowIndex);
            }
            if ((base.DataGridView.DataSource == null) && !base.DataGridView.VirtualMode)
            {
                return errorTextInternal;
            }
            return base.DataGridView.OnRowErrorTextNeeded(rowIndex, errorTextInternal);
        }

        internal bool GetFrozen(int rowIndex)
        {
            return ((this.GetState(rowIndex) & DataGridViewElementStates.Frozen) != DataGridViewElementStates.None);
        }

        internal int GetHeight(int rowIndex)
        {
            int num;
            int num2;
            base.GetHeightInfo(rowIndex, out num, out num2);
            return num;
        }

        internal int GetMinimumHeight(int rowIndex)
        {
            int num;
            int num2;
            base.GetHeightInfo(rowIndex, out num, out num2);
            return num2;
        }

        public virtual int GetPreferredHeight(int rowIndex, DataGridViewAutoSizeRowMode autoSizeRowMode, bool fixedWidth)
        {
            if ((autoSizeRowMode & ~DataGridViewAutoSizeRowMode.AllCells) != ((DataGridViewAutoSizeRowMode) 0))
            {
                throw new InvalidEnumArgumentException("autoSizeRowMode", (int) autoSizeRowMode, typeof(DataGridViewAutoSizeRowMode));
            }
            if ((base.DataGridView != null) && ((rowIndex < 0) || (rowIndex >= base.DataGridView.Rows.Count)))
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            if (base.DataGridView == null)
            {
                return -1;
            }
            int num = 0;
            if (base.DataGridView.RowHeadersVisible && ((autoSizeRowMode & DataGridViewAutoSizeRowMode.RowHeader) != ((DataGridViewAutoSizeRowMode) 0)))
            {
                if ((fixedWidth || (base.DataGridView.RowHeadersWidthSizeMode == DataGridViewRowHeadersWidthSizeMode.EnableResizing)) || (base.DataGridView.RowHeadersWidthSizeMode == DataGridViewRowHeadersWidthSizeMode.DisableResizing))
                {
                    num = Math.Max(num, this.HeaderCell.GetPreferredHeight(rowIndex, base.DataGridView.RowHeadersWidth));
                }
                else
                {
                    num = Math.Max(num, this.HeaderCell.GetPreferredSize(rowIndex).Height);
                }
            }
            if ((autoSizeRowMode & DataGridViewAutoSizeRowMode.AllCellsExceptHeader) != ((DataGridViewAutoSizeRowMode) 0))
            {
                foreach (DataGridViewCell cell in this.Cells)
                {
                    DataGridViewColumn column = base.DataGridView.Columns[cell.ColumnIndex];
                    if (column.Visible)
                    {
                        int preferredHeight;
                        if (fixedWidth || ((column.InheritedAutoSizeMode & (DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader | DataGridViewAutoSizeColumnMode.AllCellsExceptHeader)) == DataGridViewAutoSizeColumnMode.NotSet))
                        {
                            preferredHeight = cell.GetPreferredHeight(rowIndex, column.Width);
                        }
                        else
                        {
                            preferredHeight = cell.GetPreferredSize(rowIndex).Height;
                        }
                        if (num < preferredHeight)
                        {
                            num = preferredHeight;
                        }
                    }
                }
            }
            return num;
        }

        internal bool GetReadOnly(int rowIndex)
        {
            return (((this.GetState(rowIndex) & DataGridViewElementStates.ReadOnly) != DataGridViewElementStates.None) || ((base.DataGridView != null) && base.DataGridView.ReadOnly));
        }

        internal DataGridViewTriState GetResizable(int rowIndex)
        {
            if ((this.GetState(rowIndex) & DataGridViewElementStates.ResizableSet) != DataGridViewElementStates.None)
            {
                if ((this.GetState(rowIndex) & DataGridViewElementStates.Resizable) == DataGridViewElementStates.None)
                {
                    return DataGridViewTriState.False;
                }
                return DataGridViewTriState.True;
            }
            if (base.DataGridView == null)
            {
                return DataGridViewTriState.NotSet;
            }
            if (!base.DataGridView.AllowUserToResizeRows)
            {
                return DataGridViewTriState.False;
            }
            return DataGridViewTriState.True;
        }

        internal bool GetSelected(int rowIndex)
        {
            return ((this.GetState(rowIndex) & DataGridViewElementStates.Selected) != DataGridViewElementStates.None);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public virtual DataGridViewElementStates GetState(int rowIndex)
        {
            if ((base.DataGridView != null) && ((rowIndex < 0) || (rowIndex >= base.DataGridView.Rows.Count)))
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            if ((base.DataGridView != null) && (base.DataGridView.Rows.SharedRow(rowIndex).Index == -1))
            {
                return base.DataGridView.Rows.GetRowState(rowIndex);
            }
            if (rowIndex != base.Index)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "rowIndex", rowIndex.ToString(CultureInfo.CurrentCulture) }));
            }
            return base.State;
        }

        internal bool GetVisible(int rowIndex)
        {
            return ((this.GetState(rowIndex) & DataGridViewElementStates.Visible) != DataGridViewElementStates.None);
        }

        internal void OnSharedStateChanged(int sharedRowIndex, DataGridViewElementStates elementState)
        {
            base.DataGridView.Rows.InvalidateCachedRowCount(elementState);
            base.DataGridView.Rows.InvalidateCachedRowsHeight(elementState);
            base.DataGridView.OnDataGridViewElementStateChanged(this, sharedRowIndex, elementState);
        }

        internal void OnSharedStateChanging(int sharedRowIndex, DataGridViewElementStates elementState)
        {
            base.DataGridView.OnDataGridViewElementStateChanging(this, sharedRowIndex, elementState);
        }

        protected internal virtual void Paint(Graphics graphics, Rectangle clipBounds, Rectangle rowBounds, int rowIndex, DataGridViewElementStates rowState, bool isFirstDisplayedRow, bool isLastVisibleRow)
        {
            if (base.DataGridView == null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_RowDoesNotYetBelongToDataGridView"));
            }
            DataGridView dataGridView = base.DataGridView;
            Rectangle rectangle = clipBounds;
            DataGridViewRow row = dataGridView.Rows.SharedRow(rowIndex);
            DataGridViewCellStyle inheritedRowStyle = new DataGridViewCellStyle();
            this.BuildInheritedRowStyle(rowIndex, inheritedRowStyle);
            DataGridViewRowPrePaintEventArgs rowPrePaintEventArgs = dataGridView.RowPrePaintEventArgs;
            rowPrePaintEventArgs.SetProperties(graphics, clipBounds, rowBounds, rowIndex, rowState, row.GetErrorText(rowIndex), inheritedRowStyle, isFirstDisplayedRow, isLastVisibleRow);
            dataGridView.OnRowPrePaint(rowPrePaintEventArgs);
            if (!rowPrePaintEventArgs.Handled)
            {
                DataGridViewPaintParts paintParts = rowPrePaintEventArgs.PaintParts;
                rectangle = rowPrePaintEventArgs.ClipBounds;
                this.PaintHeader(graphics, rectangle, rowBounds, rowIndex, rowState, isFirstDisplayedRow, isLastVisibleRow, paintParts);
                this.PaintCells(graphics, rectangle, rowBounds, rowIndex, rowState, isFirstDisplayedRow, isLastVisibleRow, paintParts);
                row = dataGridView.Rows.SharedRow(rowIndex);
                this.BuildInheritedRowStyle(rowIndex, inheritedRowStyle);
                DataGridViewRowPostPaintEventArgs rowPostPaintEventArgs = dataGridView.RowPostPaintEventArgs;
                rowPostPaintEventArgs.SetProperties(graphics, rectangle, rowBounds, rowIndex, rowState, row.GetErrorText(rowIndex), inheritedRowStyle, isFirstDisplayedRow, isLastVisibleRow);
                dataGridView.OnRowPostPaint(rowPostPaintEventArgs);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected internal virtual void PaintCells(Graphics graphics, Rectangle clipBounds, Rectangle rowBounds, int rowIndex, DataGridViewElementStates rowState, bool isFirstDisplayedRow, bool isLastVisibleRow, DataGridViewPaintParts paintParts)
        {
            DataGridViewCell cell;
            DataGridViewAdvancedBorderStyle style3;
            if (base.DataGridView == null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_RowDoesNotYetBelongToDataGridView"));
            }
            if ((paintParts < DataGridViewPaintParts.None) || (paintParts > DataGridViewPaintParts.All))
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_InvalidDataGridViewPaintPartsCombination", new object[] { "paintParts" }));
            }
            DataGridView dataGridView = base.DataGridView;
            Rectangle rect = rowBounds;
            int num = dataGridView.RowHeadersVisible ? dataGridView.RowHeadersWidth : 0;
            bool isFirstDisplayedColumn = true;
            DataGridViewElementStates none = DataGridViewElementStates.None;
            DataGridViewCellStyle inheritedCellStyle = new DataGridViewCellStyle();
            DataGridViewColumn column = null;
            DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStylePlaceholder = new DataGridViewAdvancedBorderStyle();
            DataGridViewColumn firstColumn = dataGridView.Columns.GetFirstColumn(DataGridViewElementStates.Visible | DataGridViewElementStates.Frozen);
            while (firstColumn != null)
            {
                cell = this.Cells[firstColumn.Index];
                rect.Width = firstColumn.Thickness;
                if (dataGridView.SingleVerticalBorderAdded && isFirstDisplayedColumn)
                {
                    rect.Width++;
                }
                if (dataGridView.RightToLeftInternal)
                {
                    rect.X = (rowBounds.Right - num) - rect.Width;
                }
                else
                {
                    rect.X = rowBounds.X + num;
                }
                column = dataGridView.Columns.GetNextColumn(firstColumn, DataGridViewElementStates.Visible | DataGridViewElementStates.Frozen, DataGridViewElementStates.None);
                if (clipBounds.IntersectsWith(rect))
                {
                    none = cell.CellStateFromColumnRowStates(rowState);
                    if (base.Index != -1)
                    {
                        none |= cell.State;
                    }
                    cell.GetInheritedStyle(inheritedCellStyle, rowIndex, true);
                    style3 = cell.AdjustCellBorderStyle(dataGridView.AdvancedCellBorderStyle, dataGridViewAdvancedBorderStylePlaceholder, dataGridView.SingleVerticalBorderAdded, dataGridView.SingleHorizontalBorderAdded, isFirstDisplayedColumn, isFirstDisplayedRow);
                    cell.PaintWork(graphics, clipBounds, rect, rowIndex, none, inheritedCellStyle, style3, paintParts);
                }
                num += rect.Width;
                if (num >= rowBounds.Width)
                {
                    break;
                }
                firstColumn = column;
                isFirstDisplayedColumn = false;
            }
            Rectangle rectangle2 = rowBounds;
            if ((num < rectangle2.Width) && (dataGridView.FirstDisplayedScrollingColumnIndex >= 0))
            {
                if (!dataGridView.RightToLeftInternal)
                {
                    rectangle2.X -= dataGridView.FirstDisplayedScrollingColumnHiddenWidth;
                }
                rectangle2.Width += dataGridView.FirstDisplayedScrollingColumnHiddenWidth;
                Region clip = null;
                if (dataGridView.FirstDisplayedScrollingColumnHiddenWidth > 0)
                {
                    clip = graphics.Clip;
                    Rectangle rectangle3 = rowBounds;
                    if (!dataGridView.RightToLeftInternal)
                    {
                        rectangle3.X += num;
                    }
                    rectangle3.Width -= num;
                    graphics.SetClip(rectangle3);
                }
                firstColumn = dataGridView.Columns[dataGridView.FirstDisplayedScrollingColumnIndex];
                while (firstColumn != null)
                {
                    cell = this.Cells[firstColumn.Index];
                    rect.Width = firstColumn.Thickness;
                    if (dataGridView.SingleVerticalBorderAdded && isFirstDisplayedColumn)
                    {
                        rect.Width++;
                    }
                    if (dataGridView.RightToLeftInternal)
                    {
                        rect.X = (rectangle2.Right - num) - rect.Width;
                    }
                    else
                    {
                        rect.X = rectangle2.X + num;
                    }
                    column = dataGridView.Columns.GetNextColumn(firstColumn, DataGridViewElementStates.Visible, DataGridViewElementStates.None);
                    if (clipBounds.IntersectsWith(rect))
                    {
                        none = cell.CellStateFromColumnRowStates(rowState);
                        if (base.Index != -1)
                        {
                            none |= cell.State;
                        }
                        cell.GetInheritedStyle(inheritedCellStyle, rowIndex, true);
                        style3 = cell.AdjustCellBorderStyle(dataGridView.AdvancedCellBorderStyle, dataGridViewAdvancedBorderStylePlaceholder, dataGridView.SingleVerticalBorderAdded, dataGridView.SingleHorizontalBorderAdded, isFirstDisplayedColumn, isFirstDisplayedRow);
                        cell.PaintWork(graphics, clipBounds, rect, rowIndex, none, inheritedCellStyle, style3, paintParts);
                    }
                    num += rect.Width;
                    if (num >= rectangle2.Width)
                    {
                        break;
                    }
                    firstColumn = column;
                    isFirstDisplayedColumn = false;
                }
                if (clip != null)
                {
                    graphics.Clip = clip;
                    clip.Dispose();
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected internal virtual void PaintHeader(Graphics graphics, Rectangle clipBounds, Rectangle rowBounds, int rowIndex, DataGridViewElementStates rowState, bool isFirstDisplayedRow, bool isLastVisibleRow, DataGridViewPaintParts paintParts)
        {
            if (base.DataGridView == null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_RowDoesNotYetBelongToDataGridView"));
            }
            if ((paintParts < DataGridViewPaintParts.None) || (paintParts > DataGridViewPaintParts.All))
            {
                throw new InvalidEnumArgumentException("paintParts", (int) paintParts, typeof(DataGridViewPaintParts));
            }
            DataGridView dataGridView = base.DataGridView;
            if (dataGridView.RowHeadersVisible)
            {
                Rectangle rect = rowBounds;
                rect.Width = dataGridView.RowHeadersWidth;
                if (dataGridView.RightToLeftInternal)
                {
                    rect.X = rowBounds.Right - rect.Width;
                }
                if (clipBounds.IntersectsWith(rect))
                {
                    DataGridViewCellStyle inheritedCellStyle = new DataGridViewCellStyle();
                    DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStylePlaceholder = new DataGridViewAdvancedBorderStyle();
                    this.BuildInheritedRowHeaderCellStyle(inheritedCellStyle);
                    DataGridViewAdvancedBorderStyle advancedBorderStyle = this.AdjustRowHeaderBorderStyle(dataGridView.AdvancedRowHeadersBorderStyle, dataGridViewAdvancedBorderStylePlaceholder, dataGridView.SingleVerticalBorderAdded, dataGridView.SingleHorizontalBorderAdded, isFirstDisplayedRow, isLastVisibleRow);
                    this.HeaderCell.PaintWork(graphics, clipBounds, rect, rowIndex, rowState, inheritedCellStyle, advancedBorderStyle, paintParts);
                }
            }
        }

        internal void SetReadOnlyCellCore(DataGridViewCell dataGridViewCell, bool readOnly)
        {
            if (this.ReadOnly && !readOnly)
            {
                foreach (DataGridViewCell cell in this.Cells)
                {
                    cell.ReadOnlyInternal = true;
                }
                dataGridViewCell.ReadOnlyInternal = false;
                this.ReadOnly = false;
            }
            else if (!this.ReadOnly && readOnly)
            {
                dataGridViewCell.ReadOnlyInternal = true;
            }
        }

        public bool SetValues(params object[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            if (base.DataGridView != null)
            {
                if (base.DataGridView.VirtualMode)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidOperationInVirtualMode"));
                }
                if (base.Index == -1)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidOperationOnSharedRow"));
                }
            }
            return this.SetValuesInternal(values);
        }

        internal bool SetValuesInternal(params object[] values)
        {
            bool flag = true;
            DataGridViewCellCollection cells = this.Cells;
            int count = cells.Count;
            for (int i = 0; i < cells.Count; i++)
            {
                if (i == values.Length)
                {
                    break;
                }
                if (!cells[i].SetValueInternal(base.Index, values[i]))
                {
                    flag = false;
                }
            }
            return (flag && (values.Length <= count));
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(0x24);
            builder.Append("DataGridViewRow { Index=");
            builder.Append(base.Index.ToString(CultureInfo.CurrentCulture));
            builder.Append(" }");
            return builder.ToString();
        }

        [Browsable(false)]
        public AccessibleObject AccessibilityObject
        {
            get
            {
                AccessibleObject obj2 = (AccessibleObject) base.Properties.GetObject(PropRowAccessibilityObject);
                if (obj2 == null)
                {
                    obj2 = this.CreateAccessibilityInstance();
                    base.Properties.SetObject(PropRowAccessibilityObject, obj2);
                }
                return obj2;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public DataGridViewCellCollection Cells
        {
            get
            {
                if (this.rowCells == null)
                {
                    this.rowCells = this.CreateCellsInstance();
                }
                return this.rowCells;
            }
        }

        [DefaultValue((string) null), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("DataGridView_RowContextMenuStripDescr")]
        public override System.Windows.Forms.ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return base.ContextMenuStrip;
            }
            set
            {
                base.ContextMenuStrip = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
        public object DataBoundItem
        {
            get
            {
                if (((base.DataGridView != null) && (base.DataGridView.DataConnection != null)) && ((base.Index > -1) && (base.Index != base.DataGridView.NewRowIndex)))
                {
                    return base.DataGridView.DataConnection.CurrencyManager[base.Index];
                }
                return null;
            }
        }

        [System.Windows.Forms.SRDescription("DataGridView_RowDefaultCellStyleDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), System.Windows.Forms.SRCategory("CatAppearance"), Browsable(true), NotifyParentProperty(true)]
        public override DataGridViewCellStyle DefaultCellStyle
        {
            get
            {
                return base.DefaultCellStyle;
            }
            set
            {
                if ((base.DataGridView != null) && (base.Index == -1))
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidPropertySetOnSharedRow", new object[] { "DefaultCellStyle" }));
                }
                base.DefaultCellStyle = value;
            }
        }

        [Browsable(false)]
        public override bool Displayed
        {
            get
            {
                if ((base.DataGridView != null) && (base.Index == -1))
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidPropertyGetOnSharedRow", new object[] { "Displayed" }));
                }
                return this.GetDisplayed(base.Index);
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("DataGridView_RowDividerHeightDescr"), DefaultValue(0), NotifyParentProperty(true)]
        public int DividerHeight
        {
            get
            {
                return base.DividerThickness;
            }
            set
            {
                if ((base.DataGridView != null) && (base.Index == -1))
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidPropertySetOnSharedRow", new object[] { "DividerHeight" }));
                }
                base.DividerThickness = value;
            }
        }

        [DefaultValue(""), System.Windows.Forms.SRCategory("CatAppearance"), NotifyParentProperty(true), System.Windows.Forms.SRDescription("DataGridView_RowErrorTextDescr")]
        public string ErrorText
        {
            get
            {
                return this.GetErrorText(base.Index);
            }
            set
            {
                this.ErrorTextInternal = value;
            }
        }

        private string ErrorTextInternal
        {
            get
            {
                object obj2 = base.Properties.GetObject(PropRowErrorText);
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                string errorTextInternal = this.ErrorTextInternal;
                if (!string.IsNullOrEmpty(value) || base.Properties.ContainsObject(PropRowErrorText))
                {
                    base.Properties.SetObject(PropRowErrorText, value);
                }
                if ((base.DataGridView != null) && !errorTextInternal.Equals(this.ErrorTextInternal))
                {
                    base.DataGridView.OnRowErrorTextChanged(this);
                }
            }
        }

        [Browsable(false)]
        public override bool Frozen
        {
            get
            {
                if ((base.DataGridView != null) && (base.Index == -1))
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidPropertyGetOnSharedRow", new object[] { "Frozen" }));
                }
                return this.GetFrozen(base.Index);
            }
            set
            {
                if ((base.DataGridView != null) && (base.Index == -1))
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidPropertySetOnSharedRow", new object[] { "Frozen" }));
                }
                base.Frozen = value;
            }
        }

        internal bool HasErrorText
        {
            get
            {
                return (base.Properties.ContainsObject(PropRowErrorText) && (base.Properties.GetObject(PropRowErrorText) != null));
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public DataGridViewRowHeaderCell HeaderCell
        {
            get
            {
                return (DataGridViewRowHeaderCell) base.HeaderCellCore;
            }
            set
            {
                base.HeaderCellCore = value;
            }
        }

        [System.Windows.Forms.SRDescription("DataGridView_RowHeightDescr"), DefaultValue(0x16), NotifyParentProperty(true), System.Windows.Forms.SRCategory("CatAppearance")]
        public int Height
        {
            get
            {
                return base.Thickness;
            }
            set
            {
                if ((base.DataGridView != null) && (base.Index == -1))
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidPropertySetOnSharedRow", new object[] { "Height" }));
                }
                base.Thickness = value;
            }
        }

        public override DataGridViewCellStyle InheritedStyle
        {
            get
            {
                if (base.Index == -1)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidPropertyGetOnSharedRow", new object[] { "InheritedStyle" }));
                }
                DataGridViewCellStyle inheritedRowStyle = new DataGridViewCellStyle();
                this.BuildInheritedRowStyle(base.Index, inheritedRowStyle);
                return inheritedRowStyle;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsNewRow
        {
            get
            {
                return ((base.DataGridView != null) && (base.DataGridView.NewRowIndex == base.Index));
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int MinimumHeight
        {
            get
            {
                return base.MinimumThickness;
            }
            set
            {
                if ((base.DataGridView != null) && (base.Index == -1))
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidPropertySetOnSharedRow", new object[] { "MinimumHeight" }));
                }
                base.MinimumThickness = value;
            }
        }

        [DefaultValue(false), NotifyParentProperty(true), Browsable(true), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("DataGridView_RowReadOnlyDescr")]
        public override bool ReadOnly
        {
            get
            {
                if ((base.DataGridView != null) && (base.Index == -1))
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidPropertyGetOnSharedRow", new object[] { "ReadOnly" }));
                }
                return this.GetReadOnly(base.Index);
            }
            set
            {
                base.ReadOnly = value;
            }
        }

        [System.Windows.Forms.SRDescription("DataGridView_RowResizableDescr"), NotifyParentProperty(true), System.Windows.Forms.SRCategory("CatBehavior")]
        public override DataGridViewTriState Resizable
        {
            get
            {
                if ((base.DataGridView != null) && (base.Index == -1))
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidPropertyGetOnSharedRow", new object[] { "Resizable" }));
                }
                return this.GetResizable(base.Index);
            }
            set
            {
                base.Resizable = value;
            }
        }

        public override bool Selected
        {
            get
            {
                if ((base.DataGridView != null) && (base.Index == -1))
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidPropertyGetOnSharedRow", new object[] { "Selected" }));
                }
                return this.GetSelected(base.Index);
            }
            set
            {
                base.Selected = value;
            }
        }

        public override DataGridViewElementStates State
        {
            get
            {
                if ((base.DataGridView != null) && (base.Index == -1))
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidPropertyGetOnSharedRow", new object[] { "State" }));
                }
                return this.GetState(base.Index);
            }
        }

        [Browsable(false)]
        public override bool Visible
        {
            get
            {
                if ((base.DataGridView != null) && (base.Index == -1))
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidPropertyGetOnSharedRow", new object[] { "Visible" }));
                }
                return this.GetVisible(base.Index);
            }
            set
            {
                if ((base.DataGridView != null) && (base.Index == -1))
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidPropertySetOnSharedRow", new object[] { "Visible" }));
                }
                base.Visible = value;
            }
        }

        [ComVisible(true)]
        protected class DataGridViewRowAccessibleObject : AccessibleObject
        {
            private DataGridViewRow owner;
            private DataGridViewRow.DataGridViewSelectedRowCellsAccessibleObject selectedCellsAccessibilityObject;

            public DataGridViewRowAccessibleObject()
            {
            }

            public DataGridViewRowAccessibleObject(DataGridViewRow owner)
            {
                this.owner = owner;
            }

            public override AccessibleObject GetChild(int index)
            {
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                if (this.owner == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowAccessibleObject_OwnerNotSet"));
                }
                if ((index == 0) && this.owner.DataGridView.RowHeadersVisible)
                {
                    return this.owner.HeaderCell.AccessibilityObject;
                }
                if (this.owner.DataGridView.RowHeadersVisible)
                {
                    index--;
                }
                int num = this.owner.DataGridView.Columns.ActualDisplayIndexToColumnIndex(index, DataGridViewElementStates.Visible);
                return this.owner.Cells[num].AccessibilityObject;
            }

            public override int GetChildCount()
            {
                if (this.owner == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowAccessibleObject_OwnerNotSet"));
                }
                int columnCount = this.owner.DataGridView.Columns.GetColumnCount(DataGridViewElementStates.Visible);
                if (this.owner.DataGridView.RowHeadersVisible)
                {
                    columnCount++;
                }
                return columnCount;
            }

            public override AccessibleObject GetFocused()
            {
                if (this.owner == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowAccessibleObject_OwnerNotSet"));
                }
                if ((this.owner.DataGridView.Focused && (this.owner.DataGridView.CurrentCell != null)) && (this.owner.DataGridView.CurrentCell.RowIndex == this.owner.Index))
                {
                    return this.owner.DataGridView.CurrentCell.AccessibilityObject;
                }
                return null;
            }

            public override AccessibleObject GetSelected()
            {
                return this.SelectedCellsAccessibilityObject;
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override AccessibleObject Navigate(AccessibleNavigation navigationDirection)
            {
                if (this.owner == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowAccessibleObject_OwnerNotSet"));
                }
                switch (navigationDirection)
                {
                    case AccessibleNavigation.Up:
                    case AccessibleNavigation.Previous:
                    {
                        if (this.owner.Index == this.owner.DataGridView.Rows.GetFirstRow(DataGridViewElementStates.Visible))
                        {
                            if (this.owner.DataGridView.ColumnHeadersVisible)
                            {
                                return this.ParentPrivate.GetChild(0);
                            }
                            return null;
                        }
                        int previousRow = this.owner.DataGridView.Rows.GetPreviousRow(this.owner.Index, DataGridViewElementStates.Visible);
                        int index = this.owner.DataGridView.Rows.GetRowCount(DataGridViewElementStates.Visible, 0, previousRow);
                        if (!this.owner.DataGridView.ColumnHeadersVisible)
                        {
                            return this.owner.DataGridView.AccessibilityObject.GetChild(index);
                        }
                        return this.owner.DataGridView.AccessibilityObject.GetChild(index + 1);
                    }
                    case AccessibleNavigation.Down:
                    case AccessibleNavigation.Next:
                    {
                        if (this.owner.Index == this.owner.DataGridView.Rows.GetLastRow(DataGridViewElementStates.Visible))
                        {
                            return null;
                        }
                        int nextRow = this.owner.DataGridView.Rows.GetNextRow(this.owner.Index, DataGridViewElementStates.Visible);
                        int num2 = this.owner.DataGridView.Rows.GetRowCount(DataGridViewElementStates.Visible, 0, nextRow);
                        if (!this.owner.DataGridView.ColumnHeadersVisible)
                        {
                            return this.owner.DataGridView.AccessibilityObject.GetChild(num2);
                        }
                        return this.owner.DataGridView.AccessibilityObject.GetChild(num2 + 1);
                    }
                    case AccessibleNavigation.FirstChild:
                        if (this.GetChildCount() != 0)
                        {
                            return this.GetChild(0);
                        }
                        return null;

                    case AccessibleNavigation.LastChild:
                    {
                        int childCount = this.GetChildCount();
                        if (childCount != 0)
                        {
                            return this.GetChild(childCount - 1);
                        }
                        return null;
                    }
                }
                return null;
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void Select(AccessibleSelection flags)
            {
                if (this.owner == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowAccessibleObject_OwnerNotSet"));
                }
                DataGridView dataGridView = this.owner.DataGridView;
                if (dataGridView != null)
                {
                    if ((flags & AccessibleSelection.TakeFocus) == AccessibleSelection.TakeFocus)
                    {
                        dataGridView.FocusInternal();
                    }
                    if (((flags & AccessibleSelection.TakeSelection) == AccessibleSelection.TakeSelection) && (this.owner.Cells.Count > 0))
                    {
                        if ((dataGridView.CurrentCell != null) && (dataGridView.CurrentCell.OwningColumn != null))
                        {
                            dataGridView.CurrentCell = this.owner.Cells[dataGridView.CurrentCell.OwningColumn.Index];
                        }
                        else
                        {
                            int index = dataGridView.Columns.GetFirstColumn(DataGridViewElementStates.Visible).Index;
                            if (index > -1)
                            {
                                dataGridView.CurrentCell = this.owner.Cells[index];
                            }
                        }
                    }
                    if ((((flags & AccessibleSelection.AddSelection) == AccessibleSelection.AddSelection) && ((flags & AccessibleSelection.TakeSelection) == AccessibleSelection.None)) && ((dataGridView.SelectionMode == DataGridViewSelectionMode.FullRowSelect) || (dataGridView.SelectionMode == DataGridViewSelectionMode.RowHeaderSelect)))
                    {
                        this.owner.Selected = true;
                    }
                    if (((flags & AccessibleSelection.RemoveSelection) == AccessibleSelection.RemoveSelection) && ((flags & (AccessibleSelection.AddSelection | AccessibleSelection.TakeSelection)) == AccessibleSelection.None))
                    {
                        this.owner.Selected = false;
                    }
                }
            }

            public override Rectangle Bounds
            {
                get
                {
                    Rectangle bounds;
                    if (this.owner == null)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowAccessibleObject_OwnerNotSet"));
                    }
                    if (this.owner.Index < this.owner.DataGridView.FirstDisplayedScrollingRowIndex)
                    {
                        int num = this.owner.DataGridView.Rows.GetRowCount(DataGridViewElementStates.Visible, 0, this.owner.Index);
                        bounds = this.ParentPrivate.GetChild((num + 1) + 1).Bounds;
                        bounds.Y -= this.owner.Height;
                        bounds.Height = this.owner.Height;
                        return bounds;
                    }
                    if ((this.owner.Index >= this.owner.DataGridView.FirstDisplayedScrollingRowIndex) && (this.owner.Index < (this.owner.DataGridView.FirstDisplayedScrollingRowIndex + this.owner.DataGridView.DisplayedRowCount(true))))
                    {
                        bounds = this.owner.DataGridView.GetRowDisplayRectangle(this.owner.Index, false);
                        return this.owner.DataGridView.RectangleToScreen(bounds);
                    }
                    int index = this.owner.DataGridView.Rows.GetRowCount(DataGridViewElementStates.Visible, 0, this.owner.Index);
                    if (!this.owner.DataGridView.Rows[0].Visible)
                    {
                        index--;
                    }
                    if (!this.owner.DataGridView.ColumnHeadersVisible)
                    {
                        index--;
                    }
                    bounds = this.ParentPrivate.GetChild(index).Bounds;
                    bounds.Y += bounds.Height;
                    bounds.Height = this.owner.Height;
                    return bounds;
                }
            }

            public override string Name
            {
                get
                {
                    if (this.owner == null)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowAccessibleObject_OwnerNotSet"));
                    }
                    return System.Windows.Forms.SR.GetString("DataGridView_AccRowName", new object[] { this.owner.Index.ToString(CultureInfo.CurrentCulture) });
                }
            }

            public DataGridViewRow Owner
            {
                get
                {
                    return this.owner;
                }
                set
                {
                    if (this.owner != null)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowAccessibleObject_OwnerAlreadySet"));
                    }
                    this.owner = value;
                }
            }

            public override AccessibleObject Parent
            {
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    return this.ParentPrivate;
                }
            }

            private AccessibleObject ParentPrivate
            {
                get
                {
                    if (this.owner == null)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowAccessibleObject_OwnerNotSet"));
                    }
                    return this.owner.DataGridView.AccessibilityObject;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    return AccessibleRole.Row;
                }
            }

            private AccessibleObject SelectedCellsAccessibilityObject
            {
                get
                {
                    if (this.owner == null)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowAccessibleObject_OwnerNotSet"));
                    }
                    if (this.selectedCellsAccessibilityObject == null)
                    {
                        this.selectedCellsAccessibilityObject = new DataGridViewRow.DataGridViewSelectedRowCellsAccessibleObject(this.owner);
                    }
                    return this.selectedCellsAccessibilityObject;
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    if (this.owner == null)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowAccessibleObject_OwnerNotSet"));
                    }
                    AccessibleStates selectable = AccessibleStates.Selectable;
                    bool flag = true;
                    if (this.owner.Selected)
                    {
                        flag = true;
                    }
                    else
                    {
                        for (int i = 0; i < this.owner.Cells.Count; i++)
                        {
                            if (!this.owner.Cells[i].Selected)
                            {
                                flag = false;
                                break;
                            }
                        }
                    }
                    if (flag)
                    {
                        selectable |= AccessibleStates.Selected;
                    }
                    if (!this.owner.DataGridView.GetRowDisplayRectangle(this.owner.Index, true).IntersectsWith(this.owner.DataGridView.ClientRectangle))
                    {
                        selectable |= AccessibleStates.Offscreen;
                    }
                    return selectable;
                }
            }

            public override string Value
            {
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    if (this.owner == null)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowAccessibleObject_OwnerNotSet"));
                    }
                    if (this.owner.DataGridView.AllowUserToAddRows && (this.owner.Index == this.owner.DataGridView.NewRowIndex))
                    {
                        return System.Windows.Forms.SR.GetString("DataGridView_AccRowCreateNew");
                    }
                    StringBuilder builder = new StringBuilder(0x400);
                    int childCount = this.GetChildCount();
                    int num2 = this.owner.DataGridView.RowHeadersVisible ? 1 : 0;
                    for (int i = num2; i < childCount; i++)
                    {
                        AccessibleObject child = this.GetChild(i);
                        if (child != null)
                        {
                            builder.Append(child.Value);
                        }
                        if (i != (childCount - 1))
                        {
                            builder.Append(";");
                        }
                    }
                    return builder.ToString();
                }
            }
        }

        private class DataGridViewSelectedRowCellsAccessibleObject : AccessibleObject
        {
            private DataGridViewRow owner;

            internal DataGridViewSelectedRowCellsAccessibleObject(DataGridViewRow owner)
            {
                this.owner = owner;
            }

            public override AccessibleObject GetChild(int index)
            {
                if (index < this.GetChildCount())
                {
                    int num = -1;
                    for (int i = 1; i < this.owner.AccessibilityObject.GetChildCount(); i++)
                    {
                        if ((this.owner.AccessibilityObject.GetChild(i).State & AccessibleStates.Selected) == AccessibleStates.Selected)
                        {
                            num++;
                        }
                        if (num == index)
                        {
                            return this.owner.AccessibilityObject.GetChild(i);
                        }
                    }
                }
                return null;
            }

            public override int GetChildCount()
            {
                int num = 0;
                for (int i = 1; i < this.owner.AccessibilityObject.GetChildCount(); i++)
                {
                    if ((this.owner.AccessibilityObject.GetChild(i).State & AccessibleStates.Selected) == AccessibleStates.Selected)
                    {
                        num++;
                    }
                }
                return num;
            }

            public override AccessibleObject GetFocused()
            {
                if ((this.owner.DataGridView.CurrentCell != null) && this.owner.DataGridView.CurrentCell.Selected)
                {
                    return this.owner.DataGridView.CurrentCell.AccessibilityObject;
                }
                return null;
            }

            public override AccessibleObject GetSelected()
            {
                return this;
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override AccessibleObject Navigate(AccessibleNavigation navigationDirection)
            {
                switch (navigationDirection)
                {
                    case AccessibleNavigation.FirstChild:
                        if (this.GetChildCount() <= 0)
                        {
                            return null;
                        }
                        return this.GetChild(0);

                    case AccessibleNavigation.LastChild:
                        if (this.GetChildCount() <= 0)
                        {
                            return null;
                        }
                        return this.GetChild(this.GetChildCount() - 1);
                }
                return null;
            }

            public override string Name
            {
                get
                {
                    return System.Windows.Forms.SR.GetString("DataGridView_AccSelectedRowCellsName");
                }
            }

            public override AccessibleObject Parent
            {
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    return this.owner.AccessibilityObject;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    return AccessibleRole.Grouping;
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    return (AccessibleStates.Selectable | AccessibleStates.Selected);
                }
            }

            public override string Value
            {
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    return this.Name;
                }
            }
        }
    }
}

