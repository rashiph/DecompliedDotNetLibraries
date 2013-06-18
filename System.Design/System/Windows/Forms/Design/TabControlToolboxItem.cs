namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Runtime.Serialization;
    using System.Windows.Forms;

    [Serializable]
    internal class TabControlToolboxItem : ToolboxItem
    {
        public TabControlToolboxItem() : base(typeof(TabControl))
        {
        }

        private TabControlToolboxItem(SerializationInfo info, StreamingContext context)
        {
            this.Deserialize(info, context);
        }

        protected override IComponent[] CreateComponentsCore(IDesignerHost host)
        {
            IComponent[] componentArray = base.CreateComponentsCore(host);
            if (((componentArray != null) && (componentArray.Length > 0)) && (componentArray[0] is TabControl))
            {
                TabControl control = (TabControl) componentArray[0];
                control.ShowToolTips = true;
            }
            return componentArray;
        }
    }
}

