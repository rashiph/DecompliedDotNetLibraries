namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel.Design;
    using System.Windows.Forms;

    internal class DataGridColumnCollectionEditor : CollectionEditor
    {
        public DataGridColumnCollectionEditor(System.Type type) : base(type)
        {
        }

        protected override System.Type[] CreateNewItemTypes()
        {
            return new System.Type[] { typeof(DataGridTextBoxColumn), typeof(DataGridBoolColumn) };
        }
    }
}

