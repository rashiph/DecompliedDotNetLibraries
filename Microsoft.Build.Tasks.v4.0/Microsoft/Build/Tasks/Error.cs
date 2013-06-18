namespace Microsoft.Build.Tasks
{
    using System;

    public sealed class Error : TaskExtension
    {
        private string code;
        private string file;
        private string helpKeyword;
        private string text;

        public override bool Execute()
        {
            if ((this.Text != null) || (this.Code != null))
            {
                base.Log.LogError(null, this.Code, this.HelpKeyword, this.File, 0, 0, 0, 0, (this.Text == null) ? string.Empty : this.Text, new object[0]);
            }
            return false;
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

        public string File
        {
            get
            {
                return this.file;
            }
            set
            {
                this.file = value;
            }
        }

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

