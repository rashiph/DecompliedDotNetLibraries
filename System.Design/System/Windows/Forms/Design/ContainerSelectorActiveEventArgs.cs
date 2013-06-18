namespace System.Windows.Forms.Design
{
    using System;

    internal class ContainerSelectorActiveEventArgs : EventArgs
    {
        private readonly object component;
        private readonly ContainerSelectorActiveEventArgsType eventType;

        public ContainerSelectorActiveEventArgs(object component) : this(component, ContainerSelectorActiveEventArgsType.Mouse)
        {
        }

        public ContainerSelectorActiveEventArgs(object component, ContainerSelectorActiveEventArgsType eventType)
        {
            this.component = component;
            this.eventType = eventType;
        }
    }
}

