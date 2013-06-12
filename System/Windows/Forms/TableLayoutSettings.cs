namespace System.Windows.Forms
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Windows.Forms.Layout;

    [Serializable, TypeConverter(typeof(TableLayoutSettingsTypeConverter))]
    public sealed class TableLayoutSettings : LayoutSettings, ISerializable
    {
        private TableLayoutPanelCellBorderStyle _borderStyle;
        private TableLayoutSettingsStub _stub;
        private static int[] borderStyleToOffset = new int[] { 0, 1, 2, 3, 2, 3, 3 };

        internal TableLayoutSettings() : base(null)
        {
            this._stub = new TableLayoutSettingsStub();
        }

        internal TableLayoutSettings(IArrangedElement owner) : base(owner)
        {
        }

        internal TableLayoutSettings(SerializationInfo serializationInfo, StreamingContext context) : this()
        {
            TypeConverter converter = TypeDescriptor.GetConverter(this);
            string str = serializationInfo.GetString("SerializedString");
            if (!string.IsNullOrEmpty(str) && (converter != null))
            {
                TableLayoutSettings settings = converter.ConvertFromInvariantString(str) as TableLayoutSettings;
                if (settings != null)
                {
                    this.ApplySettings(settings);
                }
            }
        }

        internal void ApplySettings(TableLayoutSettings settings)
        {
            if (settings.IsStub)
            {
                if (!this.IsStub)
                {
                    settings._stub.ApplySettings(this);
                }
                else
                {
                    this._stub = settings._stub;
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), DefaultValue(-1), System.Windows.Forms.SRDescription("TableLayoutSettingsGetCellPositionDescr")]
        public TableLayoutPanelCellPosition GetCellPosition(object control)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            return new TableLayoutPanelCellPosition(this.GetColumn(control), this.GetRow(control));
        }

        [System.Windows.Forms.SRDescription("GridPanelColumnDescr"), DefaultValue(-1), System.Windows.Forms.SRCategory("CatLayout")]
        public int GetColumn(object control)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            if (this.IsStub)
            {
                return this._stub.GetColumn(control);
            }
            return System.Windows.Forms.Layout.TableLayout.GetLayoutInfo(this.LayoutEngine.CastToArrangedElement(control)).ColumnPosition;
        }

        public int GetColumnSpan(object control)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            if (this.IsStub)
            {
                return this._stub.GetColumnSpan(control);
            }
            return System.Windows.Forms.Layout.TableLayout.GetLayoutInfo(this.LayoutEngine.CastToArrangedElement(control)).ColumnSpan;
        }

        internal IArrangedElement GetControlFromPosition(int column, int row)
        {
            return this.TableLayout.GetControlFromPosition(base.Owner, column, row);
        }

        internal List<ControlInformation> GetControlsInformation()
        {
            if (this.IsStub)
            {
                return this._stub.GetControlsInformation();
            }
            List<ControlInformation> list = new List<ControlInformation>(base.Owner.Children.Count);
            foreach (IArrangedElement element in base.Owner.Children)
            {
                Control component = element as Control;
                if (component != null)
                {
                    ControlInformation item = new ControlInformation();
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component)["Name"];
                    if ((descriptor != null) && (descriptor.PropertyType == typeof(string)))
                    {
                        item.Name = descriptor.GetValue(component);
                    }
                    item.Row = this.GetRow(component);
                    item.RowSpan = this.GetRowSpan(component);
                    item.Column = this.GetColumn(component);
                    item.ColumnSpan = this.GetColumnSpan(component);
                    list.Add(item);
                }
            }
            return list;
        }

        internal TableLayoutPanelCellPosition GetPositionFromControl(IArrangedElement element)
        {
            return this.TableLayout.GetPositionFromControl(base.Owner, element);
        }

        [DefaultValue(-1), System.Windows.Forms.SRDescription("GridPanelRowDescr"), System.Windows.Forms.SRCategory("CatLayout")]
        public int GetRow(object control)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            if (this.IsStub)
            {
                return this._stub.GetRow(control);
            }
            return System.Windows.Forms.Layout.TableLayout.GetLayoutInfo(this.LayoutEngine.CastToArrangedElement(control)).RowPosition;
        }

        public int GetRowSpan(object control)
        {
            if (this.IsStub)
            {
                return this._stub.GetRowSpan(control);
            }
            return System.Windows.Forms.Layout.TableLayout.GetLayoutInfo(this.LayoutEngine.CastToArrangedElement(control)).RowSpan;
        }

        [DefaultValue(-1), System.Windows.Forms.SRDescription("TableLayoutSettingsSetCellPositionDescr"), System.Windows.Forms.SRCategory("CatLayout")]
        public void SetCellPosition(object control, TableLayoutPanelCellPosition cellPosition)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            this.SetCellPosition(control, cellPosition.Row, cellPosition.Column, true, true);
        }

        private void SetCellPosition(object control, int row, int column, bool rowSpecified, bool colSpecified)
        {
            if (this.IsStub)
            {
                if (colSpecified)
                {
                    this._stub.SetColumn(control, column);
                }
                if (rowSpecified)
                {
                    this._stub.SetRow(control, row);
                }
            }
            else
            {
                IArrangedElement element = this.LayoutEngine.CastToArrangedElement(control);
                if (element.Container != null)
                {
                    System.Windows.Forms.Layout.TableLayout.ClearCachedAssignments(System.Windows.Forms.Layout.TableLayout.GetContainerInfo(element.Container));
                }
                System.Windows.Forms.Layout.TableLayout.LayoutInfo layoutInfo = System.Windows.Forms.Layout.TableLayout.GetLayoutInfo(element);
                if (colSpecified)
                {
                    layoutInfo.ColumnPosition = column;
                }
                if (rowSpecified)
                {
                    layoutInfo.RowPosition = row;
                }
                LayoutTransaction.DoLayout(element.Container, element, PropertyNames.TableIndex);
            }
        }

        public void SetColumn(object control, int column)
        {
            if (column < -1)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "Column", column.ToString(CultureInfo.CurrentCulture) }));
            }
            if (this.IsStub)
            {
                this._stub.SetColumn(control, column);
            }
            else
            {
                this.SetCellPosition(control, -1, column, false, true);
            }
        }

        public void SetColumnSpan(object control, int value)
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException("ColumnSpan", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "ColumnSpan", value.ToString(CultureInfo.CurrentCulture) }));
            }
            if (this.IsStub)
            {
                this._stub.SetColumnSpan(control, value);
            }
            else
            {
                IArrangedElement element = this.LayoutEngine.CastToArrangedElement(control);
                if (element.Container != null)
                {
                    System.Windows.Forms.Layout.TableLayout.ClearCachedAssignments(System.Windows.Forms.Layout.TableLayout.GetContainerInfo(element.Container));
                }
                System.Windows.Forms.Layout.TableLayout.GetLayoutInfo(element).ColumnSpan = value;
                LayoutTransaction.DoLayout(element.Container, element, PropertyNames.ColumnSpan);
            }
        }

        public void SetRow(object control, int row)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            if (row < -1)
            {
                throw new ArgumentOutOfRangeException("Row", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "Row", row.ToString(CultureInfo.CurrentCulture) }));
            }
            this.SetCellPosition(control, row, -1, true, false);
        }

        public void SetRowSpan(object control, int value)
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException("RowSpan", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "RowSpan", value.ToString(CultureInfo.CurrentCulture) }));
            }
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            if (this.IsStub)
            {
                this._stub.SetRowSpan(control, value);
            }
            else
            {
                IArrangedElement element = this.LayoutEngine.CastToArrangedElement(control);
                if (element.Container != null)
                {
                    System.Windows.Forms.Layout.TableLayout.ClearCachedAssignments(System.Windows.Forms.Layout.TableLayout.GetContainerInfo(element.Container));
                }
                System.Windows.Forms.Layout.TableLayout.GetLayoutInfo(element).RowSpan = value;
                LayoutTransaction.DoLayout(element.Container, element, PropertyNames.RowSpan);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(this);
            string str = (converter != null) ? converter.ConvertToInvariantString(this) : null;
            if (!string.IsNullOrEmpty(str))
            {
                si.AddValue("SerializedString", str);
            }
        }

        [System.Windows.Forms.SRDescription("TableLayoutPanelCellBorderStyleDescr"), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(0)]
        internal TableLayoutPanelCellBorderStyle CellBorderStyle
        {
            get
            {
                return this._borderStyle;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 6))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "CellBorderStyle", value.ToString() }));
                }
                this._borderStyle = value;
                System.Windows.Forms.Layout.TableLayout.GetContainerInfo(base.Owner).CellBorderWidth = borderStyleToOffset[(int) value];
                LayoutTransaction.DoLayout(base.Owner, base.Owner, PropertyNames.CellBorderStyle);
            }
        }

        [DefaultValue(0)]
        internal int CellBorderWidth
        {
            get
            {
                return System.Windows.Forms.Layout.TableLayout.GetContainerInfo(base.Owner).CellBorderWidth;
            }
        }

        [DefaultValue(0), System.Windows.Forms.SRDescription("GridPanelColumnsDescr"), System.Windows.Forms.SRCategory("CatLayout")]
        public int ColumnCount
        {
            get
            {
                return System.Windows.Forms.Layout.TableLayout.GetContainerInfo(base.Owner).MaxColumns;
            }
            set
            {
                if (value < 0)
                {
                    object[] args = new object[] { "ColumnCount", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("ColumnCount", value, System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                System.Windows.Forms.Layout.TableLayout.GetContainerInfo(base.Owner).MaxColumns = value;
                LayoutTransaction.DoLayout(base.Owner, base.Owner, PropertyNames.Columns);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), System.Windows.Forms.SRDescription("GridPanelColumnStylesDescr"), System.Windows.Forms.SRCategory("CatLayout")]
        public TableLayoutColumnStyleCollection ColumnStyles
        {
            get
            {
                if (this.IsStub)
                {
                    return this._stub.ColumnStyles;
                }
                return System.Windows.Forms.Layout.TableLayout.GetContainerInfo(base.Owner).ColumnStyles;
            }
        }

        [System.Windows.Forms.SRDescription("TableLayoutPanelGrowStyleDescr"), System.Windows.Forms.SRCategory("CatLayout"), DefaultValue(1)]
        public TableLayoutPanelGrowStyle GrowStyle
        {
            get
            {
                return System.Windows.Forms.Layout.TableLayout.GetContainerInfo(base.Owner).GrowStyle;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "GrowStyle", value.ToString() }));
                }
                System.Windows.Forms.Layout.TableLayout.ContainerInfo containerInfo = System.Windows.Forms.Layout.TableLayout.GetContainerInfo(base.Owner);
                if (containerInfo.GrowStyle != value)
                {
                    containerInfo.GrowStyle = value;
                    LayoutTransaction.DoLayout(base.Owner, base.Owner, PropertyNames.GrowStyle);
                }
            }
        }

        internal bool IsStub
        {
            get
            {
                return (this._stub != null);
            }
        }

        public override System.Windows.Forms.Layout.LayoutEngine LayoutEngine
        {
            get
            {
                return System.Windows.Forms.Layout.TableLayout.Instance;
            }
        }

        [DefaultValue(0), System.Windows.Forms.SRDescription("GridPanelRowsDescr"), System.Windows.Forms.SRCategory("CatLayout")]
        public int RowCount
        {
            get
            {
                return System.Windows.Forms.Layout.TableLayout.GetContainerInfo(base.Owner).MaxRows;
            }
            set
            {
                if (value < 0)
                {
                    object[] args = new object[] { "RowCount", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("RowCount", value, System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                System.Windows.Forms.Layout.TableLayout.GetContainerInfo(base.Owner).MaxRows = value;
                LayoutTransaction.DoLayout(base.Owner, base.Owner, PropertyNames.Rows);
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("GridPanelRowStylesDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TableLayoutRowStyleCollection RowStyles
        {
            get
            {
                if (this.IsStub)
                {
                    return this._stub.RowStyles;
                }
                return System.Windows.Forms.Layout.TableLayout.GetContainerInfo(base.Owner).RowStyles;
            }
        }

        private System.Windows.Forms.Layout.TableLayout TableLayout
        {
            get
            {
                return (System.Windows.Forms.Layout.TableLayout) this.LayoutEngine;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct ControlInformation
        {
            internal object Name;
            internal int Row;
            internal int Column;
            internal int RowSpan;
            internal int ColumnSpan;
            internal ControlInformation(object name, int row, int column, int rowSpan, int columnSpan)
            {
                this.Name = name;
                this.Row = row;
                this.Column = column;
                this.RowSpan = rowSpan;
                this.ColumnSpan = columnSpan;
            }
        }

        internal class StyleConverter : TypeConverter
        {
            public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
            {
                return ((destinationType == typeof(InstanceDescriptor)) || base.CanConvertTo(context, destinationType));
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
            {
                if (destinationType == null)
                {
                    throw new ArgumentNullException("destinationType");
                }
                if ((destinationType == typeof(InstanceDescriptor)) && (value is TableLayoutStyle))
                {
                    TableLayoutStyle style = (TableLayoutStyle) value;
                    switch (style.SizeType)
                    {
                        case SizeType.AutoSize:
                            return new InstanceDescriptor(style.GetType().GetConstructor(new System.Type[0]), new object[0]);

                        case SizeType.Absolute:
                        case SizeType.Percent:
                            return new InstanceDescriptor(style.GetType().GetConstructor(new System.Type[] { typeof(SizeType), typeof(int) }), new object[] { style.SizeType, style.Size });
                    }
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        private class TableLayoutSettingsStub
        {
            private TableLayoutColumnStyleCollection columnStyles;
            private Dictionary<object, TableLayoutSettings.ControlInformation> controlsInfo;
            private static TableLayoutSettings.ControlInformation DefaultControlInfo = new TableLayoutSettings.ControlInformation(null, -1, -1, 1, 1);
            private bool isValid = true;
            private TableLayoutRowStyleCollection rowStyles;

            internal void ApplySettings(TableLayoutSettings settings)
            {
                TableLayout.ContainerInfo containerInfo = TableLayout.GetContainerInfo(settings.Owner);
                Control container = containerInfo.Container as Control;
                if ((container != null) && (this.controlsInfo != null))
                {
                    foreach (object obj2 in this.controlsInfo.Keys)
                    {
                        TableLayoutSettings.ControlInformation information = this.controlsInfo[obj2];
                        foreach (Control control2 in container.Controls)
                        {
                            if (control2 != null)
                            {
                                string str = null;
                                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(control2)["Name"];
                                if ((descriptor != null) && (descriptor.PropertyType == typeof(string)))
                                {
                                    str = descriptor.GetValue(control2) as string;
                                }
                                if (WindowsFormsUtils.SafeCompareStrings(str, obj2 as string, false))
                                {
                                    settings.SetRow(control2, information.Row);
                                    settings.SetColumn(control2, information.Column);
                                    settings.SetRowSpan(control2, information.RowSpan);
                                    settings.SetColumnSpan(control2, information.ColumnSpan);
                                    break;
                                }
                            }
                        }
                    }
                }
                containerInfo.RowStyles = this.rowStyles;
                containerInfo.ColumnStyles = this.columnStyles;
                this.columnStyles = null;
                this.rowStyles = null;
                this.isValid = false;
            }

            public int GetColumn(object controlName)
            {
                return this.GetControlInformation(controlName).Column;
            }

            public int GetColumnSpan(object controlName)
            {
                return this.GetControlInformation(controlName).ColumnSpan;
            }

            private TableLayoutSettings.ControlInformation GetControlInformation(object controlName)
            {
                if (this.controlsInfo == null)
                {
                    return DefaultControlInfo;
                }
                if (!this.controlsInfo.ContainsKey(controlName))
                {
                    return DefaultControlInfo;
                }
                return this.controlsInfo[controlName];
            }

            internal List<TableLayoutSettings.ControlInformation> GetControlsInformation()
            {
                if (this.controlsInfo == null)
                {
                    return new List<TableLayoutSettings.ControlInformation>();
                }
                List<TableLayoutSettings.ControlInformation> list = new List<TableLayoutSettings.ControlInformation>(this.controlsInfo.Count);
                foreach (object obj2 in this.controlsInfo.Keys)
                {
                    TableLayoutSettings.ControlInformation item = this.controlsInfo[obj2];
                    item.Name = obj2;
                    list.Add(item);
                }
                return list;
            }

            public int GetRow(object controlName)
            {
                return this.GetControlInformation(controlName).Row;
            }

            public int GetRowSpan(object controlName)
            {
                return this.GetControlInformation(controlName).RowSpan;
            }

            public void SetColumn(object controlName, int column)
            {
                if (this.GetColumn(controlName) != column)
                {
                    TableLayoutSettings.ControlInformation controlInformation = this.GetControlInformation(controlName);
                    controlInformation.Column = column;
                    this.SetControlInformation(controlName, controlInformation);
                }
            }

            public void SetColumnSpan(object controlName, int value)
            {
                if (this.GetColumnSpan(controlName) != value)
                {
                    TableLayoutSettings.ControlInformation controlInformation = this.GetControlInformation(controlName);
                    controlInformation.ColumnSpan = value;
                    this.SetControlInformation(controlName, controlInformation);
                }
            }

            private void SetControlInformation(object controlName, TableLayoutSettings.ControlInformation info)
            {
                if (this.controlsInfo == null)
                {
                    this.controlsInfo = new Dictionary<object, TableLayoutSettings.ControlInformation>();
                }
                this.controlsInfo[controlName] = info;
            }

            public void SetRow(object controlName, int row)
            {
                if (this.GetRow(controlName) != row)
                {
                    TableLayoutSettings.ControlInformation controlInformation = this.GetControlInformation(controlName);
                    controlInformation.Row = row;
                    this.SetControlInformation(controlName, controlInformation);
                }
            }

            public void SetRowSpan(object controlName, int value)
            {
                if (this.GetRowSpan(controlName) != value)
                {
                    TableLayoutSettings.ControlInformation controlInformation = this.GetControlInformation(controlName);
                    controlInformation.RowSpan = value;
                    this.SetControlInformation(controlName, controlInformation);
                }
            }

            public TableLayoutColumnStyleCollection ColumnStyles
            {
                get
                {
                    if (this.columnStyles == null)
                    {
                        this.columnStyles = new TableLayoutColumnStyleCollection();
                    }
                    return this.columnStyles;
                }
            }

            public bool IsValid
            {
                get
                {
                    return this.isValid;
                }
            }

            public TableLayoutRowStyleCollection RowStyles
            {
                get
                {
                    if (this.rowStyles == null)
                    {
                        this.rowStyles = new TableLayoutRowStyleCollection();
                    }
                    return this.rowStyles;
                }
            }
        }
    }
}

