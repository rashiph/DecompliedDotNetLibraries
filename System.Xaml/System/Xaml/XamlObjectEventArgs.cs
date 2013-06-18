namespace System.Xaml
{
    using System;
    using System.Runtime.CompilerServices;

    public class XamlObjectEventArgs : EventArgs
    {
        public XamlObjectEventArgs(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            this.Instance = instance;
        }

        public object Instance { get; private set; }
    }
}

