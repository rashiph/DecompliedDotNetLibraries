namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using System;

    public class CreateProperty : TaskExtension
    {
        private string[] prop;

        public override bool Execute()
        {
            if (this.prop == null)
            {
                this.prop = new string[0];
            }
            return true;
        }

        [Output]
        public string[] Value
        {
            get
            {
                return this.prop;
            }
            set
            {
                this.prop = value;
            }
        }

        [Output]
        public string[] ValueSetByTask
        {
            get
            {
                return this.prop;
            }
        }
    }
}

