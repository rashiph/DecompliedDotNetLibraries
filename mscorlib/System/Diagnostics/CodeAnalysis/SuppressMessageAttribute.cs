namespace System.Diagnostics.CodeAnalysis
{
    using System;
    using System.Diagnostics;

    [Conditional("CODE_ANALYSIS"), AttributeUsage(AttributeTargets.All, Inherited=false, AllowMultiple=true)]
    public sealed class SuppressMessageAttribute : Attribute
    {
        private string category;
        private string checkId;
        private string justification;
        private string messageId;
        private string scope;
        private string target;

        public SuppressMessageAttribute(string category, string checkId)
        {
            this.category = category;
            this.checkId = checkId;
        }

        public string Category
        {
            get
            {
                return this.category;
            }
        }

        public string CheckId
        {
            get
            {
                return this.checkId;
            }
        }

        public string Justification
        {
            get
            {
                return this.justification;
            }
            set
            {
                this.justification = value;
            }
        }

        public string MessageId
        {
            get
            {
                return this.messageId;
            }
            set
            {
                this.messageId = value;
            }
        }

        public string Scope
        {
            get
            {
                return this.scope;
            }
            set
            {
                this.scope = value;
            }
        }

        public string Target
        {
            get
            {
                return this.target;
            }
            set
            {
                this.target = value;
            }
        }
    }
}

