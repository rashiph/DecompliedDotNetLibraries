namespace System.Drawing.Printing
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;

    [Serializable]
    public class PaperSize
    {
        private bool createdByDefaultConstructor;
        private int height;
        private PaperKind kind;
        private string name;
        private int width;

        public PaperSize()
        {
            this.kind = PaperKind.Custom;
            this.name = string.Empty;
            this.createdByDefaultConstructor = true;
        }

        public PaperSize(string name, int width, int height)
        {
            this.kind = PaperKind.Custom;
            this.name = name;
            this.width = width;
            this.height = height;
        }

        internal PaperSize(PaperKind kind, string name, int width, int height)
        {
            this.kind = kind;
            this.name = name;
            this.width = width;
            this.height = height;
        }

        public override string ToString()
        {
            return ("[PaperSize " + this.PaperName + " Kind=" + TypeDescriptor.GetConverter(typeof(PaperKind)).ConvertToString((int) this.Kind) + " Height=" + this.Height.ToString(CultureInfo.InvariantCulture) + " Width=" + this.Width.ToString(CultureInfo.InvariantCulture) + "]");
        }

        public int Height
        {
            get
            {
                return this.height;
            }
            set
            {
                if ((this.kind != PaperKind.Custom) && !this.createdByDefaultConstructor)
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("PSizeNotCustom"));
                }
                this.height = value;
            }
        }

        public PaperKind Kind
        {
            get
            {
                if (((this.kind <= PaperKind.PrcEnvelopeNumber10Rotated) && (this.kind != (PaperKind.C65Envelope | PaperKind.Standard10x14))) && (this.kind != (PaperKind.B4Envelope | PaperKind.Standard10x14)))
                {
                    return this.kind;
                }
                return PaperKind.Custom;
            }
        }

        public string PaperName
        {
            get
            {
                return this.name;
            }
            set
            {
                if ((this.kind != PaperKind.Custom) && !this.createdByDefaultConstructor)
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("PSizeNotCustom"));
                }
                this.name = value;
            }
        }

        public int RawKind
        {
            get
            {
                return (int) this.kind;
            }
            set
            {
                this.kind = (PaperKind) value;
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
                if ((this.kind != PaperKind.Custom) && !this.createdByDefaultConstructor)
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("PSizeNotCustom"));
                }
                this.width = value;
            }
        }
    }
}

