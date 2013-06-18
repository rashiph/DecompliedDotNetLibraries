namespace Microsoft.VisualBasic.FileIO
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;

    internal class QuoteDelimitedFieldBuilder
    {
        private int m_DelimiterLength;
        private Regex m_DelimiterRegex;
        private StringBuilder m_Field = new StringBuilder();
        private bool m_FieldFinished;
        private int m_Index;
        private bool m_MalformedLine;
        private string m_SpaceChars;

        public QuoteDelimitedFieldBuilder(Regex DelimiterRegex, string SpaceChars)
        {
            this.m_DelimiterRegex = DelimiterRegex;
            this.m_SpaceChars = SpaceChars;
        }

        public void BuildField(string Line, int StartAt)
        {
            this.m_Index = StartAt;
            int length = Line.Length;
            while (this.m_Index < length)
            {
                if (Line[this.m_Index] == '"')
                {
                    if ((this.m_Index + 1) == length)
                    {
                        this.m_FieldFinished = true;
                        this.m_DelimiterLength = 1;
                        this.m_Index++;
                        return;
                    }
                    if (!(((this.m_Index + 1) < Line.Length) & (Line[this.m_Index + 1] == '"')))
                    {
                        int num2;
                        Match match = this.m_DelimiterRegex.Match(Line, (int) (this.m_Index + 1));
                        if (!match.Success)
                        {
                            num2 = length - 1;
                        }
                        else
                        {
                            num2 = match.Index - 1;
                        }
                        int num4 = num2;
                        for (int i = this.m_Index + 1; i <= num4; i++)
                        {
                            if (this.m_SpaceChars.IndexOf(Line[i]) < 0)
                            {
                                this.m_MalformedLine = true;
                                return;
                            }
                        }
                        this.m_DelimiterLength = (1 + num2) - this.m_Index;
                        if (match.Success)
                        {
                            this.m_DelimiterLength += match.Length;
                        }
                        this.m_FieldFinished = true;
                        return;
                    }
                    this.m_Field.Append('"');
                    this.m_Index += 2;
                }
                else
                {
                    this.m_Field.Append(Line[this.m_Index]);
                    this.m_Index++;
                }
            }
        }

        public int DelimiterLength
        {
            get
            {
                return this.m_DelimiterLength;
            }
        }

        public string Field
        {
            get
            {
                return this.m_Field.ToString();
            }
        }

        public bool FieldFinished
        {
            get
            {
                return this.m_FieldFinished;
            }
        }

        public int Index
        {
            get
            {
                return this.m_Index;
            }
        }

        public bool MalformedLine
        {
            get
            {
                return this.m_MalformedLine;
            }
        }
    }
}

