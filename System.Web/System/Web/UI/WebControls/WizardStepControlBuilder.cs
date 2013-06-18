namespace System.Web.UI.WebControls
{
    using System;
    using System.Web;
    using System.Web.UI;

    public sealed class WizardStepControlBuilder : ControlBuilder
    {
        internal override void SetParentBuilder(ControlBuilder parentBuilder)
        {
            if (!base.Parser.FInDesigner && !(base.Parser is PageThemeParser))
            {
                if ((parentBuilder.ControlType == null) || !typeof(WizardStepCollection).IsAssignableFrom(parentBuilder.ControlType))
                {
                    throw new HttpException(System.Web.SR.GetString("WizardStep_WrongContainment"));
                }
                base.SetParentBuilder(parentBuilder);
            }
        }
    }
}

