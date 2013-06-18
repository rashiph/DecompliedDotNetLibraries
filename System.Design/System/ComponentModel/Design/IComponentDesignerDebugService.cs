namespace System.ComponentModel.Design
{
    using System;
    using System.Diagnostics;

    public interface IComponentDesignerDebugService
    {
        void Assert(bool condition, string message);
        void Fail(string message);
        void Trace(string message, string category);

        int IndentLevel { get; set; }

        TraceListenerCollection Listeners { get; }
    }
}

