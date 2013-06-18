namespace System.Windows.Forms.Design
{
    using System;
    using System.Windows.Forms;

    public sealed class EventHandlerService : IEventHandlerService
    {
        private readonly Control focusWnd;
        private HandlerEntry handlerHead;
        private object lastHandler;
        private System.Type lastHandlerType;

        public event EventHandler EventHandlerChanged;

        public EventHandlerService(Control focusWnd)
        {
            this.focusWnd = focusWnd;
        }

        public object GetHandler(System.Type handlerType)
        {
            if (handlerType == this.lastHandlerType)
            {
                return this.lastHandler;
            }
            for (HandlerEntry entry = this.handlerHead; entry != null; entry = entry.next)
            {
                if ((entry.handler != null) && handlerType.IsInstanceOfType(entry.handler))
                {
                    this.lastHandlerType = handlerType;
                    this.lastHandler = entry.handler;
                    return entry.handler;
                }
            }
            return null;
        }

        private void OnEventHandlerChanged(EventArgs e)
        {
            if (this.changedEvent != null)
            {
                this.changedEvent(this, e);
            }
        }

        public void PopHandler(object handler)
        {
            for (HandlerEntry entry = this.handlerHead; entry != null; entry = entry.next)
            {
                if (entry.handler == handler)
                {
                    this.handlerHead = entry.next;
                    this.lastHandler = null;
                    this.lastHandlerType = null;
                    this.OnEventHandlerChanged(EventArgs.Empty);
                    return;
                }
            }
        }

        public void PushHandler(object handler)
        {
            this.handlerHead = new HandlerEntry(handler, this.handlerHead);
            this.lastHandlerType = handler.GetType();
            this.lastHandler = this.handlerHead.handler;
            this.OnEventHandlerChanged(EventArgs.Empty);
        }

        public Control FocusWindow
        {
            get
            {
                return this.focusWnd;
            }
        }

        private sealed class HandlerEntry
        {
            public object handler;
            public EventHandlerService.HandlerEntry next;

            public HandlerEntry(object handler, EventHandlerService.HandlerEntry next)
            {
                this.handler = handler;
                this.next = next;
            }
        }
    }
}

