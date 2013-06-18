namespace System.Activities.Debugger
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    [DebuggerNonUserCode]
    public class LocalsItemDescription
    {
        public LocalsItemDescription(string name, System.Type type)
        {
            this.Name = name;
            this.Type = type;
        }

        public override string ToString()
        {
            return (this.Name + ":" + this.Type.ToString());
        }

        public string Name { get; private set; }

        public System.Type Type { get; private set; }
    }
}

