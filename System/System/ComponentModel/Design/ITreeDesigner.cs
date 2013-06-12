namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;

    public interface ITreeDesigner : IDesigner, IDisposable
    {
        ICollection Children { get; }

        IDesigner Parent { get; }
    }
}

