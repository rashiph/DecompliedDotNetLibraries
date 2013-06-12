namespace System.Drawing.Drawing2D
{
    using System;

    public sealed class GraphicsContainer : MarshalByRefObject
    {
        internal int nativeGraphicsContainer;

        internal GraphicsContainer(int graphicsContainer)
        {
            this.nativeGraphicsContainer = graphicsContainer;
        }
    }
}

