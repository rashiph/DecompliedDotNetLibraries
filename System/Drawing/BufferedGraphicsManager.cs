namespace System.Drawing
{
    using System;
    using System.Runtime.ConstrainedExecution;

    public sealed class BufferedGraphicsManager
    {
        private static BufferedGraphicsContext bufferedGraphicsContext;

        static BufferedGraphicsManager()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(BufferedGraphicsManager.OnShutdown);
            AppDomain.CurrentDomain.DomainUnload += new EventHandler(BufferedGraphicsManager.OnShutdown);
            bufferedGraphicsContext = new BufferedGraphicsContext();
        }

        private BufferedGraphicsManager()
        {
        }

        [PrePrepareMethod]
        private static void OnShutdown(object sender, EventArgs e)
        {
            Current.Invalidate();
        }

        public static BufferedGraphicsContext Current
        {
            get
            {
                return bufferedGraphicsContext;
            }
        }
    }
}

