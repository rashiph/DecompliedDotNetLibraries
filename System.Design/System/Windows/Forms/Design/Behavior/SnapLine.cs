namespace System.Windows.Forms.Design.Behavior
{
    using System;

    public sealed class SnapLine
    {
        private string filter;
        internal const string Margin = "Margin";
        internal const string MarginBottom = "Margin.Bottom";
        internal const string MarginLeft = "Margin.Left";
        internal const string MarginRight = "Margin.Right";
        internal const string MarginTop = "Margin.Top";
        private int offset;
        internal const string Padding = "Padding";
        internal const string PaddingBottom = "Padding.Bottom";
        internal const string PaddingLeft = "Padding.Left";
        internal const string PaddingRight = "Padding.Right";
        internal const string PaddingTop = "Padding.Top";
        private SnapLinePriority priority;
        private System.Windows.Forms.Design.Behavior.SnapLineType type;

        public SnapLine(System.Windows.Forms.Design.Behavior.SnapLineType type, int offset) : this(type, offset, null, SnapLinePriority.Low)
        {
        }

        public SnapLine(System.Windows.Forms.Design.Behavior.SnapLineType type, int offset, string filter) : this(type, offset, filter, SnapLinePriority.Low)
        {
        }

        public SnapLine(System.Windows.Forms.Design.Behavior.SnapLineType type, int offset, SnapLinePriority priority) : this(type, offset, null, priority)
        {
        }

        public SnapLine(System.Windows.Forms.Design.Behavior.SnapLineType type, int offset, string filter, SnapLinePriority priority)
        {
            this.type = type;
            this.offset = offset;
            this.filter = filter;
            this.priority = priority;
        }

        public void AdjustOffset(int adjustment)
        {
            this.offset += adjustment;
        }

        public static bool ShouldSnap(SnapLine line1, SnapLine line2)
        {
            if (line1.SnapLineType != line2.SnapLineType)
            {
                return false;
            }
            if ((line1.Filter == null) && (line2.Filter == null))
            {
                return true;
            }
            if ((line1.Filter == null) || (line2.Filter == null))
            {
                return false;
            }
            if (line1.Filter.Contains("Margin"))
            {
                if (((!line1.Filter.Equals("Margin.Right") || (!line2.Filter.Equals("Margin.Left") && !line2.Filter.Equals("Padding.Right"))) && (!line1.Filter.Equals("Margin.Left") || (!line2.Filter.Equals("Margin.Right") && !line2.Filter.Equals("Padding.Left")))) && (((!line1.Filter.Equals("Margin.Top") || (!line2.Filter.Equals("Margin.Bottom") && !line2.Filter.Equals("Padding.Top"))) && (!line1.Filter.Equals("Margin.Bottom") || !line2.Filter.Equals("Margin.Top"))) && !line2.Filter.Equals("Padding.Bottom")))
                {
                    return false;
                }
                return true;
            }
            if (line1.Filter.Contains("Padding"))
            {
                if (((!line1.Filter.Equals("Padding.Left") || !line2.Filter.Equals("Margin.Left")) && (!line1.Filter.Equals("Padding.Right") || !line2.Filter.Equals("Margin.Right"))) && ((!line1.Filter.Equals("Padding.Top") || !line2.Filter.Equals("Margin.Top")) && (!line1.Filter.Equals("Padding.Bottom") || !line2.Filter.Equals("Margin.Bottom"))))
                {
                    return false;
                }
                return true;
            }
            return line1.Filter.Equals(line2.Filter);
        }

        public override string ToString()
        {
            return string.Concat(new object[] { "SnapLine: {type = ", this.type, ", offset  = ", this.offset, ", priority = ", this.priority, ", filter = ", (this.filter == null) ? "<null>" : this.filter, "}" });
        }

        public string Filter
        {
            get
            {
                return this.filter;
            }
        }

        public bool IsHorizontal
        {
            get
            {
                if (((this.type != System.Windows.Forms.Design.Behavior.SnapLineType.Top) && (this.type != System.Windows.Forms.Design.Behavior.SnapLineType.Bottom)) && (this.type != System.Windows.Forms.Design.Behavior.SnapLineType.Horizontal))
                {
                    return (this.type == System.Windows.Forms.Design.Behavior.SnapLineType.Baseline);
                }
                return true;
            }
        }

        public bool IsVertical
        {
            get
            {
                if ((this.type != System.Windows.Forms.Design.Behavior.SnapLineType.Left) && (this.type != System.Windows.Forms.Design.Behavior.SnapLineType.Right))
                {
                    return (this.type == System.Windows.Forms.Design.Behavior.SnapLineType.Vertical);
                }
                return true;
            }
        }

        public int Offset
        {
            get
            {
                return this.offset;
            }
        }

        public SnapLinePriority Priority
        {
            get
            {
                return this.priority;
            }
        }

        public System.Windows.Forms.Design.Behavior.SnapLineType SnapLineType
        {
            get
            {
                return this.type;
            }
        }
    }
}

