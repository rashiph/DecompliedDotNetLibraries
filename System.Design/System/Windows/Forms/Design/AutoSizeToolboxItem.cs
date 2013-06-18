namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Runtime.Serialization;
    using System.Windows.Forms;

    [Serializable]
    internal class AutoSizeToolboxItem : ToolboxItem
    {
        public AutoSizeToolboxItem()
        {
        }

        public AutoSizeToolboxItem(System.Type toolType) : base(toolType)
        {
        }

        private AutoSizeToolboxItem(SerializationInfo info, StreamingContext context)
        {
            this.Deserialize(info, context);
        }

        protected override IComponent[] CreateComponentsCore(IDesignerHost host)
        {
            IComponent[] componentArray = base.CreateComponentsCore(host);
            if (((componentArray != null) && (componentArray.Length > 0)) && (componentArray[0] is Control))
            {
                Control control = componentArray[0] as Control;
                control.AutoSize = true;
            }
            return componentArray;
        }
    }
}

