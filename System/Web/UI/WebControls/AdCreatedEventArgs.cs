namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Web;

    public class AdCreatedEventArgs : EventArgs
    {
        private IDictionary adProperties;
        private string alternateText;
        internal const string AlternateTextElement = "AlternateText";
        private bool hasHeight;
        private bool hasWidth;
        private Unit height;
        private const string HeightElement = "Height";
        private string imageUrl;
        internal const string ImageUrlElement = "ImageUrl";
        private string navigateUrl;
        internal const string NavigateUrlElement = "NavigateUrl";
        private Unit width;
        private const string WidthElement = "Width";

        public AdCreatedEventArgs(IDictionary adProperties) : this(adProperties, null, null, null)
        {
        }

        internal AdCreatedEventArgs(IDictionary adProperties, string imageUrlField, string navigateUrlField, string alternateTextField)
        {
            this.imageUrl = string.Empty;
            this.navigateUrl = string.Empty;
            this.alternateText = string.Empty;
            if (adProperties != null)
            {
                this.adProperties = adProperties;
                this.imageUrl = this.GetAdProperty("ImageUrl", imageUrlField);
                this.navigateUrl = this.GetAdProperty("NavigateUrl", navigateUrlField);
                this.alternateText = this.GetAdProperty("AlternateText", alternateTextField);
                this.hasWidth = this.GetUnitValue(adProperties, "Width", ref this.width);
                this.hasHeight = this.GetUnitValue(adProperties, "Height", ref this.height);
            }
        }

        private string GetAdProperty(string defaultIndex, string keyIndex)
        {
            string str = string.IsNullOrEmpty(keyIndex) ? defaultIndex : keyIndex;
            string str2 = (this.adProperties == null) ? null : ((string) this.adProperties[str]);
            if (str2 != null)
            {
                return str2;
            }
            return string.Empty;
        }

        private bool GetUnitValue(IDictionary properties, string keyIndex, ref Unit unitValue)
        {
            string str = properties[keyIndex] as string;
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            try
            {
                unitValue = Unit.Parse(str, CultureInfo.InvariantCulture);
            }
            catch
            {
                throw new FormatException(System.Web.SR.GetString("AdRotator_invalid_integer_format", new object[] { str, keyIndex, typeof(Unit).FullName }));
            }
            return true;
        }

        public IDictionary AdProperties
        {
            get
            {
                return this.adProperties;
            }
        }

        public string AlternateText
        {
            get
            {
                return this.alternateText;
            }
            set
            {
                this.alternateText = value;
            }
        }

        internal bool HasHeight
        {
            get
            {
                return this.hasHeight;
            }
        }

        internal bool HasWidth
        {
            get
            {
                return this.hasWidth;
            }
        }

        internal Unit Height
        {
            get
            {
                return this.height;
            }
        }

        public string ImageUrl
        {
            get
            {
                return this.imageUrl;
            }
            set
            {
                this.imageUrl = value;
            }
        }

        public string NavigateUrl
        {
            get
            {
                return this.navigateUrl;
            }
            set
            {
                this.navigateUrl = value;
            }
        }

        internal Unit Width
        {
            get
            {
                return this.width;
            }
        }
    }
}

