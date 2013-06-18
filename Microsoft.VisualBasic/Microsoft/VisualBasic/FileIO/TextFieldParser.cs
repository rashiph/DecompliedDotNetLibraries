namespace Microsoft.VisualBasic.FileIO
{
    using Microsoft.VisualBasic;
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Text.RegularExpressions;

    public class TextFieldParser : IDisposable
    {
        private const string BEGINS_WITH_QUOTE = "\\G[{0}]*\"";
        private const int DEFAULT_BUFFER_LENGTH = 0x1000;
        private const int DEFAULT_BUILDER_INCREASE = 10;
        private const string ENDING_QUOTE = "\"[{0}]*";
        private Regex m_BeginQuotesRegex;
        private char[] m_Buffer;
        private int m_CharsRead;
        private string[] m_CommentTokens;
        private Regex m_DelimiterRegex;
        private string[] m_Delimiters;
        private string[] m_DelimitersCopy;
        private Regex m_DelimiterWithEndCharsRegex;
        private bool m_Disposed;
        private bool m_EndOfData;
        private string m_ErrorLine;
        private long m_ErrorLineNumber;
        private int[] m_FieldWidths;
        private int[] m_FieldWidthsCopy;
        private bool m_HasFieldsEnclosedInQuotes;
        private bool m_LeaveOpen;
        private int m_LineLength;
        private long m_LineNumber;
        private int m_MaxBufferSize;
        private int m_MaxLineSize;
        private bool m_NeedPropertyCheck;
        private int m_PeekPosition;
        private int m_Position;
        private TextReader m_Reader;
        private string m_SpaceChars;
        private FieldType m_TextFieldType;
        private bool m_TrimWhiteSpace;
        private int[] m_WhitespaceCodes;
        private Regex m_WhiteSpaceRegEx;
        private const RegexOptions REGEX_OPTIONS = RegexOptions.CultureInvariant;

        [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
        public TextFieldParser(Stream stream)
        {
            this.m_CommentTokens = new string[0];
            this.m_LineNumber = 1L;
            this.m_EndOfData = false;
            this.m_ErrorLine = "";
            this.m_ErrorLineNumber = -1L;
            this.m_TextFieldType = FieldType.Delimited;
            this.m_WhitespaceCodes = new int[] { 
                9, 11, 12, 0x20, 0x85, 160, 0x1680, 0x2000, 0x2001, 0x2002, 0x2003, 0x2004, 0x2005, 0x2006, 0x2007, 0x2008, 
                0x2009, 0x200a, 0x200b, 0x2028, 0x2029, 0x3000, 0xfeff
             };
            this.m_WhiteSpaceRegEx = new Regex(@"\s", RegexOptions.CultureInvariant);
            this.m_TrimWhiteSpace = true;
            this.m_Position = 0;
            this.m_PeekPosition = 0;
            this.m_CharsRead = 0;
            this.m_NeedPropertyCheck = true;
            this.m_Buffer = new char[0x1000];
            this.m_HasFieldsEnclosedInQuotes = true;
            this.m_MaxLineSize = 0x989680;
            this.m_MaxBufferSize = 0x989680;
            this.m_LeaveOpen = false;
            this.InitializeFromStream(stream, Encoding.UTF8, true);
        }

        [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
        public TextFieldParser(TextReader reader)
        {
            this.m_CommentTokens = new string[0];
            this.m_LineNumber = 1L;
            this.m_EndOfData = false;
            this.m_ErrorLine = "";
            this.m_ErrorLineNumber = -1L;
            this.m_TextFieldType = FieldType.Delimited;
            this.m_WhitespaceCodes = new int[] { 
                9, 11, 12, 0x20, 0x85, 160, 0x1680, 0x2000, 0x2001, 0x2002, 0x2003, 0x2004, 0x2005, 0x2006, 0x2007, 0x2008, 
                0x2009, 0x200a, 0x200b, 0x2028, 0x2029, 0x3000, 0xfeff
             };
            this.m_WhiteSpaceRegEx = new Regex(@"\s", RegexOptions.CultureInvariant);
            this.m_TrimWhiteSpace = true;
            this.m_Position = 0;
            this.m_PeekPosition = 0;
            this.m_CharsRead = 0;
            this.m_NeedPropertyCheck = true;
            this.m_Buffer = new char[0x1000];
            this.m_HasFieldsEnclosedInQuotes = true;
            this.m_MaxLineSize = 0x989680;
            this.m_MaxBufferSize = 0x989680;
            this.m_LeaveOpen = false;
            if (reader == null)
            {
                throw ExceptionUtils.GetArgumentNullException("reader");
            }
            this.m_Reader = reader;
            this.ReadToBuffer();
        }

        [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
        public TextFieldParser(string path)
        {
            this.m_CommentTokens = new string[0];
            this.m_LineNumber = 1L;
            this.m_EndOfData = false;
            this.m_ErrorLine = "";
            this.m_ErrorLineNumber = -1L;
            this.m_TextFieldType = FieldType.Delimited;
            this.m_WhitespaceCodes = new int[] { 
                9, 11, 12, 0x20, 0x85, 160, 0x1680, 0x2000, 0x2001, 0x2002, 0x2003, 0x2004, 0x2005, 0x2006, 0x2007, 0x2008, 
                0x2009, 0x200a, 0x200b, 0x2028, 0x2029, 0x3000, 0xfeff
             };
            this.m_WhiteSpaceRegEx = new Regex(@"\s", RegexOptions.CultureInvariant);
            this.m_TrimWhiteSpace = true;
            this.m_Position = 0;
            this.m_PeekPosition = 0;
            this.m_CharsRead = 0;
            this.m_NeedPropertyCheck = true;
            this.m_Buffer = new char[0x1000];
            this.m_HasFieldsEnclosedInQuotes = true;
            this.m_MaxLineSize = 0x989680;
            this.m_MaxBufferSize = 0x989680;
            this.m_LeaveOpen = false;
            this.InitializeFromPath(path, Encoding.UTF8, true);
        }

        [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
        public TextFieldParser(Stream stream, Encoding defaultEncoding)
        {
            this.m_CommentTokens = new string[0];
            this.m_LineNumber = 1L;
            this.m_EndOfData = false;
            this.m_ErrorLine = "";
            this.m_ErrorLineNumber = -1L;
            this.m_TextFieldType = FieldType.Delimited;
            this.m_WhitespaceCodes = new int[] { 
                9, 11, 12, 0x20, 0x85, 160, 0x1680, 0x2000, 0x2001, 0x2002, 0x2003, 0x2004, 0x2005, 0x2006, 0x2007, 0x2008, 
                0x2009, 0x200a, 0x200b, 0x2028, 0x2029, 0x3000, 0xfeff
             };
            this.m_WhiteSpaceRegEx = new Regex(@"\s", RegexOptions.CultureInvariant);
            this.m_TrimWhiteSpace = true;
            this.m_Position = 0;
            this.m_PeekPosition = 0;
            this.m_CharsRead = 0;
            this.m_NeedPropertyCheck = true;
            this.m_Buffer = new char[0x1000];
            this.m_HasFieldsEnclosedInQuotes = true;
            this.m_MaxLineSize = 0x989680;
            this.m_MaxBufferSize = 0x989680;
            this.m_LeaveOpen = false;
            this.InitializeFromStream(stream, defaultEncoding, true);
        }

        [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
        public TextFieldParser(string path, Encoding defaultEncoding)
        {
            this.m_CommentTokens = new string[0];
            this.m_LineNumber = 1L;
            this.m_EndOfData = false;
            this.m_ErrorLine = "";
            this.m_ErrorLineNumber = -1L;
            this.m_TextFieldType = FieldType.Delimited;
            this.m_WhitespaceCodes = new int[] { 
                9, 11, 12, 0x20, 0x85, 160, 0x1680, 0x2000, 0x2001, 0x2002, 0x2003, 0x2004, 0x2005, 0x2006, 0x2007, 0x2008, 
                0x2009, 0x200a, 0x200b, 0x2028, 0x2029, 0x3000, 0xfeff
             };
            this.m_WhiteSpaceRegEx = new Regex(@"\s", RegexOptions.CultureInvariant);
            this.m_TrimWhiteSpace = true;
            this.m_Position = 0;
            this.m_PeekPosition = 0;
            this.m_CharsRead = 0;
            this.m_NeedPropertyCheck = true;
            this.m_Buffer = new char[0x1000];
            this.m_HasFieldsEnclosedInQuotes = true;
            this.m_MaxLineSize = 0x989680;
            this.m_MaxBufferSize = 0x989680;
            this.m_LeaveOpen = false;
            this.InitializeFromPath(path, defaultEncoding, true);
        }

        [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
        public TextFieldParser(Stream stream, Encoding defaultEncoding, bool detectEncoding)
        {
            this.m_CommentTokens = new string[0];
            this.m_LineNumber = 1L;
            this.m_EndOfData = false;
            this.m_ErrorLine = "";
            this.m_ErrorLineNumber = -1L;
            this.m_TextFieldType = FieldType.Delimited;
            this.m_WhitespaceCodes = new int[] { 
                9, 11, 12, 0x20, 0x85, 160, 0x1680, 0x2000, 0x2001, 0x2002, 0x2003, 0x2004, 0x2005, 0x2006, 0x2007, 0x2008, 
                0x2009, 0x200a, 0x200b, 0x2028, 0x2029, 0x3000, 0xfeff
             };
            this.m_WhiteSpaceRegEx = new Regex(@"\s", RegexOptions.CultureInvariant);
            this.m_TrimWhiteSpace = true;
            this.m_Position = 0;
            this.m_PeekPosition = 0;
            this.m_CharsRead = 0;
            this.m_NeedPropertyCheck = true;
            this.m_Buffer = new char[0x1000];
            this.m_HasFieldsEnclosedInQuotes = true;
            this.m_MaxLineSize = 0x989680;
            this.m_MaxBufferSize = 0x989680;
            this.m_LeaveOpen = false;
            this.InitializeFromStream(stream, defaultEncoding, detectEncoding);
        }

        [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
        public TextFieldParser(string path, Encoding defaultEncoding, bool detectEncoding)
        {
            this.m_CommentTokens = new string[0];
            this.m_LineNumber = 1L;
            this.m_EndOfData = false;
            this.m_ErrorLine = "";
            this.m_ErrorLineNumber = -1L;
            this.m_TextFieldType = FieldType.Delimited;
            this.m_WhitespaceCodes = new int[] { 
                9, 11, 12, 0x20, 0x85, 160, 0x1680, 0x2000, 0x2001, 0x2002, 0x2003, 0x2004, 0x2005, 0x2006, 0x2007, 0x2008, 
                0x2009, 0x200a, 0x200b, 0x2028, 0x2029, 0x3000, 0xfeff
             };
            this.m_WhiteSpaceRegEx = new Regex(@"\s", RegexOptions.CultureInvariant);
            this.m_TrimWhiteSpace = true;
            this.m_Position = 0;
            this.m_PeekPosition = 0;
            this.m_CharsRead = 0;
            this.m_NeedPropertyCheck = true;
            this.m_Buffer = new char[0x1000];
            this.m_HasFieldsEnclosedInQuotes = true;
            this.m_MaxLineSize = 0x989680;
            this.m_MaxBufferSize = 0x989680;
            this.m_LeaveOpen = false;
            this.InitializeFromPath(path, defaultEncoding, detectEncoding);
        }

        [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
        public TextFieldParser(Stream stream, Encoding defaultEncoding, bool detectEncoding, bool leaveOpen)
        {
            this.m_CommentTokens = new string[0];
            this.m_LineNumber = 1L;
            this.m_EndOfData = false;
            this.m_ErrorLine = "";
            this.m_ErrorLineNumber = -1L;
            this.m_TextFieldType = FieldType.Delimited;
            this.m_WhitespaceCodes = new int[] { 
                9, 11, 12, 0x20, 0x85, 160, 0x1680, 0x2000, 0x2001, 0x2002, 0x2003, 0x2004, 0x2005, 0x2006, 0x2007, 0x2008, 
                0x2009, 0x200a, 0x200b, 0x2028, 0x2029, 0x3000, 0xfeff
             };
            this.m_WhiteSpaceRegEx = new Regex(@"\s", RegexOptions.CultureInvariant);
            this.m_TrimWhiteSpace = true;
            this.m_Position = 0;
            this.m_PeekPosition = 0;
            this.m_CharsRead = 0;
            this.m_NeedPropertyCheck = true;
            this.m_Buffer = new char[0x1000];
            this.m_HasFieldsEnclosedInQuotes = true;
            this.m_MaxLineSize = 0x989680;
            this.m_MaxBufferSize = 0x989680;
            this.m_LeaveOpen = false;
            this.m_LeaveOpen = leaveOpen;
            this.InitializeFromStream(stream, defaultEncoding, detectEncoding);
        }

        private bool ArrayHasChanged()
        {
            int lowerBound = 0;
            int upperBound = 0;
            switch (this.m_TextFieldType)
            {
                case FieldType.Delimited:
                    if (this.m_Delimiters != null)
                    {
                        lowerBound = this.m_DelimitersCopy.GetLowerBound(0);
                        upperBound = this.m_DelimitersCopy.GetUpperBound(0);
                        int num5 = upperBound;
                        for (int i = lowerBound; i <= num5; i++)
                        {
                            if (this.m_Delimiters[i] != this.m_DelimitersCopy[i])
                            {
                                return true;
                            }
                        }
                        break;
                    }
                    return false;

                case FieldType.FixedWidth:
                    if (this.m_FieldWidths != null)
                    {
                        lowerBound = this.m_FieldWidthsCopy.GetLowerBound(0);
                        upperBound = this.m_FieldWidthsCopy.GetUpperBound(0);
                        int num6 = upperBound;
                        for (int j = lowerBound; j <= num6; j++)
                        {
                            if (this.m_FieldWidths[j] != this.m_FieldWidthsCopy[j])
                            {
                                return true;
                            }
                        }
                        break;
                    }
                    return false;
            }
            return false;
        }

        private bool CharacterIsInDelimiter(char testCharacter)
        {
            foreach (string str in this.m_Delimiters)
            {
                if (str.IndexOf(testCharacter) > -1)
                {
                    return true;
                }
            }
            return false;
        }

        private void CheckCommentTokensForWhitespace(string[] tokens)
        {
            if (tokens != null)
            {
                foreach (string str in tokens)
                {
                    if (this.m_WhiteSpaceRegEx.IsMatch(str))
                    {
                        throw ExceptionUtils.GetArgumentExceptionWithArgName("CommentTokens", "TextFieldParser_WhitespaceInToken", new string[0]);
                    }
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Close()
        {
            this.CloseReader();
        }

        private void CloseReader()
        {
            this.FinishReading();
            if (this.m_Reader != null)
            {
                if (!this.m_LeaveOpen)
                {
                    this.m_Reader.Close();
                }
                this.m_Reader = null;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!this.m_Disposed)
                {
                    this.Close();
                }
                this.m_Disposed = true;
            }
        }

        protected override void Finalize()
        {
            this.Dispose(false);
            base.Finalize();
        }

        private void FinishReading()
        {
            this.m_LineNumber = -1L;
            this.m_EndOfData = true;
            this.m_Buffer = null;
            this.m_DelimiterRegex = null;
            this.m_BeginQuotesRegex = null;
        }

        private int GetEndOfLineIndex(string Line)
        {
            int length = Line.Length;
            if (length != 1)
            {
                if ((Conversions.ToString(Line[length - 2]) == "\r") | (Conversions.ToString(Line[length - 2]) == "\n"))
                {
                    return (length - 2);
                }
                if ((Conversions.ToString(Line[length - 1]) == "\r") | (Conversions.ToString(Line[length - 1]) == "\n"))
                {
                    return (length - 1);
                }
            }
            return length;
        }

        private string GetFixedWidthField(StringInfo Line, int Index, int FieldLength)
        {
            string str;
            if (FieldLength > 0)
            {
                str = Line.SubstringByTextElements(Index, FieldLength);
            }
            else if (Index >= Line.LengthInTextElements)
            {
                str = string.Empty;
            }
            else
            {
                str = Line.SubstringByTextElements(Index).TrimEnd(new char[] { '\r', '\n' });
            }
            if (this.m_TrimWhiteSpace)
            {
                return str.Trim();
            }
            return str;
        }

        private bool IgnoreLine(string line)
        {
            if (line != null)
            {
                string str = line.Trim();
                if (str.Length == 0)
                {
                    return true;
                }
                if (this.m_CommentTokens != null)
                {
                    foreach (string str2 in this.m_CommentTokens)
                    {
                        if (str2 != "")
                        {
                            if (str.StartsWith(str2, StringComparison.Ordinal))
                            {
                                return true;
                            }
                            if (line.StartsWith(str2, StringComparison.Ordinal))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private int IncreaseBufferSize()
        {
            this.m_PeekPosition = this.m_CharsRead;
            int num = this.m_Buffer.Length + 0x1000;
            if (num > this.m_MaxBufferSize)
            {
                throw ExceptionUtils.GetInvalidOperationException("TextFieldParser_BufferExceededMaxSize", new string[0]);
            }
            char[] destinationArray = new char[(num - 1) + 1];
            Array.Copy(this.m_Buffer, destinationArray, this.m_Buffer.Length);
            int num2 = this.m_Reader.Read(destinationArray, this.m_Buffer.Length, 0x1000);
            this.m_Buffer = destinationArray;
            this.m_CharsRead += num2;
            return num2;
        }

        private void InitializeFromPath(string path, Encoding defaultEncoding, bool detectEncoding)
        {
            if (path == "")
            {
                throw ExceptionUtils.GetArgumentNullException("path");
            }
            if (defaultEncoding == null)
            {
                throw ExceptionUtils.GetArgumentNullException("defaultEncoding");
            }
            FileStream stream = new FileStream(this.ValidatePath(path), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            this.m_Reader = new StreamReader(stream, defaultEncoding, detectEncoding);
            this.ReadToBuffer();
        }

        private void InitializeFromStream(Stream stream, Encoding defaultEncoding, bool detectEncoding)
        {
            if (stream == null)
            {
                throw ExceptionUtils.GetArgumentNullException("stream");
            }
            if (!stream.CanRead)
            {
                throw ExceptionUtils.GetArgumentExceptionWithArgName("stream", "TextFieldParser_StreamNotReadable", new string[] { "stream" });
            }
            if (defaultEncoding == null)
            {
                throw ExceptionUtils.GetArgumentNullException("defaultEncoding");
            }
            this.m_Reader = new StreamReader(stream, defaultEncoding, detectEncoding);
            this.ReadToBuffer();
        }

        private string[] ParseDelimitedLine()
        {
            string line = this.ReadNextDataLine();
            if (line == null)
            {
                return null;
            }
            long lineNumber = this.m_LineNumber - 1L;
            int startat = 0;
            List<string> list = new List<string>();
            int endOfLineIndex = this.GetEndOfLineIndex(line);
            while (startat <= endOfLineIndex)
            {
                string field;
                Match match = null;
                bool success = false;
                if (this.m_HasFieldsEnclosedInQuotes)
                {
                    match = this.BeginQuotesRegex.Match(line, startat);
                    success = match.Success;
                }
                if (success)
                {
                    startat = match.Index + match.Length;
                    QuoteDelimitedFieldBuilder builder = new QuoteDelimitedFieldBuilder(this.m_DelimiterWithEndCharsRegex, this.m_SpaceChars);
                    builder.BuildField(line, startat);
                    if (builder.MalformedLine)
                    {
                        this.m_ErrorLine = line.TrimEnd(new char[] { '\r', '\n' });
                        this.m_ErrorLineNumber = lineNumber;
                        throw new MalformedLineException(Utils.GetResourceString("TextFieldParser_MalFormedDelimitedLine", new string[] { lineNumber.ToString(CultureInfo.InvariantCulture) }), lineNumber);
                    }
                    if (builder.FieldFinished)
                    {
                        field = builder.Field;
                        startat = builder.Index + builder.DelimiterLength;
                    }
                    else
                    {
                        do
                        {
                            int length = line.Length;
                            string str3 = this.ReadNextDataLine();
                            if (str3 == null)
                            {
                                this.m_ErrorLine = line.TrimEnd(new char[] { '\r', '\n' });
                                this.m_ErrorLineNumber = lineNumber;
                                throw new MalformedLineException(Utils.GetResourceString("TextFieldParser_MalFormedDelimitedLine", new string[] { lineNumber.ToString(CultureInfo.InvariantCulture) }), lineNumber);
                            }
                            if ((line.Length + str3.Length) > this.m_MaxLineSize)
                            {
                                this.m_ErrorLine = line.TrimEnd(new char[] { '\r', '\n' });
                                this.m_ErrorLineNumber = lineNumber;
                                throw new MalformedLineException(Utils.GetResourceString("TextFieldParser_MaxLineSizeExceeded", new string[] { lineNumber.ToString(CultureInfo.InvariantCulture) }), lineNumber);
                            }
                            line = line + str3;
                            endOfLineIndex = this.GetEndOfLineIndex(line);
                            builder.BuildField(line, length);
                            if (builder.MalformedLine)
                            {
                                this.m_ErrorLine = line.TrimEnd(new char[] { '\r', '\n' });
                                this.m_ErrorLineNumber = lineNumber;
                                throw new MalformedLineException(Utils.GetResourceString("TextFieldParser_MalFormedDelimitedLine", new string[] { lineNumber.ToString(CultureInfo.InvariantCulture) }), lineNumber);
                            }
                        }
                        while (!builder.FieldFinished);
                        field = builder.Field;
                        startat = builder.Index + builder.DelimiterLength;
                    }
                    if (this.m_TrimWhiteSpace)
                    {
                        field = field.Trim();
                    }
                    list.Add(field);
                }
                else
                {
                    Match match2 = this.m_DelimiterRegex.Match(line, startat);
                    if (match2.Success)
                    {
                        field = line.Substring(startat, match2.Index - startat);
                        if (this.m_TrimWhiteSpace)
                        {
                            field = field.Trim();
                        }
                        list.Add(field);
                        startat = match2.Index + match2.Length;
                    }
                    else
                    {
                        field = line.Substring(startat).TrimEnd(new char[] { '\r', '\n' });
                        if (this.m_TrimWhiteSpace)
                        {
                            field = field.Trim();
                        }
                        list.Add(field);
                        break;
                    }
                }
            }
            return list.ToArray();
        }

        private string[] ParseFixedWidthLine()
        {
            string str = this.ReadNextDataLine();
            if (str == null)
            {
                return null;
            }
            StringInfo line = new StringInfo(str.TrimEnd(new char[] { '\r', '\n' }));
            this.ValidateFixedWidthLine(line, this.m_LineNumber - 1L);
            int index = 0;
            int num = this.m_FieldWidths.Length - 1;
            string[] strArray = new string[num + 1];
            int num4 = num;
            for (int i = 0; i <= num4; i++)
            {
                strArray[i] = this.GetFixedWidthField(line, index, this.m_FieldWidths[i]);
                index += this.m_FieldWidths[i];
            }
            return strArray;
        }

        public string PeekChars(int numberOfChars)
        {
            if (numberOfChars <= 0)
            {
                throw ExceptionUtils.GetArgumentExceptionWithArgName("numberOfChars", "TextFieldParser_NumberOfCharsMustBePositive", new string[] { "numberOfChars" });
            }
            if ((this.m_Reader == null) | (this.m_Buffer == null))
            {
                return null;
            }
            if (this.m_EndOfData)
            {
                return null;
            }
            string str = this.PeekNextDataLine();
            if (str == null)
            {
                this.m_EndOfData = true;
                return null;
            }
            str = str.TrimEnd(new char[] { '\r', '\n' });
            if (str.Length < numberOfChars)
            {
                return str;
            }
            StringInfo info = new StringInfo(str);
            return info.SubstringByTextElements(0, numberOfChars);
        }

        private string PeekNextDataLine()
        {
            string str;
            ChangeBufferFunction changeBuffer = new ChangeBufferFunction(this.IncreaseBufferSize);
            this.SlideCursorToStartOfBuffer();
            this.m_PeekPosition = 0;
            do
            {
                str = this.ReadNextLine(ref this.m_PeekPosition, changeBuffer);
            }
            while (this.IgnoreLine(str));
            return str;
        }

        public string[] ReadFields()
        {
            if (!((this.m_Reader == null) | (this.m_Buffer == null)))
            {
                this.ValidateReadyToRead();
                switch (this.m_TextFieldType)
                {
                    case FieldType.Delimited:
                        return this.ParseDelimitedLine();

                    case FieldType.FixedWidth:
                        return this.ParseFixedWidthLine();
                }
            }
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public string ReadLine()
        {
            if ((this.m_Reader == null) | (this.m_Buffer == null))
            {
                return null;
            }
            ChangeBufferFunction changeBuffer = new ChangeBufferFunction(this.ReadToBuffer);
            string str = this.ReadNextLine(ref this.m_Position, changeBuffer);
            if (str == null)
            {
                this.FinishReading();
                return null;
            }
            this.m_LineNumber += 1L;
            return str.TrimEnd(new char[] { '\r', '\n' });
        }

        private string ReadNextDataLine()
        {
            string str;
            ChangeBufferFunction changeBuffer = new ChangeBufferFunction(this.ReadToBuffer);
            do
            {
                str = this.ReadNextLine(ref this.m_Position, changeBuffer);
                this.m_LineNumber += 1L;
            }
            while (this.IgnoreLine(str));
            if (str == null)
            {
                this.CloseReader();
            }
            return str;
        }

        private string ReadNextLine(ref int Cursor, ChangeBufferFunction ChangeBuffer)
        {
            if ((Cursor == this.m_CharsRead) && (ChangeBuffer() == 0))
            {
                return null;
            }
            StringBuilder builder = null;
            do
            {
                int num3 = this.m_CharsRead - 1;
                for (int i = Cursor; i <= num3; i++)
                {
                    char ch = this.m_Buffer[i];
                    if ((Conversions.ToString(ch) == "\r") | (Conversions.ToString(ch) == "\n"))
                    {
                        if (builder != null)
                        {
                            builder.Append(this.m_Buffer, Cursor, (i - Cursor) + 1);
                        }
                        else
                        {
                            builder = new StringBuilder(i + 1);
                            builder.Append(this.m_Buffer, Cursor, (i - Cursor) + 1);
                        }
                        Cursor = i + 1;
                        if (Conversions.ToString(ch) == "\r")
                        {
                            if (Cursor < this.m_CharsRead)
                            {
                                if (Conversions.ToString(this.m_Buffer[Cursor]) == "\n")
                                {
                                    Cursor++;
                                    builder.Append("\n");
                                }
                            }
                            else if ((ChangeBuffer() > 0) && (Conversions.ToString(this.m_Buffer[Cursor]) == "\n"))
                            {
                                Cursor++;
                                builder.Append("\n");
                            }
                        }
                        return builder.ToString();
                    }
                }
                int charCount = this.m_CharsRead - Cursor;
                if (builder == null)
                {
                    builder = new StringBuilder(charCount + 10);
                }
                builder.Append(this.m_Buffer, Cursor, charCount);
            }
            while (ChangeBuffer() > 0);
            return builder.ToString();
        }

        private int ReadToBuffer()
        {
            this.m_Position = 0;
            int length = this.m_Buffer.Length;
            if (length > 0x1000)
            {
                length = 0x1000;
                this.m_Buffer = new char[(length - 1) + 1];
            }
            this.m_CharsRead = this.m_Reader.Read(this.m_Buffer, 0, length);
            return this.m_CharsRead;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public string ReadToEnd()
        {
            if ((this.m_Reader == null) | (this.m_Buffer == null))
            {
                return null;
            }
            StringBuilder builder = new StringBuilder(this.m_Buffer.Length);
            builder.Append(this.m_Buffer, this.m_Position, this.m_CharsRead - this.m_Position);
            builder.Append(this.m_Reader.ReadToEnd());
            this.FinishReading();
            return builder.ToString();
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void SetDelimiters(params string[] delimiters)
        {
            this.Delimiters = delimiters;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void SetFieldWidths(params int[] fieldWidths)
        {
            this.FieldWidths = fieldWidths;
        }

        private int SlideCursorToStartOfBuffer()
        {
            if (this.m_Position > 0)
            {
                int length = this.m_Buffer.Length;
                char[] destinationArray = new char[(length - 1) + 1];
                Array.Copy(this.m_Buffer, this.m_Position, destinationArray, 0, length - this.m_Position);
                int num3 = this.m_Reader.Read(destinationArray, length - this.m_Position, this.m_Position);
                this.m_CharsRead = (this.m_CharsRead - this.m_Position) + num3;
                this.m_Position = 0;
                this.m_Buffer = destinationArray;
                return num3;
            }
            return 0;
        }

        private void ValidateAndEscapeDelimiters()
        {
            if (this.m_Delimiters == null)
            {
                throw ExceptionUtils.GetArgumentExceptionWithArgName("Delimiters", "TextFieldParser_DelimitersNothing", new string[] { "Delimiters" });
            }
            if (this.m_Delimiters.Length == 0)
            {
                throw ExceptionUtils.GetArgumentExceptionWithArgName("Delimiters", "TextFieldParser_DelimitersNothing", new string[] { "Delimiters" });
            }
            int length = this.m_Delimiters.Length;
            StringBuilder builder = new StringBuilder();
            StringBuilder builder2 = new StringBuilder();
            builder2.Append(this.EndQuotePattern + "(");
            int num3 = length - 1;
            for (int i = 0; i <= num3; i++)
            {
                if (this.m_Delimiters[i] != null)
                {
                    if (this.m_HasFieldsEnclosedInQuotes && (this.m_Delimiters[i].IndexOf('"') > -1))
                    {
                        throw ExceptionUtils.GetInvalidOperationException("TextFieldParser_IllegalDelimiter", new string[0]);
                    }
                    string str = Regex.Escape(this.m_Delimiters[i]);
                    builder.Append(str + "|");
                    builder2.Append(str + "|");
                }
            }
            this.m_SpaceChars = this.WhitespaceCharacters;
            this.m_DelimiterRegex = new Regex(builder.ToString(0, builder.Length - 1), RegexOptions.CultureInvariant);
            builder.Append("\r|\n");
            this.m_DelimiterWithEndCharsRegex = new Regex(builder.ToString(), RegexOptions.CultureInvariant);
            builder2.Append("\r|\n)|\"$");
        }

        private void ValidateDelimiters(string[] delimiterArray)
        {
            if (delimiterArray != null)
            {
                foreach (string str in delimiterArray)
                {
                    if (str == "")
                    {
                        throw ExceptionUtils.GetArgumentExceptionWithArgName("Delimiters", "TextFieldParser_DelimiterNothing", new string[] { "Delimiters" });
                    }
                    if (str.IndexOfAny(new char[] { '\r', '\n' }) > -1)
                    {
                        throw ExceptionUtils.GetArgumentExceptionWithArgName("Delimiters", "TextFieldParser_EndCharsInDelimiter", new string[0]);
                    }
                }
            }
        }

        private void ValidateFieldTypeEnumValue(FieldType value, string paramName)
        {
            if ((value < FieldType.Delimited) || (value > FieldType.FixedWidth))
            {
                throw new InvalidEnumArgumentException(paramName, (int) value, typeof(FieldType));
            }
        }

        private void ValidateFieldWidths()
        {
            if (this.m_FieldWidths == null)
            {
                throw ExceptionUtils.GetInvalidOperationException("TextFieldParser_FieldWidthsNothing", new string[0]);
            }
            if (this.m_FieldWidths.Length == 0)
            {
                throw ExceptionUtils.GetInvalidOperationException("TextFieldParser_FieldWidthsNothing", new string[0]);
            }
            int index = this.m_FieldWidths.Length - 1;
            this.m_LineLength = 0;
            int num3 = index - 1;
            for (int i = 0; i <= num3; i++)
            {
                this.m_LineLength += this.m_FieldWidths[i];
            }
            if (this.m_FieldWidths[index] > 0)
            {
                this.m_LineLength += this.m_FieldWidths[index];
            }
        }

        private void ValidateFieldWidthsOnInput(int[] Widths)
        {
            int num = Widths.Length - 1;
            int num3 = num - 1;
            for (int i = 0; i <= num3; i++)
            {
                if (Widths[i] < 1)
                {
                    throw ExceptionUtils.GetArgumentExceptionWithArgName("FieldWidths", "TextFieldParser_FieldWidthsMustPositive", new string[] { "FieldWidths" });
                }
            }
        }

        private void ValidateFixedWidthLine(StringInfo Line, long LineNumber)
        {
            if (Line.LengthInTextElements < this.m_LineLength)
            {
                this.m_ErrorLine = Line.String;
                this.m_ErrorLineNumber = this.m_LineNumber - 1L;
                throw new MalformedLineException(Utils.GetResourceString("TextFieldParser_MalFormedFixedWidthLine", new string[] { LineNumber.ToString(CultureInfo.InvariantCulture) }), LineNumber);
            }
        }

        private string ValidatePath(string path)
        {
            string str = Microsoft.VisualBasic.FileIO.FileSystem.NormalizeFilePath(path, "path");
            if (!File.Exists(str))
            {
                throw new FileNotFoundException(Utils.GetResourceString("IO_FileNotFound_Path", new string[] { str }));
            }
            return str;
        }

        private void ValidateReadyToRead()
        {
            if (this.m_NeedPropertyCheck | this.ArrayHasChanged())
            {
                switch (this.m_TextFieldType)
                {
                    case FieldType.Delimited:
                        this.ValidateAndEscapeDelimiters();
                        break;

                    case FieldType.FixedWidth:
                        this.ValidateFieldWidths();
                        break;
                }
                if (this.m_CommentTokens != null)
                {
                    foreach (string str in this.m_CommentTokens)
                    {
                        if (((str != "") && (this.m_HasFieldsEnclosedInQuotes & (this.m_TextFieldType == FieldType.Delimited))) && (string.Compare(str.Trim(), "\"", StringComparison.Ordinal) == 0))
                        {
                            throw ExceptionUtils.GetInvalidOperationException("TextFieldParser_InvalidComment", new string[0]);
                        }
                    }
                }
                this.m_NeedPropertyCheck = false;
            }
        }

        private Regex BeginQuotesRegex
        {
            get
            {
                if (this.m_BeginQuotesRegex == null)
                {
                    string pattern = string.Format(CultureInfo.InvariantCulture, "\\G[{0}]*\"", new object[] { this.WhitespacePattern });
                    this.m_BeginQuotesRegex = new Regex(pattern, RegexOptions.CultureInvariant);
                }
                return this.m_BeginQuotesRegex;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public string[] CommentTokens
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_CommentTokens;
            }
            set
            {
                this.CheckCommentTokensForWhitespace(value);
                this.m_CommentTokens = value;
                this.m_NeedPropertyCheck = true;
            }
        }

        public string[] Delimiters
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_Delimiters;
            }
            set
            {
                if (value != null)
                {
                    this.ValidateDelimiters(value);
                    this.m_DelimitersCopy = (string[]) value.Clone();
                }
                else
                {
                    this.m_DelimitersCopy = null;
                }
                this.m_Delimiters = value;
                this.m_NeedPropertyCheck = true;
                this.m_BeginQuotesRegex = null;
            }
        }

        public bool EndOfData
        {
            get
            {
                if (this.m_EndOfData)
                {
                    return this.m_EndOfData;
                }
                if ((this.m_Reader == null) | (this.m_Buffer == null))
                {
                    this.m_EndOfData = true;
                    return true;
                }
                if (this.PeekNextDataLine() != null)
                {
                    return false;
                }
                this.m_EndOfData = true;
                return true;
            }
        }

        private string EndQuotePattern
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "\"[{0}]*", new object[] { this.WhitespacePattern });
            }
        }

        public string ErrorLine
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_ErrorLine;
            }
        }

        public long ErrorLineNumber
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_ErrorLineNumber;
            }
        }

        public int[] FieldWidths
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_FieldWidths;
            }
            set
            {
                if (value != null)
                {
                    this.ValidateFieldWidthsOnInput(value);
                    this.m_FieldWidthsCopy = (int[]) value.Clone();
                }
                else
                {
                    this.m_FieldWidthsCopy = null;
                }
                this.m_FieldWidths = value;
                this.m_NeedPropertyCheck = true;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public bool HasFieldsEnclosedInQuotes
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_HasFieldsEnclosedInQuotes;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.m_HasFieldsEnclosedInQuotes = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public long LineNumber
        {
            get
            {
                if ((this.m_LineNumber != -1L) && ((this.m_Reader.Peek() == -1) & (this.m_Position == this.m_CharsRead)))
                {
                    this.CloseReader();
                }
                return this.m_LineNumber;
            }
        }

        public FieldType TextFieldType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_TextFieldType;
            }
            set
            {
                this.ValidateFieldTypeEnumValue(value, "value");
                this.m_TextFieldType = value;
                this.m_NeedPropertyCheck = true;
            }
        }

        public bool TrimWhiteSpace
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_TrimWhiteSpace;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.m_TrimWhiteSpace = value;
            }
        }

        private string WhitespaceCharacters
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                foreach (int num in this.m_WhitespaceCodes)
                {
                    char testCharacter = Strings.ChrW(num);
                    if (!this.CharacterIsInDelimiter(testCharacter))
                    {
                        builder.Append(testCharacter);
                    }
                }
                return builder.ToString();
            }
        }

        private string WhitespacePattern
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                foreach (int num in this.m_WhitespaceCodes)
                {
                    char testCharacter = Strings.ChrW(num);
                    if (!this.CharacterIsInDelimiter(testCharacter))
                    {
                        builder.Append(@"\u" + num.ToString("X4", CultureInfo.InvariantCulture));
                    }
                }
                return builder.ToString();
            }
        }

        private delegate int ChangeBufferFunction();
    }
}

