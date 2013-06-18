namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using System;
    using System.Globalization;

    public sealed class FormatVersion : TaskExtension
    {
        private _FormatType formatType;
        private string outputVersion;
        private int revision;
        private string specifiedFormatType;
        private string version;

        public override bool Execute()
        {
            if (!this.ValidateInputs())
            {
                return false;
            }
            if (string.IsNullOrEmpty(this.Version))
            {
                this.OutputVersion = "1.0.0.0";
            }
            else if (this.Version.EndsWith("*", StringComparison.Ordinal))
            {
                this.OutputVersion = this.Version.Substring(0, this.Version.Length - 1) + this.Revision.ToString("G", CultureInfo.InvariantCulture);
            }
            else
            {
                this.OutputVersion = this.Version;
            }
            if (this.formatType == _FormatType.Path)
            {
                this.OutputVersion = this.OutputVersion.Replace('.', '_');
            }
            return true;
        }

        private bool ValidateInputs()
        {
            if (this.specifiedFormatType != null)
            {
                try
                {
                    this.formatType = (_FormatType) Enum.Parse(typeof(_FormatType), this.specifiedFormatType, true);
                }
                catch (ArgumentException)
                {
                    base.Log.LogErrorWithCodeFromResources("General.InvalidValue", new object[] { "FormatType", "FormatVersion" });
                    return false;
                }
            }
            return true;
        }

        public string FormatType
        {
            get
            {
                return this.specifiedFormatType;
            }
            set
            {
                this.specifiedFormatType = value;
            }
        }

        [Output]
        public string OutputVersion
        {
            get
            {
                return this.outputVersion;
            }
            set
            {
                this.outputVersion = value;
            }
        }

        public int Revision
        {
            get
            {
                return this.revision;
            }
            set
            {
                this.revision = value;
            }
        }

        public string Version
        {
            get
            {
                return this.version;
            }
            set
            {
                this.version = value;
            }
        }

        private enum _FormatType
        {
            Version,
            Path
        }
    }
}

