namespace System.Management.Instrumentation
{
    using System;
    using System.Collections.Specialized;

    internal class ReferencesCollection
    {
        private StringCollection assemblies = new StringCollection();
        private StringCollection namespaces = new StringCollection();
        private CodeWriter usingCode = new CodeWriter();

        public void Add(Type type)
        {
            if (!this.namespaces.Contains(type.Namespace))
            {
                this.namespaces.Add(type.Namespace);
                this.usingCode.Line(string.Format("using {0};", type.Namespace));
            }
            if (!this.assemblies.Contains(type.Assembly.Location))
            {
                this.assemblies.Add(type.Assembly.Location);
            }
        }

        public StringCollection Assemblies
        {
            get
            {
                return this.assemblies;
            }
        }

        public StringCollection Namespaces
        {
            get
            {
                return this.namespaces;
            }
        }

        public CodeWriter UsingCode
        {
            get
            {
                return this.usingCode;
            }
        }
    }
}

