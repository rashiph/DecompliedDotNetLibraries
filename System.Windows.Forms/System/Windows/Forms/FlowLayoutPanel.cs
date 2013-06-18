namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Windows.Forms.Layout;

    [ClassInterface(ClassInterfaceType.AutoDispatch), Designer("System.Windows.Forms.Design.FlowLayoutPanelDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), Docking(DockingBehavior.Ask), System.Windows.Forms.SRDescription("DescriptionFlowLayoutPanel"), DefaultProperty("FlowDirection"), ProvideProperty("FlowBreak", typeof(Control)), ComVisible(true)]
    public class FlowLayoutPanel : Panel, IExtenderProvider
    {
        private FlowLayoutSettings _flowLayoutSettings;

        public FlowLayoutPanel()
        {
            this._flowLayoutSettings = FlowLayout.CreateSettings(this);
        }

        [DefaultValue(false), DisplayName("FlowBreak")]
        public bool GetFlowBreak(Control control)
        {
            return this._flowLayoutSettings.GetFlowBreak(control);
        }

        [DisplayName("FlowBreak")]
        public void SetFlowBreak(Control control, bool value)
        {
            this._flowLayoutSettings.SetFlowBreak(control, value);
        }

        bool IExtenderProvider.CanExtend(object obj)
        {
            Control control = obj as Control;
            return ((control != null) && (control.Parent == this));
        }

        [DefaultValue(0), System.Windows.Forms.SRCategory("CatLayout"), Localizable(true), System.Windows.Forms.SRDescription("FlowPanelFlowDirectionDescr")]
        public System.Windows.Forms.FlowDirection FlowDirection
        {
            get
            {
                return this._flowLayoutSettings.FlowDirection;
            }
            set
            {
                this._flowLayoutSettings.FlowDirection = value;
            }
        }

        public override System.Windows.Forms.Layout.LayoutEngine LayoutEngine
        {
            get
            {
                return FlowLayout.Instance;
            }
        }

        [DefaultValue(true), System.Windows.Forms.SRDescription("FlowPanelWrapContentsDescr"), System.Windows.Forms.SRCategory("CatLayout"), Localizable(true)]
        public bool WrapContents
        {
            get
            {
                return this._flowLayoutSettings.WrapContents;
            }
            set
            {
                this._flowLayoutSettings.WrapContents = value;
            }
        }
    }
}

