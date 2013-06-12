namespace System.Windows.Forms.VisualStyles
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct TextMetrics
    {
        private int height;
        private int ascent;
        private int descent;
        private int internalLeading;
        private int externalLeading;
        private int aveCharWidth;
        private int maxCharWidth;
        private int weight;
        private int overhang;
        private int digitizedAspectX;
        private int digitizedAspectY;
        private char firstChar;
        private char lastChar;
        private char defaultChar;
        private char breakChar;
        private bool italic;
        private bool underlined;
        private bool struckOut;
        private TextMetricsPitchAndFamilyValues pitchAndFamily;
        private TextMetricsCharacterSet charSet;
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
        public int Ascent
        {
            get
            {
                return this.ascent;
            }
            set
            {
                this.ascent = value;
            }
        }
        public int Descent
        {
            get
            {
                return this.descent;
            }
            set
            {
                this.descent = value;
            }
        }
        public int InternalLeading
        {
            get
            {
                return this.internalLeading;
            }
            set
            {
                this.internalLeading = value;
            }
        }
        public int ExternalLeading
        {
            get
            {
                return this.externalLeading;
            }
            set
            {
                this.externalLeading = value;
            }
        }
        public int AverageCharWidth
        {
            get
            {
                return this.aveCharWidth;
            }
            set
            {
                this.aveCharWidth = value;
            }
        }
        public int MaxCharWidth
        {
            get
            {
                return this.maxCharWidth;
            }
            set
            {
                this.maxCharWidth = value;
            }
        }
        public int Weight
        {
            get
            {
                return this.weight;
            }
            set
            {
                this.weight = value;
            }
        }
        public int Overhang
        {
            get
            {
                return this.overhang;
            }
            set
            {
                this.overhang = value;
            }
        }
        public int DigitizedAspectX
        {
            get
            {
                return this.digitizedAspectX;
            }
            set
            {
                this.digitizedAspectX = value;
            }
        }
        public int DigitizedAspectY
        {
            get
            {
                return this.digitizedAspectY;
            }
            set
            {
                this.digitizedAspectY = value;
            }
        }
        public char FirstChar
        {
            get
            {
                return this.firstChar;
            }
            set
            {
                this.firstChar = value;
            }
        }
        public char LastChar
        {
            get
            {
                return this.lastChar;
            }
            set
            {
                this.lastChar = value;
            }
        }
        public char DefaultChar
        {
            get
            {
                return this.defaultChar;
            }
            set
            {
                this.defaultChar = value;
            }
        }
        public char BreakChar
        {
            get
            {
                return this.breakChar;
            }
            set
            {
                this.breakChar = value;
            }
        }
        public bool Italic
        {
            get
            {
                return this.italic;
            }
            set
            {
                this.italic = value;
            }
        }
        public bool Underlined
        {
            get
            {
                return this.underlined;
            }
            set
            {
                this.underlined = value;
            }
        }
        public bool StruckOut
        {
            get
            {
                return this.struckOut;
            }
            set
            {
                this.struckOut = value;
            }
        }
        public TextMetricsPitchAndFamilyValues PitchAndFamily
        {
            get
            {
                return this.pitchAndFamily;
            }
            set
            {
                this.pitchAndFamily = value;
            }
        }
        public TextMetricsCharacterSet CharSet
        {
            get
            {
                return this.charSet;
            }
            set
            {
                this.charSet = value;
            }
        }
    }
}

