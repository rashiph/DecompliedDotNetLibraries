namespace System.Drawing
{
    using System;
    using System.Drawing.Drawing2D;

    internal class GraphicsContext : IDisposable
    {
        private Region clipRegion;
        private int contextState;
        private bool isCumulative;
        private GraphicsContext nextContext;
        private GraphicsContext prevContext;
        private PointF transformOffset;

        private GraphicsContext()
        {
        }

        public GraphicsContext(Graphics g)
        {
            Matrix transform = g.Transform;
            if (!transform.IsIdentity)
            {
                float[] elements = transform.Elements;
                this.transformOffset.X = elements[4];
                this.transformOffset.Y = elements[5];
            }
            transform.Dispose();
            Region clip = g.Clip;
            if (clip.IsInfinite(g))
            {
                clip.Dispose();
            }
            else
            {
                this.clipRegion = clip;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (this.nextContext != null)
            {
                this.nextContext.Dispose();
                this.nextContext = null;
            }
            if (this.clipRegion != null)
            {
                this.clipRegion.Dispose();
                this.clipRegion = null;
            }
        }

        public Region Clip
        {
            get
            {
                return this.clipRegion;
            }
        }

        public bool IsCumulative
        {
            get
            {
                return this.isCumulative;
            }
            set
            {
                this.isCumulative = value;
            }
        }

        public GraphicsContext Next
        {
            get
            {
                return this.nextContext;
            }
            set
            {
                this.nextContext = value;
            }
        }

        public GraphicsContext Previous
        {
            get
            {
                return this.prevContext;
            }
            set
            {
                this.prevContext = value;
            }
        }

        public int State
        {
            get
            {
                return this.contextState;
            }
            set
            {
                this.contextState = value;
            }
        }

        public PointF TransformOffset
        {
            get
            {
                return this.transformOffset;
            }
        }
    }
}

