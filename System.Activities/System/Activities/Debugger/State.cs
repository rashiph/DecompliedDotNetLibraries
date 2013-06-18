namespace System.Activities.Debugger
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;

    [DebuggerNonUserCode]
    public class State
    {
        private IEnumerable<LocalsItemDescription> earlyLocals;
        private SourceLocation location;
        private string methodName;
        private string name;
        private int numberOfEarlyLocals;
        private Type type;

        internal State(SourceLocation location, string name, IEnumerable<LocalsItemDescription> earlyLocals, int numberOfEarlyLocals)
        {
            this.location = location;
            this.name = name;
            this.earlyLocals = earlyLocals;
            this.numberOfEarlyLocals = (earlyLocals == null) ? 0 : numberOfEarlyLocals;
        }

        internal void CacheMethodInfo(Type type, string methodName)
        {
            this.type = type;
            this.methodName = methodName;
        }

        internal MethodInfo GetMethodInfo(bool withPriming)
        {
            return this.type.GetMethod(withPriming ? ("_" + this.methodName) : this.methodName);
        }

        internal IEnumerable<LocalsItemDescription> EarlyLocals
        {
            get
            {
                return this.earlyLocals;
            }
        }

        internal SourceLocation Location
        {
            get
            {
                return this.location;
            }
        }

        internal string Name
        {
            get
            {
                return this.name;
            }
        }

        internal int NumberOfEarlyLocals
        {
            get
            {
                return this.numberOfEarlyLocals;
            }
        }
    }
}

