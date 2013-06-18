namespace System.ComponentModel.Design.Data
{
    using System;
    using System.Collections;

    public abstract class DesignerDataTable : DesignerDataTableBase
    {
        private ICollection _relationships;

        protected DesignerDataTable(string name) : base(name)
        {
        }

        protected DesignerDataTable(string name, string owner) : base(name, owner)
        {
        }

        protected abstract ICollection CreateRelationships();

        public ICollection Relationships
        {
            get
            {
                if (this._relationships == null)
                {
                    this._relationships = this.CreateRelationships();
                }
                return this._relationships;
            }
        }
    }
}

