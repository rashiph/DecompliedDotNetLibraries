namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using System;

    public sealed class Message : TaskExtension
    {
        private string importance;
        private string text;

        public override bool Execute()
        {
            MessageImportance normal;
            if ((this.Importance == null) || (this.Importance.Length == 0))
            {
                normal = MessageImportance.Normal;
            }
            else
            {
                try
                {
                    normal = (MessageImportance) Enum.Parse(typeof(MessageImportance), this.Importance, true);
                }
                catch (ArgumentException)
                {
                    base.Log.LogErrorWithCodeFromResources("Message.InvalidImportance", new object[] { this.Importance });
                    return false;
                }
            }
            if (this.Text != null)
            {
                base.Log.LogMessage(normal, "{0}", new object[] { this.Text });
            }
            return true;
        }

        public string Importance
        {
            get
            {
                return this.importance;
            }
            set
            {
                this.importance = value;
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

