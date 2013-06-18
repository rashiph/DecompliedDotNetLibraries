namespace System.Web.Services.Description
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.Web.Services;
    using System.Xml.Serialization;

    public sealed class MimeTextMatch
    {
        private int capture;
        private int group = 1;
        private bool ignoreCase;
        private MimeTextMatchCollection matches = new MimeTextMatchCollection();
        private string name;
        private string pattern;
        private int repeats = 1;
        private string type;

        [XmlAttribute("capture"), DefaultValue(0)]
        public int Capture
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.capture;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(Res.GetString("WebNegativeValue", new object[] { "capture" }));
                }
                this.capture = value;
            }
        }

        [XmlAttribute("group"), DefaultValue(1)]
        public int Group
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.group;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(Res.GetString("WebNegativeValue", new object[] { "group" }));
                }
                this.group = value;
            }
        }

        [XmlAttribute("ignoreCase")]
        public bool IgnoreCase
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.ignoreCase;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.ignoreCase = value;
            }
        }

        [XmlElement("match")]
        public MimeTextMatchCollection Matches
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.matches;
            }
        }

        [XmlAttribute("name")]
        public string Name
        {
            get
            {
                if (this.name != null)
                {
                    return this.name;
                }
                return string.Empty;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.name = value;
            }
        }

        [XmlAttribute("pattern")]
        public string Pattern
        {
            get
            {
                if (this.pattern != null)
                {
                    return this.pattern;
                }
                return string.Empty;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.pattern = value;
            }
        }

        [XmlIgnore]
        public int Repeats
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.repeats;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(Res.GetString("WebNegativeValue", new object[] { "repeats" }));
                }
                this.repeats = value;
            }
        }

        [XmlAttribute("repeats"), DefaultValue("1")]
        public string RepeatsString
        {
            get
            {
                if (this.repeats != 0x7fffffff)
                {
                    return this.repeats.ToString(CultureInfo.InvariantCulture);
                }
                return "*";
            }
            set
            {
                if (value == "*")
                {
                    this.repeats = 0x7fffffff;
                }
                else
                {
                    this.Repeats = int.Parse(value, CultureInfo.InvariantCulture);
                }
            }
        }

        [XmlAttribute("type")]
        public string Type
        {
            get
            {
                if (this.type != null)
                {
                    return this.type;
                }
                return string.Empty;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.type = value;
            }
        }
    }
}

