namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Globalization;
    using System.Runtime;
    using System.Text.RegularExpressions;
    using System.Xml.Serialization;

    [XmlType("authorizedType")]
    public sealed class AuthorizedType
    {
        private string assemblyName;
        private bool isAuthorized;
        private string namespaceName;
        private Regex regex;
        private string typeName;

        private static string MakeRegex(string inputString)
        {
            return inputString.Replace(@"\", @"\\").Replace("[", @"\[").Replace("^", @"\^").Replace("$", @"\$").Replace("|", @"\|").Replace("+", @"\+").Replace("(", @"\(").Replace(")", @"\)").Replace(".", @"\x2E").Replace("*", @"[\w\x60\x2E]{0,}").Replace("?", @"\w{1,1}").Replace(" ", @"\s{0,}");
        }

        [XmlAttribute]
        public string Assembly
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.assemblyName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.assemblyName = value;
            }
        }

        [XmlAttribute]
        public string Authorized
        {
            get
            {
                return this.isAuthorized.ToString();
            }
            set
            {
                this.isAuthorized = bool.Parse(value);
            }
        }

        [XmlAttribute]
        public string Namespace
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.namespaceName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.namespaceName = value;
            }
        }

        [XmlIgnore]
        public Regex RegularExpression
        {
            get
            {
                if (this.regex == null)
                {
                    this.regex = new Regex(MakeRegex(string.Format(CultureInfo.InvariantCulture, "{0}.{1}, {2}", new object[] { this.namespaceName, this.typeName, this.assemblyName })), RegexOptions.Compiled);
                    return this.regex;
                }
                return this.regex;
            }
        }

        [XmlAttribute]
        public string TypeName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.typeName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.typeName = value;
            }
        }
    }
}

