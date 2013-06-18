namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    internal static class LoginDesignerUtil
    {
        internal abstract class GenericConvertToTemplateHelper<ControlType, ControlDesignerType> where ControlType: WebControl, IControlDesignerAccessor where ControlDesignerType: ControlDesigner
        {
            private ControlDesignerType _designer;
            private IDesignerHost _designerHost;
            private const string _failureTextID = "FailureText";

            public GenericConvertToTemplateHelper(ControlDesignerType designer, IDesignerHost designerHost)
            {
                this._designer = designer;
                this._designerHost = designerHost;
            }

            private void ConvertPersistedControlsToLiteralControls(Control defaultTemplateContents)
            {
                foreach (string str in this.PersistedControlIDs)
                {
                    Control control = defaultTemplateContents.FindControl(str);
                    if (control != null)
                    {
                        if (Array.IndexOf<string>(this.PersistedIfNotVisibleControlIDs, str) >= 0)
                        {
                            control.Visible = true;
                            control.Parent.Visible = true;
                            control.Parent.Parent.Visible = true;
                        }
                        if (control.Visible)
                        {
                            LiteralControl child = new LiteralControl(ControlPersister.PersistControl(control, this._designerHost));
                            ControlCollection controls = control.Parent.Controls;
                            int index = controls.IndexOf(control);
                            controls.Remove(control);
                            controls.AddAt(index, child);
                        }
                    }
                }
            }

            public ITemplate ConvertToTemplate()
            {
                ITemplate template = null;
                ITemplate template2 = this.GetTemplate(this.ViewControl);
                if (template2 != null)
                {
                    return template2;
                }
                this._designer.ViewControlCreated = false;
                Hashtable data = new Hashtable(1);
                data.Add("ConvertToTemplate", true);
                this.ViewControl.SetDesignModeState(data);
                this._designer.GetDesignTimeHtml();
                Control defaultTemplateContents = this.GetDefaultTemplateContents();
                this.SetFailureTextStyle(defaultTemplateContents);
                this.ConvertPersistedControlsToLiteralControls(defaultTemplateContents);
                StringWriter writer = new StringWriter(CultureInfo.CurrentCulture);
                HtmlTextWriter writer2 = new HtmlTextWriter(writer);
                defaultTemplateContents.RenderControl(writer2);
                template = ControlParser.ParseTemplate(this._designerHost, writer.ToString());
                Hashtable hashtable2 = new Hashtable(1);
                hashtable2.Add("ConvertToTemplate", false);
                this.ViewControl.SetDesignModeState(hashtable2);
                return template;
            }

            protected abstract Control GetDefaultTemplateContents();
            protected abstract Style GetFailureTextStyle(ControlType control);
            protected abstract ITemplate GetTemplate(ControlType control);
            private void SetFailureTextStyle(Control defaultTemplateContents)
            {
                Control control = defaultTemplateContents.FindControl("FailureText");
                if (control != null)
                {
                    TableCell parent = (TableCell) control.Parent;
                    parent.ForeColor = Color.Red;
                    parent.ApplyStyle(this.GetFailureTextStyle(this.ViewControl));
                    control.EnableViewState = false;
                }
            }

            protected ControlDesignerType Designer
            {
                get
                {
                    return this._designer;
                }
            }

            protected abstract string[] PersistedControlIDs { get; }

            protected abstract string[] PersistedIfNotVisibleControlIDs { get; }

            private ControlType ViewControl
            {
                get
                {
                    return (ControlType) this.Designer.ViewControl;
                }
            }
        }
    }
}

