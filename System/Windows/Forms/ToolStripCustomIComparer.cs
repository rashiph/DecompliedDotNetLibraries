namespace System.Windows.Forms
{
    using System;
    using System.Collections;

    internal class ToolStripCustomIComparer : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            if (x.GetType() != y.GetType())
            {
                if (x.GetType().IsAssignableFrom(y.GetType()))
                {
                    return 1;
                }
                if (y.GetType().IsAssignableFrom(x.GetType()))
                {
                    return -1;
                }
            }
            return 0;
        }
    }
}

