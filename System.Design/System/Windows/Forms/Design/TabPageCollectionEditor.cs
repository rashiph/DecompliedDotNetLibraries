namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel.Design;
    using System.Windows.Forms;

    internal class TabPageCollectionEditor : CollectionEditor
    {
        public TabPageCollectionEditor() : base(typeof(TabControl.TabPageCollection))
        {
        }

        protected override object CreateInstance(System.Type itemType)
        {
            TabPage page = base.CreateInstance(itemType) as TabPage;
            page.UseVisualStyleBackColor = true;
            return page;
        }

        protected override object SetItems(object editValue, object[] value)
        {
            TabControl instance = base.Context.Instance as TabControl;
            if (instance != null)
            {
                instance.SuspendLayout();
            }
            object obj2 = base.SetItems(editValue, value);
            if (instance != null)
            {
                instance.ResumeLayout();
            }
            return obj2;
        }
    }
}

