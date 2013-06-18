namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    public sealed class OutputMessage
    {
        private readonly string[] arguments;
        private readonly string name;
        private readonly string text;
        private readonly OutputMessageType type;

        internal OutputMessage(OutputMessageType type, string name, string text, params string[] arguments)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }
            this.type = type;
            this.name = name;
            this.arguments = arguments;
            this.text = text;
        }

        public string[] GetArguments()
        {
            return this.arguments;
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public string Text
        {
            get
            {
                return this.text;
            }
        }

        public OutputMessageType Type
        {
            get
            {
                return this.type;
            }
        }
    }
}

