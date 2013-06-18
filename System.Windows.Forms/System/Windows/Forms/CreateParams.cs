namespace System.Windows.Forms
{
    using System;
    using System.Text;

    public class CreateParams
    {
        private string caption;
        private string className;
        private int classStyle;
        private int exStyle;
        private int height;
        private object param;
        private IntPtr parent;
        private int style;
        private int width;
        private int x;
        private int y;

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(0x40);
            builder.Append("CreateParams {'");
            builder.Append(this.className);
            builder.Append("', '");
            builder.Append(this.caption);
            builder.Append("', 0x");
            builder.Append(Convert.ToString(this.style, 0x10));
            builder.Append(", 0x");
            builder.Append(Convert.ToString(this.exStyle, 0x10));
            builder.Append(", {");
            builder.Append(this.x);
            builder.Append(", ");
            builder.Append(this.y);
            builder.Append(", ");
            builder.Append(this.width);
            builder.Append(", ");
            builder.Append(this.height);
            builder.Append("}");
            builder.Append("}");
            return builder.ToString();
        }

        public string Caption
        {
            get
            {
                return this.caption;
            }
            set
            {
                this.caption = value;
            }
        }

        public string ClassName
        {
            get
            {
                return this.className;
            }
            set
            {
                this.className = value;
            }
        }

        public int ClassStyle
        {
            get
            {
                return this.classStyle;
            }
            set
            {
                this.classStyle = value;
            }
        }

        public int ExStyle
        {
            get
            {
                return this.exStyle;
            }
            set
            {
                this.exStyle = value;
            }
        }

        public int Height
        {
            get
            {
                return this.height;
            }
            set
            {
                this.height = value;
            }
        }

        public object Param
        {
            get
            {
                return this.param;
            }
            set
            {
                this.param = value;
            }
        }

        public IntPtr Parent
        {
            get
            {
                return this.parent;
            }
            set
            {
                this.parent = value;
            }
        }

        public int Style
        {
            get
            {
                return this.style;
            }
            set
            {
                this.style = value;
            }
        }

        public int Width
        {
            get
            {
                return this.width;
            }
            set
            {
                this.width = value;
            }
        }

        public int X
        {
            get
            {
                return this.x;
            }
            set
            {
                this.x = value;
            }
        }

        public int Y
        {
            get
            {
                return this.y;
            }
            set
            {
                this.y = value;
            }
        }
    }
}

