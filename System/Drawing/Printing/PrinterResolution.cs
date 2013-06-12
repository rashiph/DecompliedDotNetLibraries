namespace System.Drawing.Printing
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;

    [Serializable]
    public class PrinterResolution
    {
        private PrinterResolutionKind kind;
        private int x;
        private int y;

        public PrinterResolution()
        {
            this.kind = PrinterResolutionKind.Custom;
        }

        internal PrinterResolution(PrinterResolutionKind kind, int x, int y)
        {
            this.kind = kind;
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            if (this.kind != PrinterResolutionKind.Custom)
            {
                return ("[PrinterResolution " + TypeDescriptor.GetConverter(typeof(PrinterResolutionKind)).ConvertToString((int) this.Kind) + "]");
            }
            return ("[PrinterResolution X=" + this.X.ToString(CultureInfo.InvariantCulture) + " Y=" + this.Y.ToString(CultureInfo.InvariantCulture) + "]");
        }

        public PrinterResolutionKind Kind
        {
            get
            {
                return this.kind;
            }
            set
            {
                if (!System.Drawing.ClientUtils.IsEnumValid(value, (int) value, -4, 0))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(PrinterResolutionKind));
                }
                this.kind = value;
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

