namespace System.Windows.Forms.Layout
{
    using System;
    using System.Windows.Forms;

    internal sealed class LayoutTransaction : IDisposable
    {
        private Control _controlToLayout;
        private bool _resumeLayout;

        public LayoutTransaction(Control controlToLayout, IArrangedElement controlCausingLayout, string property) : this(controlToLayout, controlCausingLayout, property, true)
        {
        }

        public LayoutTransaction(Control controlToLayout, IArrangedElement controlCausingLayout, string property, bool resumeLayout)
        {
            CommonProperties.xClearPreferredSizeCache(controlCausingLayout);
            this._controlToLayout = controlToLayout;
            this._resumeLayout = resumeLayout;
            if (this._controlToLayout != null)
            {
                this._controlToLayout.SuspendLayout();
                CommonProperties.xClearPreferredSizeCache(this._controlToLayout);
                if (resumeLayout)
                {
                    this._controlToLayout.PerformLayout(new LayoutEventArgs(controlCausingLayout, property));
                }
            }
        }

        public static IDisposable CreateTransactionIf(bool condition, Control controlToLayout, IArrangedElement elementCausingLayout, string property)
        {
            if (condition)
            {
                return new LayoutTransaction(controlToLayout, elementCausingLayout, property);
            }
            CommonProperties.xClearPreferredSizeCache(elementCausingLayout);
            return new NullLayoutTransaction();
        }

        public void Dispose()
        {
            if (this._controlToLayout != null)
            {
                this._controlToLayout.ResumeLayout(this._resumeLayout);
            }
        }

        public static void DoLayout(IArrangedElement elementToLayout, IArrangedElement elementCausingLayout, string property)
        {
            if (elementCausingLayout != null)
            {
                CommonProperties.xClearPreferredSizeCache(elementCausingLayout);
                if (elementToLayout != null)
                {
                    CommonProperties.xClearPreferredSizeCache(elementToLayout);
                    elementToLayout.PerformLayout(elementCausingLayout, property);
                }
            }
        }

        public static void DoLayoutIf(bool condition, IArrangedElement elementToLayout, IArrangedElement elementCausingLayout, string property)
        {
            if (!condition)
            {
                if (elementCausingLayout != null)
                {
                    CommonProperties.xClearPreferredSizeCache(elementCausingLayout);
                }
            }
            else
            {
                DoLayout(elementToLayout, elementCausingLayout, property);
            }
        }
    }
}

