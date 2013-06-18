namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IVbHost
    {
        IWin32Window GetParentWindow();
        string GetWindowTitle();
    }
}

