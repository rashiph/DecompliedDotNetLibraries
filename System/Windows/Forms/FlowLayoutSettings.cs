namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms.Layout;

    [DefaultProperty("FlowDirection")]
    public class FlowLayoutSettings : LayoutSettings
    {
        internal FlowLayoutSettings(IArrangedElement owner) : base(owner)
        {
        }

        public bool GetFlowBreak(object child)
        {
            return CommonProperties.GetFlowBreak(FlowLayout.Instance.CastToArrangedElement(child));
        }

        public void SetFlowBreak(object child, bool value)
        {
            IArrangedElement element = FlowLayout.Instance.CastToArrangedElement(child);
            if (this.GetFlowBreak(child) != value)
            {
                CommonProperties.SetFlowBreak(element, value);
            }
        }

        [System.Windows.Forms.SRDescription("FlowPanelFlowDirectionDescr"), System.Windows.Forms.SRCategory("CatLayout"), DefaultValue(0)]
        public System.Windows.Forms.FlowDirection FlowDirection
        {
            get
            {
                return FlowLayout.GetFlowDirection(base.Owner);
            }
            set
            {
                FlowLayout.SetFlowDirection(base.Owner, value);
            }
        }

        public override System.Windows.Forms.Layout.LayoutEngine LayoutEngine
        {
            get
            {
                return FlowLayout.Instance;
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("FlowPanelWrapContentsDescr"), DefaultValue(true)]
        public bool WrapContents
        {
            get
            {
                return FlowLayout.GetWrapContents(base.Owner);
            }
            set
            {
                FlowLayout.SetWrapContents(base.Owner, value);
            }
        }
    }
}

