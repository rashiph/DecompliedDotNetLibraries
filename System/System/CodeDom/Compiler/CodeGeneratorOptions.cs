namespace System.CodeDom.Compiler
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public class CodeGeneratorOptions
    {
        private IDictionary options = new ListDictionary();

        public bool BlankLinesBetweenMembers
        {
            get
            {
                object obj2 = this.options["BlankLinesBetweenMembers"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                this.options["BlankLinesBetweenMembers"] = value;
            }
        }

        public string BracingStyle
        {
            get
            {
                object obj2 = this.options["BracingStyle"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return "Block";
            }
            set
            {
                this.options["BracingStyle"] = value;
            }
        }

        public bool ElseOnClosing
        {
            get
            {
                object obj2 = this.options["ElseOnClosing"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.options["ElseOnClosing"] = value;
            }
        }

        public string IndentString
        {
            get
            {
                object obj2 = this.options["IndentString"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return "    ";
            }
            set
            {
                this.options["IndentString"] = value;
            }
        }

        public object this[string index]
        {
            get
            {
                return this.options[index];
            }
            set
            {
                this.options[index] = value;
            }
        }

        [ComVisible(false)]
        public bool VerbatimOrder
        {
            get
            {
                object obj2 = this.options["VerbatimOrder"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.options["VerbatimOrder"] = value;
            }
        }
    }
}

