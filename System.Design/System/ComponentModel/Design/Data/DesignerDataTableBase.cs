namespace System.ComponentModel.Design.Data
{
    using System;
    using System.Collections;

    public abstract class DesignerDataTableBase
    {
        private ICollection _columns;
        private string _name;
        private string _owner;

        protected DesignerDataTableBase(string name)
        {
            this._name = name;
        }

        protected DesignerDataTableBase(string name, string owner)
        {
            this._name = name;
            this._owner = owner;
        }

        protected abstract ICollection CreateColumns();

        public ICollection Columns
        {
            get
            {
                if (this._columns == null)
                {
                    this._columns = this.CreateColumns();
                }
                return this._columns;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public string Owner
        {
            get
            {
                return this._owner;
            }
        }
    }
}

