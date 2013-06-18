namespace Microsoft.Build.Tasks.Xaml
{
    using System;

    internal class Argument
    {
        private string parameter = string.Empty;
        private bool required;
        private string separator = string.Empty;

        public string Parameter
        {
            get
            {
                return this.parameter;
            }
            set
            {
                this.parameter = value;
            }
        }

        public bool Required
        {
            get
            {
                return this.required;
            }
            set
            {
                this.required = value;
            }
        }

        public string Separator
        {
            get
            {
                return this.separator;
            }
            set
            {
                this.separator = value;
            }
        }
    }
}

