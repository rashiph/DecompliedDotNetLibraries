namespace System.Web.UI.HtmlControls
{
    using System;
    using System.ComponentModel;
    using System.Web.UI;

    [SupportsEventValidation, DefaultEvent("")]
    public class HtmlInputReset : HtmlInputButton
    {
        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler ServerClick
        {
            add
            {
                base.ServerClick += value;
            }
            remove
            {
                base.ServerClick -= value;
            }
        }

        public HtmlInputReset() : base("reset")
        {
        }

        public HtmlInputReset(string type) : base(type)
        {
        }

        internal override void RenderAttributesInternal(HtmlTextWriter writer)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override bool CausesValidation
        {
            get
            {
                return base.CausesValidation;
            }
            set
            {
                base.CausesValidation = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override string ValidationGroup
        {
            get
            {
                return base.ValidationGroup;
            }
            set
            {
                base.ValidationGroup = value;
            }
        }
    }
}

