namespace System.Deployment.Application
{
    using System;
    using System.Threading;

    internal class ModalPiece : FormPiece
    {
        protected ManualResetEvent _modalEvent;
        protected UserInterfaceModalResult _modalResult;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this._modalEvent.Set();
        }

        public override bool OnClosing()
        {
            bool flag = base.OnClosing();
            this._modalEvent.Set();
            return flag;
        }

        public UserInterfaceModalResult ModalResult
        {
            get
            {
                return this._modalResult;
            }
        }
    }
}

