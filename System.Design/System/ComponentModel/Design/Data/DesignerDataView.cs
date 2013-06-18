namespace System.ComponentModel.Design.Data
{
    using System;

    public abstract class DesignerDataView : DesignerDataTableBase
    {
        protected DesignerDataView(string name) : base(name)
        {
        }

        protected DesignerDataView(string name, string owner) : base(name, owner)
        {
        }
    }
}

