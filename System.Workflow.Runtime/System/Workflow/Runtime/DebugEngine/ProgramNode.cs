namespace System.Workflow.Runtime.DebugEngine
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;

    internal sealed class ProgramNode : IWDEProgramNode
    {
        private DebugController controller;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ProgramNode(DebugController controller)
        {
            this.controller = controller;
        }

        void IWDEProgramNode.Attach(ref Guid programId, int attachTimeout, int detachPingInterval, out string hostName, out string uri, out int controllerThreadId, out bool isSynchronousAttach)
        {
            this.controller.Attach(programId, attachTimeout, detachPingInterval, out hostName, out uri, out controllerThreadId, out isSynchronousAttach);
        }
    }
}

