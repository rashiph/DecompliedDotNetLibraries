namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    [Obsolete("The recommended alternative is PanelContainerDesigner because it uses an EditableDesignerRegion for editing the content. Designer regions allow for better control of the content being edited. http://go.microsoft.com/fwlink/?linkid=14202"), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class PanelDesigner : ReadWriteControlDesigner
    {
        protected override void MapPropertyToStyle(string propName, object varPropValue)
        {
            if (((propName != null) && (varPropValue != null)) && (varPropValue != null))
            {
                try
                {
                    if (propName.Equals("BackImageUrl"))
                    {
                        string str = Convert.ToString(varPropValue, CultureInfo.InvariantCulture);
                        if ((str != null) && (str.Length != 0))
                        {
                            str = "url(" + str + ")";
                            this.BehaviorInternal.SetStyleAttribute("backgroundImage", true, str, true);
                        }
                    }
                    else if (propName.Equals("HorizontalAlign"))
                    {
                        string str2 = string.Empty;
                        if (((HorizontalAlign) varPropValue) != HorizontalAlign.NotSet)
                        {
                            str2 = Enum.Format(typeof(HorizontalAlign), varPropValue, "G");
                        }
                        this.BehaviorInternal.SetStyleAttribute("textAlign", true, str2, true);
                    }
                    else
                    {
                        base.MapPropertyToStyle(propName, varPropValue);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        [Obsolete("The recommended alternative is ControlDesigner.Tag. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected override void OnBehaviorAttached()
        {
            base.OnBehaviorAttached();
            Panel component = (Panel) base.Component;
            string backImageUrl = component.BackImageUrl;
            if (backImageUrl != null)
            {
                this.MapPropertyToStyle("BackImageUrl", backImageUrl);
            }
            HorizontalAlign horizontalAlign = component.HorizontalAlign;
            if (horizontalAlign != HorizontalAlign.NotSet)
            {
                this.MapPropertyToStyle("HorizontalAlign", horizontalAlign);
            }
        }
    }
}

