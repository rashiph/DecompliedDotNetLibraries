namespace System.Windows.Forms.Design
{
    using System;
    using System.Windows.Forms;

    internal interface IEventHandlerService
    {
        event EventHandler EventHandlerChanged;

        object GetHandler(System.Type handlerType);
        void PopHandler(object handler);
        void PushHandler(object handler);

        Control FocusWindow { get; }
    }
}

