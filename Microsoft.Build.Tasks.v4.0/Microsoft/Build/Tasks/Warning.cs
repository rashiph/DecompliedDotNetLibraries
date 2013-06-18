namespace Microsoft.Build.Tasks
{
    using System;
    using System.Runtime.CompilerServices;

    public sealed class Warning : TaskExtension
    {
        private string code;
        private string helpKeyword;
        private string text;

        public override bool Execute()
        {
            if ((this.Text != null) || (this.Code != null))
            {
                base.Log.LogWarning(null, this.Code, this.HelpKeyword, this.File, 0, 0, 0, 0, (this.Text == null) ? string.Empty : this.Text, new object[0]);
            }
            return true;
        }

        public string Code
        {
            get
            {
                return this.code;
            }
            set
            {
                this.code = value;
            }
        }

        public string File { get; set; }

        public string HelpKeyword
        {
            get
            {
                return this.helpKeyword;
            }
            set
            {
                this.helpKeyword = value;
            }
        }

        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                this.text = value;
            }
        }
    }
}

