namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class ComboBoxDesigner : ControlDesigner
    {
        private DesignerActionListCollection _actionLists;
        private EventHandler propChanged;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.propChanged != null))
            {
                ((ComboBox) this.Control).StyleChanged -= this.propChanged;
            }
            base.Dispose(disposing);
        }

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            base.AutoResizeHandles = true;
            this.propChanged = new EventHandler(this.OnControlPropertyChanged);
            ((ComboBox) this.Control).StyleChanged += this.propChanged;
        }

        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            base.InitializeNewComponent(defaultValues);
            ((ComboBox) base.Component).FormattingEnabled = true;
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(base.Component)["Text"];
            if (((descriptor != null) && (descriptor.PropertyType == typeof(string))) && (!descriptor.IsReadOnly && descriptor.IsBrowsable))
            {
                descriptor.SetValue(base.Component, "");
            }
        }

        private void OnControlPropertyChanged(object sender, EventArgs e)
        {
            if (base.BehaviorService != null)
            {
                base.BehaviorService.SyncSelection();
            }
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (this._actionLists == null)
                {
                    this._actionLists = new DesignerActionListCollection();
                    this._actionLists.Add(new ListControlBoundActionList(this));
                }
                return this._actionLists;
            }
        }

        public override System.Windows.Forms.Design.SelectionRules SelectionRules
        {
            get
            {
                System.Windows.Forms.Design.SelectionRules selectionRules = base.SelectionRules;
                object component = base.Component;
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component)["DropDownStyle"];
                if (descriptor == null)
                {
                    return selectionRules;
                }
                ComboBoxStyle style = (ComboBoxStyle) descriptor.GetValue(component);
                if ((style != ComboBoxStyle.DropDown) && (style != ComboBoxStyle.DropDownList))
                {
                    return selectionRules;
                }
                return (selectionRules & ~(System.Windows.Forms.Design.SelectionRules.BottomSizeable | System.Windows.Forms.Design.SelectionRules.TopSizeable));
            }
        }

        public override IList SnapLines
        {
            get
            {
                ArrayList snapLines = base.SnapLines as ArrayList;
                int offset = DesignerUtils.GetTextBaseline(this.Control, ContentAlignment.TopLeft) + 3;
                snapLines.Add(new SnapLine(SnapLineType.Baseline, offset, SnapLinePriority.Medium));
                return snapLines;
            }
        }
    }
}

