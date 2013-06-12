namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public sealed class LayoutEditorPart : EditorPart
    {
        private DropDownList _chromeState;
        private string _chromeStateErrorMessage;
        private DropDownList _zone;
        private TextBox _zoneIndex;
        private string _zoneIndexErrorMessage;
        private const int MinZoneIndex = 0;
        private const int TextBoxColumns = 10;

        public override bool ApplyChanges()
        {
            WebPart webPartToEdit = base.WebPartToEdit;
            if (webPartToEdit != null)
            {
                this.EnsureChildControls();
                try
                {
                    if (this.CanChangeChromeState)
                    {
                        TypeConverter converter = TypeDescriptor.GetConverter(typeof(PartChromeState));
                        webPartToEdit.ChromeState = (PartChromeState) converter.ConvertFromString(this._chromeState.SelectedValue);
                    }
                }
                catch (Exception exception)
                {
                    this._chromeStateErrorMessage = base.CreateErrorMessage(exception.Message);
                }
                int zoneIndex = webPartToEdit.ZoneIndex;
                if (this.CanChangeZoneIndex)
                {
                    if (int.TryParse(this._zoneIndex.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out zoneIndex))
                    {
                        if (zoneIndex < 0)
                        {
                            object[] args = new object[] { 0.ToString(CultureInfo.CurrentCulture) };
                            this._zoneIndexErrorMessage = System.Web.SR.GetString("EditorPart_PropertyMinValue", args);
                        }
                    }
                    else
                    {
                        this._zoneIndexErrorMessage = System.Web.SR.GetString("EditorPart_PropertyMustBeInteger");
                    }
                }
                WebPartZoneBase zone = webPartToEdit.Zone;
                WebPartZoneBase base3 = zone;
                if (this.CanChangeZone)
                {
                    base3 = base.WebPartManager.Zones[this._zone.SelectedValue];
                }
                if ((((this._zoneIndexErrorMessage == null) && zone.AllowLayoutChange) && base3.AllowLayoutChange) && ((webPartToEdit.Zone != base3) || (webPartToEdit.ZoneIndex != zoneIndex)))
                {
                    try
                    {
                        base.WebPartManager.MoveWebPart(webPartToEdit, base3, zoneIndex);
                    }
                    catch (Exception exception2)
                    {
                        this._zoneIndexErrorMessage = base.CreateErrorMessage(exception2.Message);
                    }
                }
            }
            return !this.HasError;
        }

        protected internal override void CreateChildControls()
        {
            ControlCollection controls = this.Controls;
            controls.Clear();
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(PartChromeState));
            this._chromeState = new DropDownList();
            this._chromeState.Items.Add(new ListItem(System.Web.SR.GetString("PartChromeState_Normal"), converter.ConvertToString(PartChromeState.Normal)));
            this._chromeState.Items.Add(new ListItem(System.Web.SR.GetString("PartChromeState_Minimized"), converter.ConvertToString(PartChromeState.Minimized)));
            controls.Add(this._chromeState);
            this._zone = new DropDownList();
            WebPartManager webPartManager = base.WebPartManager;
            if (webPartManager != null)
            {
                WebPartZoneCollection zones = webPartManager.Zones;
                if (zones != null)
                {
                    foreach (WebPartZoneBase base2 in zones)
                    {
                        ListItem item = new ListItem(base2.DisplayTitle, base2.ID);
                        this._zone.Items.Add(item);
                    }
                }
            }
            controls.Add(this._zone);
            this._zoneIndex = new TextBox();
            this._zoneIndex.Columns = 10;
            controls.Add(this._zoneIndex);
            foreach (Control control in controls)
            {
                control.EnableViewState = false;
            }
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if ((this.Display && this.Visible) && !this.HasError)
            {
                this.SyncChanges();
            }
        }

        protected internal override void RenderContents(HtmlTextWriter writer)
        {
            if (this.Page != null)
            {
                this.Page.VerifyRenderingInServerForm(this);
            }
            this.EnsureChildControls();
            if (base.DesignMode)
            {
                this._zone.Items.Add(System.Web.SR.GetString("Zone_SampleHeaderText"));
            }
            string[] propertyDisplayNames = new string[] { System.Web.SR.GetString("LayoutEditorPart_ChromeState"), System.Web.SR.GetString("LayoutEditorPart_Zone"), System.Web.SR.GetString("LayoutEditorPart_ZoneIndex") };
            WebControl[] propertyEditors = new WebControl[] { this._chromeState, this._zone, this._zoneIndex };
            string[] strArray4 = new string[3];
            strArray4[0] = this._chromeStateErrorMessage;
            strArray4[2] = this._zoneIndexErrorMessage;
            string[] errorMessages = strArray4;
            base.RenderPropertyEditors(writer, propertyDisplayNames, null, propertyEditors, errorMessages);
        }

        public override void SyncChanges()
        {
            WebPart webPartToEdit = base.WebPartToEdit;
            if (webPartToEdit != null)
            {
                WebPartZoneBase zone = webPartToEdit.Zone;
                bool allowLayoutChange = zone.AllowLayoutChange;
                this.EnsureChildControls();
                this._chromeState.SelectedValue = TypeDescriptor.GetConverter(typeof(PartChromeState)).ConvertToString(webPartToEdit.ChromeState);
                this._chromeState.Enabled = this.CanChangeChromeState;
                WebPartManager webPartManager = base.WebPartManager;
                if (webPartManager != null)
                {
                    WebPartZoneCollection zones = webPartManager.Zones;
                    bool allowZoneChange = webPartToEdit.AllowZoneChange;
                    this._zone.ClearSelection();
                    foreach (ListItem item in this._zone.Items)
                    {
                        string str = item.Value;
                        WebPartZoneBase base3 = zones[str];
                        if ((base3 == zone) || (allowZoneChange && base3.AllowLayoutChange))
                        {
                            item.Enabled = true;
                        }
                        else
                        {
                            item.Enabled = false;
                        }
                        if (base3 == zone)
                        {
                            item.Selected = true;
                        }
                    }
                    this._zone.Enabled = this.CanChangeZone;
                }
                this._zoneIndex.Text = webPartToEdit.ZoneIndex.ToString(CultureInfo.CurrentCulture);
                this._zoneIndex.Enabled = this.CanChangeZoneIndex;
            }
        }

        private bool CanChangeChromeState
        {
            get
            {
                WebPart webPartToEdit = base.WebPartToEdit;
                if (!webPartToEdit.Zone.AllowLayoutChange)
                {
                    return false;
                }
                if (!webPartToEdit.AllowMinimize)
                {
                    return (webPartToEdit.ChromeState == PartChromeState.Minimized);
                }
                return true;
            }
        }

        private bool CanChangeZone
        {
            get
            {
                WebPart webPartToEdit = base.WebPartToEdit;
                return (webPartToEdit.Zone.AllowLayoutChange && webPartToEdit.AllowZoneChange);
            }
        }

        private bool CanChangeZoneIndex
        {
            get
            {
                return base.WebPartToEdit.Zone.AllowLayoutChange;
            }
        }

        [Themeable(false), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override string DefaultButton
        {
            get
            {
                return base.DefaultButton;
            }
            set
            {
                base.DefaultButton = value;
            }
        }

        public override bool Display
        {
            get
            {
                return true;
            }
        }

        private bool HasError
        {
            get
            {
                if (this._chromeStateErrorMessage == null)
                {
                    return (this._zoneIndexErrorMessage != null);
                }
                return true;
            }
        }

        [WebSysDefaultValue("LayoutEditorPart_PartTitle")]
        public override string Title
        {
            get
            {
                string str = (string) this.ViewState["Title"];
                if (str == null)
                {
                    return System.Web.SR.GetString("LayoutEditorPart_PartTitle");
                }
                return str;
            }
            set
            {
                this.ViewState["Title"] = value;
            }
        }
    }
}

