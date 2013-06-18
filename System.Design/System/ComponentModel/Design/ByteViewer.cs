namespace System.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Windows.Forms;

    [ToolboxItem(false), DesignTimeVisible(false)]
    public class ByteViewer : TableLayoutPanel
    {
        private static readonly Font ADDRESS_FONT = new Font("Microsoft Sans Serif", 8f);
        private const int ADDRESS_START_X = 5;
        private const int ADDRESS_WIDTH = 0x45;
        private const int BORDER_GAP = 2;
        private const int CELL_HEIGHT = 0x15;
        private const int CELL_WIDTH = 0x19;
        private const int CHAR_WIDTH = 8;
        private const int CLIENT_START_Y = 5;
        private const int COLUMN_COUNT = 0x10;
        private int columnCount = 0x10;
        private byte[] dataBuf;
        private const int DEFAULT_COLUMN_COUNT = 0x10;
        private const int DEFAULT_ROW_COUNT = 0x19;
        private int displayLinesCount;
        private DisplayMode displayMode;
        private const int DUMP_START_X = 0x1df;
        private const int DUMP_WIDTH = 0x80;
        private TextBox edit;
        private const int HEX_DUMP_GAP = 5;
        private const int HEX_START_X = 0x4a;
        private const int HEX_WIDTH = 400;
        private static readonly Font HEXDUMP_FONT = new Font("Courier New", 8f);
        private const int INSET_GAP = 3;
        private const int LINE_START_Y = 7;
        private int linesCount;
        private DisplayMode realDisplayMode;
        private int rowCount = 0x19;
        private VScrollBar scrollBar;
        private int SCROLLBAR_HEIGHT;
        private const int SCROLLBAR_START_X = 0x264;
        private int SCROLLBAR_WIDTH;
        private int startLine;

        public ByteViewer()
        {
            base.SuspendLayout();
            base.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
            base.ColumnCount = 1;
            base.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            base.RowCount = 1;
            base.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            this.InitUI();
            base.ResumeLayout();
            this.displayMode = DisplayMode.Hexdump;
            this.realDisplayMode = DisplayMode.Hexdump;
            this.DoubleBuffered = true;
            base.SetStyle(ControlStyles.ResizeRedraw, true);
        }

        private static int AnalizeByteOrderMark(byte[] buffer, int index)
        {
            int num = (buffer[index] << 8) | buffer[index + 1];
            int num2 = (buffer[index + 2] << 8) | buffer[index + 3];
            int encodingIndex = GetEncodingIndex(num);
            int num4 = GetEncodingIndex(num2);
            int[,] numArray = new int[,] { { 1, 5, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 11, 1, 10, 4, 1, 1, 1, 1, 1, 1 }, { 2, 9, 5, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 }, { 3, 7, 3, 7, 3, 3, 3, 3, 3, 3, 3, 3, 3 }, { 14, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 6, 1, 1, 1, 1, 1, 3, 1, 1, 1, 1, 1 }, { 1, 8, 1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 13, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 12 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
            return numArray[encodingIndex, num4];
        }

        private int CellToIndex(int column, int row)
        {
            return ((row * this.columnCount) + column);
        }

        private static bool CharIsPrintable(char c)
        {
            UnicodeCategory unicodeCategory = char.GetUnicodeCategory(c);
            if (((unicodeCategory == UnicodeCategory.Control) && (unicodeCategory != UnicodeCategory.Format)) && ((unicodeCategory != UnicodeCategory.LineSeparator) && (unicodeCategory != UnicodeCategory.ParagraphSeparator)))
            {
                return (unicodeCategory == UnicodeCategory.OtherNotAssigned);
            }
            return true;
        }

        private byte[] ComposeLineBuffer(int startLine, int line)
        {
            byte[] buffer;
            int num = startLine * this.columnCount;
            if ((num + ((line + 1) * this.columnCount)) > this.dataBuf.Length)
            {
                buffer = new byte[this.dataBuf.Length % this.columnCount];
            }
            else
            {
                buffer = new byte[this.columnCount];
            }
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = this.dataBuf[num + this.CellToIndex(i, line)];
            }
            return buffer;
        }

        private void DrawAddress(Graphics g, int startLine, int line)
        {
            Font font = ADDRESS_FONT;
            string s = ((startLine + line) * this.columnCount).ToString("X8", CultureInfo.InvariantCulture);
            using (Brush brush = new SolidBrush(this.ForeColor))
            {
                g.DrawString(s, font, brush, 5f, (float) (7 + (line * 0x15)));
            }
        }

        private void DrawClient(Graphics g)
        {
            using (Brush brush = new SolidBrush(SystemColors.ControlLightLight))
            {
                g.FillRectangle(brush, new Rectangle(0x4a, 5, 0x21a, this.rowCount * 0x15));
            }
            using (Pen pen = new Pen(SystemColors.ControlDark))
            {
                g.DrawRectangle(pen, new Rectangle(0x4a, 5, 0x219, (this.rowCount * 0x15) - 1));
                g.DrawLine(pen, 0x1da, 5, 0x1da, (5 + (this.rowCount * 0x15)) - 1);
            }
        }

        private void DrawDump(Graphics g, byte[] lineBuffer, int line)
        {
            StringBuilder builder = new StringBuilder(lineBuffer.Length);
            for (int i = 0; i < lineBuffer.Length; i++)
            {
                char c = Convert.ToChar(lineBuffer[i]);
                if (CharIsPrintable(c))
                {
                    builder.Append(c);
                }
                else
                {
                    builder.Append('.');
                }
            }
            Font font = HEXDUMP_FONT;
            using (Brush brush = new SolidBrush(this.ForeColor))
            {
                g.DrawString(builder.ToString(), font, brush, 479f, (float) (7 + (line * 0x15)));
            }
        }

        private void DrawHex(Graphics g, byte[] lineBuffer, int line)
        {
            Font font = HEXDUMP_FONT;
            StringBuilder builder = new StringBuilder((lineBuffer.Length * 3) + 1);
            for (int i = 0; i < lineBuffer.Length; i++)
            {
                builder.Append(lineBuffer[i].ToString("X2", CultureInfo.InvariantCulture));
                builder.Append(" ");
                if (i == ((this.columnCount / 2) - 1))
                {
                    builder.Append(" ");
                }
            }
            using (Brush brush = new SolidBrush(this.ForeColor))
            {
                g.DrawString(builder.ToString(), font, brush, 76f, (float) (7 + (line * 0x15)));
            }
        }

        private void DrawLines(Graphics g, int startLine, int linesCount)
        {
            for (int i = 0; i < linesCount; i++)
            {
                byte[] lineBuffer = this.ComposeLineBuffer(startLine, i);
                this.DrawAddress(g, startLine, i);
                this.DrawHex(g, lineBuffer, i);
                this.DrawDump(g, lineBuffer, i);
            }
        }

        private DisplayMode GetAutoDisplayMode()
        {
            int num = 0;
            int num2 = 0;
            if ((this.dataBuf != null) && ((this.dataBuf.Length < 0) || (this.dataBuf.Length >= 8)))
            {
                int num3;
                switch (AnalizeByteOrderMark(this.dataBuf, 0))
                {
                    case 2:
                        return DisplayMode.Hexdump;

                    case 3:
                        return DisplayMode.Unicode;

                    case 4:
                    case 5:
                        return DisplayMode.Hexdump;

                    case 6:
                    case 7:
                        return DisplayMode.Hexdump;

                    case 8:
                    case 9:
                        return DisplayMode.Hexdump;

                    case 10:
                    case 11:
                        return DisplayMode.Hexdump;

                    case 12:
                        return DisplayMode.Hexdump;

                    case 13:
                        return DisplayMode.Ansi;

                    case 14:
                        return DisplayMode.Ansi;
                }
                if (this.dataBuf.Length > 0x400)
                {
                    num3 = 0x200;
                }
                else
                {
                    num3 = this.dataBuf.Length / 2;
                }
                for (int i = 0; i < num3; i++)
                {
                    char c = (char) this.dataBuf[i];
                    if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                    {
                        num++;
                    }
                }
                for (int j = 0; j < num3; j += 2)
                {
                    char[] chars = new char[1];
                    Encoding.Unicode.GetChars(this.dataBuf, j, 2, chars, 0);
                    if (CharIsPrintable(chars[0]))
                    {
                        num2++;
                    }
                }
                if (((num2 * 100) / (num3 / 2)) > 80)
                {
                    return DisplayMode.Unicode;
                }
                if (((num * 100) / num3) > 80)
                {
                    return DisplayMode.Ansi;
                }
            }
            return DisplayMode.Hexdump;
        }

        public virtual byte[] GetBytes()
        {
            return this.dataBuf;
        }

        public virtual DisplayMode GetDisplayMode()
        {
            return this.displayMode;
        }

        private static int GetEncodingIndex(int c1)
        {
            switch (c1)
            {
                case 0x3c00:
                    return 5;

                case 0x3c3f:
                    return 9;

                case 0x3f00:
                    return 7;

                case 0:
                    return 1;

                case 60:
                    return 6;

                case 0x3f:
                    return 8;

                case 0x4c6f:
                    return 11;

                case 0x786d:
                    return 10;

                case 0xa794:
                    return 12;

                case 0xefbb:
                    return 4;

                case 0xfeff:
                    return 2;

                case 0xfffe:
                    return 3;
            }
            return 0;
        }

        private void InitAnsi()
        {
            int length = this.dataBuf.Length;
            char[] lpWideCharStr = new char[length + 1];
            length = System.Design.NativeMethods.MultiByteToWideChar(0, 0, this.dataBuf, length, lpWideCharStr, length);
            lpWideCharStr[length] = '\0';
            for (int i = 0; i < length; i++)
            {
                if (lpWideCharStr[i] == '\0')
                {
                    lpWideCharStr[i] = '\v';
                }
            }
            this.edit.Text = new string(lpWideCharStr);
        }

        private void InitState()
        {
            this.linesCount = ((this.dataBuf.Length + this.columnCount) - 1) / this.columnCount;
            this.startLine = 0;
            if (this.linesCount > this.rowCount)
            {
                this.displayLinesCount = this.rowCount;
                this.scrollBar.Hide();
                this.scrollBar.Maximum = this.linesCount - 1;
                this.scrollBar.LargeChange = this.rowCount;
                this.scrollBar.Show();
                this.scrollBar.Enabled = true;
            }
            else
            {
                this.displayLinesCount = this.linesCount;
                this.scrollBar.Hide();
                this.scrollBar.Maximum = this.rowCount;
                this.scrollBar.LargeChange = this.rowCount;
                this.scrollBar.Show();
                this.scrollBar.Enabled = false;
            }
            this.scrollBar.Select();
            base.Invalidate();
        }

        private void InitUI()
        {
            this.SCROLLBAR_HEIGHT = SystemInformation.HorizontalScrollBarHeight;
            this.SCROLLBAR_WIDTH = SystemInformation.VerticalScrollBarWidth;
            base.Size = new Size(((0x264 + this.SCROLLBAR_WIDTH) + 2) + 3, 10 + (this.rowCount * 0x15));
            this.scrollBar = new VScrollBar();
            this.scrollBar.ValueChanged += new EventHandler(this.ScrollChanged);
            this.scrollBar.TabStop = true;
            this.scrollBar.TabIndex = 0;
            this.scrollBar.Dock = DockStyle.Right;
            this.scrollBar.Visible = false;
            this.edit = new TextBox();
            this.edit.AutoSize = false;
            this.edit.BorderStyle = BorderStyle.None;
            this.edit.Multiline = true;
            this.edit.ReadOnly = true;
            this.edit.ScrollBars = ScrollBars.Both;
            this.edit.AcceptsTab = true;
            this.edit.AcceptsReturn = true;
            this.edit.Dock = DockStyle.Fill;
            this.edit.Margin = Padding.Empty;
            this.edit.WordWrap = false;
            this.edit.Visible = false;
            base.Controls.Add(this.scrollBar, 0, 0);
            base.Controls.Add(this.edit, 0, 0);
        }

        private void InitUnicode()
        {
            char[] chars = new char[(this.dataBuf.Length / 2) + 1];
            Encoding.Unicode.GetChars(this.dataBuf, 0, this.dataBuf.Length, chars, 0);
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == '\0')
                {
                    chars[i] = '\v';
                }
            }
            chars[chars.Length - 1] = '\0';
            this.edit.Text = new string(chars);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            this.scrollBar.Select();
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            int num = (base.ClientSize.Height - 10) / 0x15;
            if ((num >= 0) && (num != this.rowCount))
            {
                this.rowCount = num;
            }
            else
            {
                return;
            }
            if (this.Dock == DockStyle.None)
            {
                base.Size = new Size(((0x264 + this.SCROLLBAR_WIDTH) + 2) + 3, 10 + (this.rowCount * 0x15));
            }
            if (this.scrollBar != null)
            {
                if (this.linesCount > this.rowCount)
                {
                    this.scrollBar.Hide();
                    this.scrollBar.Maximum = this.linesCount - 1;
                    this.scrollBar.LargeChange = this.rowCount;
                    this.scrollBar.Show();
                    this.scrollBar.Enabled = true;
                    this.scrollBar.Select();
                }
                else
                {
                    this.scrollBar.Enabled = false;
                }
            }
            this.displayLinesCount = ((this.startLine + this.rowCount) < this.linesCount) ? this.rowCount : (this.linesCount - this.startLine);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            switch (this.realDisplayMode)
            {
                case DisplayMode.Hexdump:
                    base.SuspendLayout();
                    this.edit.Hide();
                    this.scrollBar.Show();
                    base.ResumeLayout();
                    this.DrawClient(g);
                    this.DrawLines(g, this.startLine, this.displayLinesCount);
                    return;

                case DisplayMode.Ansi:
                    this.edit.Invalidate();
                    return;

                case DisplayMode.Unicode:
                    this.edit.Invalidate();
                    return;
            }
        }

        public virtual void SaveToFile(string path)
        {
            if (this.dataBuf != null)
            {
                FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                try
                {
                    stream.Write(this.dataBuf, 0, this.dataBuf.Length);
                    stream.Close();
                }
                catch
                {
                    stream.Close();
                    throw;
                }
            }
        }

        protected virtual void ScrollChanged(object source, EventArgs e)
        {
            this.startLine = this.scrollBar.Value;
            base.Invalidate();
        }

        public virtual void SetBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (this.dataBuf != null)
            {
                this.dataBuf = null;
            }
            this.dataBuf = bytes;
            this.InitState();
            this.SetDisplayMode(this.displayMode);
        }

        public virtual void SetDisplayMode(DisplayMode mode)
        {
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(mode, (int) mode, 1, 4))
            {
                throw new InvalidEnumArgumentException("mode", (int) mode, typeof(DisplayMode));
            }
            this.displayMode = mode;
            this.realDisplayMode = (mode == DisplayMode.Auto) ? this.GetAutoDisplayMode() : mode;
            switch (this.realDisplayMode)
            {
                case DisplayMode.Hexdump:
                    base.SuspendLayout();
                    this.edit.Hide();
                    if (this.linesCount <= this.rowCount)
                    {
                        base.ResumeLayout();
                        return;
                    }
                    if (this.scrollBar.Visible)
                    {
                        base.ResumeLayout();
                        return;
                    }
                    this.scrollBar.Show();
                    base.ResumeLayout();
                    this.scrollBar.Invalidate();
                    this.scrollBar.Select();
                    return;

                case DisplayMode.Ansi:
                    this.InitAnsi();
                    base.SuspendLayout();
                    this.edit.Show();
                    this.scrollBar.Hide();
                    base.ResumeLayout();
                    base.Invalidate();
                    return;

                case DisplayMode.Unicode:
                    this.InitUnicode();
                    base.SuspendLayout();
                    this.edit.Show();
                    this.scrollBar.Hide();
                    base.ResumeLayout();
                    base.Invalidate();
                    return;
            }
        }

        public virtual void SetFile(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
            try
            {
                int length = (int) stream.Length;
                byte[] buffer = new byte[length + 1];
                stream.Read(buffer, 0, length);
                this.SetBytes(buffer);
                stream.Close();
            }
            catch
            {
                stream.Close();
                throw;
            }
        }

        public virtual void SetStartLine(int line)
        {
            if (((line < 0) || (line >= this.linesCount)) || (line > (this.dataBuf.Length / this.columnCount)))
            {
                this.startLine = 0;
            }
            else
            {
                this.startLine = line;
            }
        }
    }
}

