namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;

    public class DataGridViewElement
    {
        private System.Windows.Forms.DataGridView dataGridView;
        private DataGridViewElementStates state;

        public DataGridViewElement()
        {
            this.state = DataGridViewElementStates.Visible;
        }

        internal DataGridViewElement(DataGridViewElement dgveTemplate)
        {
            this.state = dgveTemplate.State & (DataGridViewElementStates.Visible | DataGridViewElementStates.ResizableSet | DataGridViewElementStates.Resizable | DataGridViewElementStates.ReadOnly | DataGridViewElementStates.Frozen);
        }

        protected virtual void OnDataGridViewChanged()
        {
        }

        protected void RaiseCellClick(DataGridViewCellEventArgs e)
        {
            if (this.dataGridView != null)
            {
                this.dataGridView.OnCellClickInternal(e);
            }
        }

        protected void RaiseCellContentClick(DataGridViewCellEventArgs e)
        {
            if (this.dataGridView != null)
            {
                this.dataGridView.OnCellContentClickInternal(e);
            }
        }

        protected void RaiseCellContentDoubleClick(DataGridViewCellEventArgs e)
        {
            if (this.dataGridView != null)
            {
                this.dataGridView.OnCellContentDoubleClickInternal(e);
            }
        }

        protected void RaiseCellValueChanged(DataGridViewCellEventArgs e)
        {
            if (this.dataGridView != null)
            {
                this.dataGridView.OnCellValueChangedInternal(e);
            }
        }

        protected void RaiseDataError(DataGridViewDataErrorEventArgs e)
        {
            if (this.dataGridView != null)
            {
                this.dataGridView.OnDataErrorInternal(e);
            }
        }

        protected void RaiseMouseWheel(MouseEventArgs e)
        {
            if (this.dataGridView != null)
            {
                this.dataGridView.OnMouseWheelInternal(e);
            }
        }

        internal bool StateExcludes(DataGridViewElementStates elementState)
        {
            return ((this.State & elementState) == DataGridViewElementStates.None);
        }

        internal bool StateIncludes(DataGridViewElementStates elementState)
        {
            return ((this.State & elementState) == elementState);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public System.Windows.Forms.DataGridView DataGridView
        {
            get
            {
                return this.dataGridView;
            }
        }

        internal System.Windows.Forms.DataGridView DataGridViewInternal
        {
            set
            {
                if (this.DataGridView != value)
                {
                    this.dataGridView = value;
                    this.OnDataGridViewChanged();
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
        public virtual DataGridViewElementStates State
        {
            get
            {
                return this.state;
            }
        }

        internal DataGridViewElementStates StateInternal
        {
            set
            {
                this.state = value;
            }
        }
    }
}

