namespace System.Windows.Forms
{
    using System;
    using System.Configuration;
    using System.Drawing;

    internal class ToolStripSettings : ApplicationSettingsBase
    {
        internal ToolStripSettings(string settingsKey) : base(settingsKey)
        {
        }

        public override void Save()
        {
            this.IsDefault = false;
            base.Save();
        }

        [UserScopedSetting, DefaultSettingValue("true")]
        public bool IsDefault
        {
            get
            {
                return (bool) this["IsDefault"];
            }
            set
            {
                this["IsDefault"] = value;
            }
        }

        [UserScopedSetting]
        public string ItemOrder
        {
            get
            {
                return (this["ItemOrder"] as string);
            }
            set
            {
                this["ItemOrder"] = value;
            }
        }

        [UserScopedSetting, DefaultSettingValue("0,0")]
        public Point Location
        {
            get
            {
                return (Point) this["Location"];
            }
            set
            {
                this["Location"] = value;
            }
        }

        [UserScopedSetting]
        public string Name
        {
            get
            {
                return (this["Name"] as string);
            }
            set
            {
                this["Name"] = value;
            }
        }

        [DefaultSettingValue("0,0"), UserScopedSetting]
        public System.Drawing.Size Size
        {
            get
            {
                return (System.Drawing.Size) this["Size"];
            }
            set
            {
                this["Size"] = value;
            }
        }

        [UserScopedSetting]
        public string ToolStripPanelName
        {
            get
            {
                return (this["ToolStripPanelName"] as string);
            }
            set
            {
                this["ToolStripPanelName"] = value;
            }
        }

        [UserScopedSetting, DefaultSettingValue("true")]
        public bool Visible
        {
            get
            {
                return (bool) this["Visible"];
            }
            set
            {
                this["Visible"] = value;
            }
        }
    }
}

