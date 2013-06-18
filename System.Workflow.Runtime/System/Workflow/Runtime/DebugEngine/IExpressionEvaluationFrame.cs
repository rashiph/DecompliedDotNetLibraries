namespace System.Workflow.Runtime.DebugEngine
{
    using System;

    public interface IExpressionEvaluationFrame
    {
        void CreateEvaluationFrame(IInstanceTable instanceTable, DebugEngineCallback callback);
    }
}

