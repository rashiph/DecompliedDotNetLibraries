namespace System
{
    using System.Runtime.CompilerServices;

    [Serializable]
    public delegate void EventHandler<TEventArgs>(object sender, TEventArgs e) where TEventArgs: EventArgs;
}

