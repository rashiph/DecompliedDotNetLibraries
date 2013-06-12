namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Windows.Forms.VisualStyles;

    public sealed class ProgressBarRenderer
    {
        [ThreadStatic]
        private static VisualStyleRenderer visualStyleRenderer;

        private ProgressBarRenderer()
        {
        }

        public static void DrawHorizontalBar(Graphics g, Rectangle bounds)
        {
            InitializeRenderer(VisualStyleElement.ProgressBar.Bar.Normal);
            visualStyleRenderer.DrawBackground(g, bounds);
        }

        public static void DrawHorizontalChunks(Graphics g, Rectangle bounds)
        {
            InitializeRenderer(VisualStyleElement.ProgressBar.Chunk.Normal);
            visualStyleRenderer.DrawBackground(g, bounds);
        }

        public static void DrawVerticalBar(Graphics g, Rectangle bounds)
        {
            InitializeRenderer(VisualStyleElement.ProgressBar.BarVertical.Normal);
            visualStyleRenderer.DrawBackground(g, bounds);
        }

        public static void DrawVerticalChunks(Graphics g, Rectangle bounds)
        {
            InitializeRenderer(VisualStyleElement.ProgressBar.ChunkVertical.Normal);
            visualStyleRenderer.DrawBackground(g, bounds);
        }

        private static void InitializeRenderer(VisualStyleElement element)
        {
            if (visualStyleRenderer == null)
            {
                visualStyleRenderer = new VisualStyleRenderer(element);
            }
            else
            {
                visualStyleRenderer.SetParameters(element);
            }
        }

        public static int ChunkSpaceThickness
        {
            get
            {
                InitializeRenderer(VisualStyleElement.ProgressBar.Chunk.Normal);
                return visualStyleRenderer.GetInteger(IntegerProperty.ProgressSpaceSize);
            }
        }

        public static int ChunkThickness
        {
            get
            {
                InitializeRenderer(VisualStyleElement.ProgressBar.Chunk.Normal);
                return visualStyleRenderer.GetInteger(IntegerProperty.ProgressChunkSize);
            }
        }

        public static bool IsSupported
        {
            get
            {
                return VisualStyleRenderer.IsSupported;
            }
        }
    }
}

