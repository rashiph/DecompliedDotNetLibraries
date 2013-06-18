namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;

    public sealed class Namespace
    {
        internal VsaEngine engine;
        private string name;

        private Namespace(string name, VsaEngine engine)
        {
            this.name = name;
            this.engine = engine;
        }

        public static Namespace GetNamespace(string name, VsaEngine engine)
        {
            return new Namespace(name, engine);
        }

        internal Type GetType(string typeName)
        {
            return this.engine.GetType(typeName);
        }

        internal string Name
        {
            get
            {
                return this.name;
            }
        }
    }
}

