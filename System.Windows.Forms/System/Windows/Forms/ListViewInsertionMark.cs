namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    public sealed class ListViewInsertionMark
    {
        private bool appearsAfterItem;
        private System.Drawing.Color color = System.Drawing.Color.Empty;
        private int index;
        private ListView listView;

        internal ListViewInsertionMark(ListView listView)
        {
            this.listView = listView;
        }

        public int NearestIndex(Point pt)
        {
            System.Windows.Forms.NativeMethods.POINT wParam = new System.Windows.Forms.NativeMethods.POINT {
                x = pt.X,
                y = pt.Y
            };
            System.Windows.Forms.NativeMethods.LVINSERTMARK lParam = new System.Windows.Forms.NativeMethods.LVINSERTMARK();
            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.listView, this.listView.Handle), 0x10a8, wParam, lParam);
            return lParam.iItem;
        }

        internal void UpdateListView()
        {
            System.Windows.Forms.NativeMethods.LVINSERTMARK lParam = new System.Windows.Forms.NativeMethods.LVINSERTMARK {
                dwFlags = this.appearsAfterItem ? 1 : 0,
                iItem = this.index
            };
            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.listView, this.listView.Handle), 0x10a6, 0, lParam);
            if (!this.color.IsEmpty)
            {
                this.listView.SendMessage(0x10aa, 0, System.Windows.Forms.SafeNativeMethods.ColorToCOLORREF(this.color));
            }
        }

        public bool AppearsAfterItem
        {
            get
            {
                return this.appearsAfterItem;
            }
            set
            {
                if (this.appearsAfterItem != value)
                {
                    this.appearsAfterItem = value;
                    if (this.listView.IsHandleCreated)
                    {
                        this.UpdateListView();
                    }
                }
            }
        }

        public Rectangle Bounds
        {
            get
            {
                System.Windows.Forms.NativeMethods.RECT lparam = new System.Windows.Forms.NativeMethods.RECT();
                this.listView.SendMessage(0x10a9, 0, ref lparam);
                return Rectangle.FromLTRB(lparam.left, lparam.top, lparam.right, lparam.bottom);
            }
        }

        public System.Drawing.Color Color
        {
            get
            {
                if (this.color.IsEmpty)
                {
                    this.color = System.Windows.Forms.SafeNativeMethods.ColorFromCOLORREF((int) this.listView.SendMessage(0x10ab, 0, 0));
                }
                return this.color;
            }
            set
            {
                if (this.color != value)
                {
                    this.color = value;
                    if (this.listView.IsHandleCreated)
                    {
                        this.listView.SendMessage(0x10aa, 0, System.Windows.Forms.SafeNativeMethods.ColorToCOLORREF(this.color));
                    }
                }
            }
        }

        public int Index
        {
            get
            {
                return this.index;
            }
            set
            {
                if (this.index != value)
                {
                    this.index = value;
                    if (this.listView.IsHandleCreated)
                    {
                        this.UpdateListView();
                    }
                }
            }
        }
    }
}

