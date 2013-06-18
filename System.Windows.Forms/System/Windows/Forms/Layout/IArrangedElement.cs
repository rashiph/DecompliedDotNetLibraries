namespace System.Windows.Forms.Layout
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    internal interface IArrangedElement : IComponent, IDisposable
    {
        Size GetPreferredSize(Size proposedSize);
        void PerformLayout(IArrangedElement affectedElement, string propertyName);
        void SetBounds(Rectangle bounds, BoundsSpecified specified);

        Rectangle Bounds { get; }

        ArrangedElementCollection Children { get; }

        IArrangedElement Container { get; }

        Rectangle DisplayRectangle { get; }

        bool ParticipatesInLayout { get; }

        PropertyStore Properties { get; }
    }
}

