namespace System.Deployment.Application
{
    using System;
    using System.Windows.Forms;

    internal class FormPiece : Panel
    {
        public virtual bool OnClosing()
        {
            return true;
        }
    }
}

