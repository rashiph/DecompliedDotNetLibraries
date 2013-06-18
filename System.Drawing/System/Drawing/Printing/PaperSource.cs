namespace System.Drawing.Printing
{
    using System;
    using System.ComponentModel;

    [Serializable]
    public class PaperSource
    {
        private PaperSourceKind kind;
        private string name;

        public PaperSource()
        {
            this.kind = PaperSourceKind.Custom;
            this.name = string.Empty;
        }

        internal PaperSource(PaperSourceKind kind, string name)
        {
            this.kind = kind;
            this.name = name;
        }

        public override string ToString()
        {
            return ("[PaperSource " + this.SourceName + " Kind=" + TypeDescriptor.GetConverter(typeof(PaperSourceKind)).ConvertToString(this.Kind) + "]");
        }

        public PaperSourceKind Kind
        {
            get
            {
                if (this.kind >= ((PaperSourceKind) 0x100))
                {
                    return PaperSourceKind.Custom;
                }
                return this.kind;
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
                this.kind = (PaperSourceKind) value;
            }
        }

        public string SourceName
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }
    }
}

