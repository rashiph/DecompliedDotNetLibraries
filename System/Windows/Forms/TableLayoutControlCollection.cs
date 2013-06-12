namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;

    [DesignerSerializer("System.Windows.Forms.Design.TableLayoutControlCollectionCodeDomSerializer, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ListBindable(false)]
    public class TableLayoutControlCollection : Control.ControlCollection
    {
        private TableLayoutPanel _container;

        public TableLayoutControlCollection(TableLayoutPanel container) : base(container)
        {
            this._container = container;
        }

        public virtual void Add(Control control, int column, int row)
        {
            base.Add(control);
            this._container.SetColumn(control, column);
            this._container.SetRow(control, row);
        }

        public TableLayoutPanel Container
        {
            get
            {
                return this._container;
            }
        }
    }
}

