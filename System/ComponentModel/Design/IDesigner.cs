namespace System.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface IDesigner : IDisposable
    {
        void DoDefaultAction();
        void Initialize(IComponent component);

        IComponent Component { get; }

        DesignerVerbCollection Verbs { get; }
    }
}

