namespace System.Diagnostics
{
    using System;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true), ComVisible(true)]
    public sealed class DebuggerVisualizerAttribute : Attribute
    {
        private string description;
        private Type target;
        private string targetName;
        private string visualizerName;
        private string visualizerObjectSourceName;

        public DebuggerVisualizerAttribute(string visualizerTypeName)
        {
            this.visualizerName = visualizerTypeName;
        }

        public DebuggerVisualizerAttribute(Type visualizer)
        {
            if (visualizer == null)
            {
                throw new ArgumentNullException("visualizer");
            }
            this.visualizerName = visualizer.AssemblyQualifiedName;
        }

        public DebuggerVisualizerAttribute(string visualizerTypeName, string visualizerObjectSourceTypeName)
        {
            this.visualizerName = visualizerTypeName;
            this.visualizerObjectSourceName = visualizerObjectSourceTypeName;
        }

        public DebuggerVisualizerAttribute(string visualizerTypeName, Type visualizerObjectSource)
        {
            if (visualizerObjectSource == null)
            {
                throw new ArgumentNullException("visualizerObjectSource");
            }
            this.visualizerName = visualizerTypeName;
            this.visualizerObjectSourceName = visualizerObjectSource.AssemblyQualifiedName;
        }

        public DebuggerVisualizerAttribute(Type visualizer, string visualizerObjectSourceTypeName)
        {
            if (visualizer == null)
            {
                throw new ArgumentNullException("visualizer");
            }
            this.visualizerName = visualizer.AssemblyQualifiedName;
            this.visualizerObjectSourceName = visualizerObjectSourceTypeName;
        }

        public DebuggerVisualizerAttribute(Type visualizer, Type visualizerObjectSource)
        {
            if (visualizer == null)
            {
                throw new ArgumentNullException("visualizer");
            }
            if (visualizerObjectSource == null)
            {
                throw new ArgumentNullException("visualizerObjectSource");
            }
            this.visualizerName = visualizer.AssemblyQualifiedName;
            this.visualizerObjectSourceName = visualizerObjectSource.AssemblyQualifiedName;
        }

        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
            }
        }

        public Type Target
        {
            get
            {
                return this.target;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.targetName = value.AssemblyQualifiedName;
                this.target = value;
            }
        }

        public string TargetTypeName
        {
            get
            {
                return this.targetName;
            }
            set
            {
                this.targetName = value;
            }
        }

        public string VisualizerObjectSourceTypeName
        {
            get
            {
                return this.visualizerObjectSourceName;
            }
        }

        public string VisualizerTypeName
        {
            get
            {
                return this.visualizerName;
            }
        }
    }
}

