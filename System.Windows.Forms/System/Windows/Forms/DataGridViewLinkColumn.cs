namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Text;

    [ToolboxBitmap(typeof(DataGridViewLinkColumn), "DataGridViewLinkColumn.bmp")]
    public class DataGridViewLinkColumn : DataGridViewColumn
    {
        private static System.Type columnType = typeof(DataGridViewLinkColumn);
        private string text;

        public DataGridViewLinkColumn() : base(new DataGridViewLinkCell())
        {
        }

        public override object Clone()
        {
            DataGridViewLinkColumn column;
            System.Type type = base.GetType();
            if (type == columnType)
            {
                column = new DataGridViewLinkColumn();
            }
            else
            {
                column = (DataGridViewLinkColumn) Activator.CreateInstance(type);
            }
            if (column != null)
            {
                base.CloneInternal(column);
                column.Text = this.text;
            }
            return column;
        }

        private bool ShouldSerializeActiveLinkColor()
        {
            return !this.ActiveLinkColor.Equals(LinkUtilities.IEActiveLinkColor);
        }

        private bool ShouldSerializeLinkColor()
        {
            return !this.LinkColor.Equals(LinkUtilities.IELinkColor);
        }

        private bool ShouldSerializeVisitedLinkColor()
        {
            return !this.VisitedLinkColor.Equals(LinkUtilities.IEVisitedLinkColor);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(0x40);
            builder.Append("DataGridViewLinkColumn { Name=");
            builder.Append(base.Name);
            builder.Append(", Index=");
            builder.Append(base.Index.ToString(CultureInfo.CurrentCulture));
            builder.Append(" }");
            return builder.ToString();
        }

        [System.Windows.Forms.SRDescription("DataGridView_LinkColumnActiveLinkColorDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public Color ActiveLinkColor
        {
            get
            {
                if (this.CellTemplate == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewColumn_CellTemplateRequired"));
                }
                return ((DataGridViewLinkCell) this.CellTemplate).ActiveLinkColor;
            }
            set
            {
                if (!this.ActiveLinkColor.Equals(value))
                {
                    ((DataGridViewLinkCell) this.CellTemplate).ActiveLinkColorInternal = value;
                    if (base.DataGridView != null)
                    {
                        DataGridViewRowCollection rows = base.DataGridView.Rows;
                        int count = rows.Count;
                        for (int i = 0; i < count; i++)
                        {
                            DataGridViewLinkCell cell = rows.SharedRow(i).Cells[base.Index] as DataGridViewLinkCell;
                            if (cell != null)
                            {
                                cell.ActiveLinkColorInternal = value;
                            }
                        }
                        base.DataGridView.InvalidateColumn(base.Index);
                    }
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override DataGridViewCell CellTemplate
        {
            get
            {
                return base.CellTemplate;
            }
            set
            {
                if ((value != null) && !(value is DataGridViewLinkCell))
                {
                    throw new InvalidCastException(System.Windows.Forms.SR.GetString("DataGridViewTypeColumn_WrongCellTemplateType", new object[] { "System.Windows.Forms.DataGridViewLinkCell" }));
                }
                base.CellTemplate = value;
            }
        }

        [DefaultValue(0), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("DataGridView_LinkColumnLinkBehaviorDescr")]
        public System.Windows.Forms.LinkBehavior LinkBehavior
        {
            get
            {
                if (this.CellTemplate == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewColumn_CellTemplateRequired"));
                }
                return ((DataGridViewLinkCell) this.CellTemplate).LinkBehavior;
            }
            set
            {
                if (!this.LinkBehavior.Equals(value))
                {
                    ((DataGridViewLinkCell) this.CellTemplate).LinkBehavior = value;
                    if (base.DataGridView != null)
                    {
                        DataGridViewRowCollection rows = base.DataGridView.Rows;
                        int count = rows.Count;
                        for (int i = 0; i < count; i++)
                        {
                            DataGridViewLinkCell cell = rows.SharedRow(i).Cells[base.Index] as DataGridViewLinkCell;
                            if (cell != null)
                            {
                                cell.LinkBehaviorInternal = value;
                            }
                        }
                        base.DataGridView.InvalidateColumn(base.Index);
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("DataGridView_LinkColumnLinkColorDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public Color LinkColor
        {
            get
            {
                if (this.CellTemplate == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewColumn_CellTemplateRequired"));
                }
                return ((DataGridViewLinkCell) this.CellTemplate).LinkColor;
            }
            set
            {
                if (!this.LinkColor.Equals(value))
                {
                    ((DataGridViewLinkCell) this.CellTemplate).LinkColorInternal = value;
                    if (base.DataGridView != null)
                    {
                        DataGridViewRowCollection rows = base.DataGridView.Rows;
                        int count = rows.Count;
                        for (int i = 0; i < count; i++)
                        {
                            DataGridViewLinkCell cell = rows.SharedRow(i).Cells[base.Index] as DataGridViewLinkCell;
                            if (cell != null)
                            {
                                cell.LinkColorInternal = value;
                            }
                        }
                        base.DataGridView.InvalidateColumn(base.Index);
                    }
                }
            }
        }

        [DefaultValue((string) null), System.Windows.Forms.SRDescription("DataGridView_LinkColumnTextDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                if (!string.Equals(value, this.text, StringComparison.Ordinal))
                {
                    this.text = value;
                    if (base.DataGridView != null)
                    {
                        if (this.UseColumnTextForLinkValue)
                        {
                            base.DataGridView.OnColumnCommonChange(base.Index);
                        }
                        else
                        {
                            DataGridViewRowCollection rows = base.DataGridView.Rows;
                            int count = rows.Count;
                            for (int i = 0; i < count; i++)
                            {
                                DataGridViewLinkCell cell = rows.SharedRow(i).Cells[base.Index] as DataGridViewLinkCell;
                                if ((cell != null) && cell.UseColumnTextForLinkValue)
                                {
                                    base.DataGridView.OnColumnCommonChange(base.Index);
                                    return;
                                }
                            }
                            base.DataGridView.InvalidateColumn(base.Index);
                        }
                    }
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true), System.Windows.Forms.SRDescription("DataGridView_LinkColumnTrackVisitedStateDescr")]
        public bool TrackVisitedState
        {
            get
            {
                if (this.CellTemplate == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewColumn_CellTemplateRequired"));
                }
                return ((DataGridViewLinkCell) this.CellTemplate).TrackVisitedState;
            }
            set
            {
                if (this.TrackVisitedState != value)
                {
                    ((DataGridViewLinkCell) this.CellTemplate).TrackVisitedStateInternal = value;
                    if (base.DataGridView != null)
                    {
                        DataGridViewRowCollection rows = base.DataGridView.Rows;
                        int count = rows.Count;
                        for (int i = 0; i < count; i++)
                        {
                            DataGridViewLinkCell cell = rows.SharedRow(i).Cells[base.Index] as DataGridViewLinkCell;
                            if (cell != null)
                            {
                                cell.TrackVisitedStateInternal = value;
                            }
                        }
                        base.DataGridView.InvalidateColumn(base.Index);
                    }
                }
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("DataGridView_LinkColumnUseColumnTextForLinkValueDescr")]
        public bool UseColumnTextForLinkValue
        {
            get
            {
                if (this.CellTemplate == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewColumn_CellTemplateRequired"));
                }
                return ((DataGridViewLinkCell) this.CellTemplate).UseColumnTextForLinkValue;
            }
            set
            {
                if (this.UseColumnTextForLinkValue != value)
                {
                    ((DataGridViewLinkCell) this.CellTemplate).UseColumnTextForLinkValueInternal = value;
                    if (base.DataGridView != null)
                    {
                        DataGridViewRowCollection rows = base.DataGridView.Rows;
                        int count = rows.Count;
                        for (int i = 0; i < count; i++)
                        {
                            DataGridViewLinkCell cell = rows.SharedRow(i).Cells[base.Index] as DataGridViewLinkCell;
                            if (cell != null)
                            {
                                cell.UseColumnTextForLinkValueInternal = value;
                            }
                        }
                        base.DataGridView.OnColumnCommonChange(base.Index);
                    }
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("DataGridView_LinkColumnVisitedLinkColorDescr")]
        public Color VisitedLinkColor
        {
            get
            {
                if (this.CellTemplate == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewColumn_CellTemplateRequired"));
                }
                return ((DataGridViewLinkCell) this.CellTemplate).VisitedLinkColor;
            }
            set
            {
                if (!this.VisitedLinkColor.Equals(value))
                {
                    ((DataGridViewLinkCell) this.CellTemplate).VisitedLinkColorInternal = value;
                    if (base.DataGridView != null)
                    {
                        DataGridViewRowCollection rows = base.DataGridView.Rows;
                        int count = rows.Count;
                        for (int i = 0; i < count; i++)
                        {
                            DataGridViewLinkCell cell = rows.SharedRow(i).Cells[base.Index] as DataGridViewLinkCell;
                            if (cell != null)
                            {
                                cell.VisitedLinkColorInternal = value;
                            }
                        }
                        base.DataGridView.InvalidateColumn(base.Index);
                    }
                }
            }
        }
    }
}

