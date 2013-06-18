namespace System.Workflow.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Xml;

    internal sealed class CreationContext
    {
        internal Dictionary<string, object> Args;
        internal bool Created;
        internal string InvokeActivityID;
        internal WorkflowExecutor InvokerExecutor;
        internal bool IsActivation;
        internal XmlReader RulesReader;
        internal System.Type Type;
        internal XmlReader XomlReader;

        internal CreationContext(XmlReader xomlReader, XmlReader rulesReader, Dictionary<string, object> args)
        {
            this.XomlReader = xomlReader;
            this.RulesReader = rulesReader;
            this.InvokerExecutor = null;
            this.InvokeActivityID = null;
            this.Args = args;
            this.IsActivation = true;
        }

        internal CreationContext(System.Type type, WorkflowExecutor invokerExec, string invokeActivityID, Dictionary<string, object> args)
        {
            this.Type = type;
            this.InvokerExecutor = invokerExec;
            this.InvokeActivityID = invokeActivityID;
            this.Args = args;
            this.IsActivation = true;
        }
    }
}

