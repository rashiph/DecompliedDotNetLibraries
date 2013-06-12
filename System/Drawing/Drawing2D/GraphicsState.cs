namespace System.Drawing.Drawing2D
{
    using System;
    using System.Runtime;

    public sealed class GraphicsState : MarshalByRefObject
    {
        internal int nativeState;

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal GraphicsState(int nativeState)
        {
            this.nativeState = nativeState;
        }
    }
}

