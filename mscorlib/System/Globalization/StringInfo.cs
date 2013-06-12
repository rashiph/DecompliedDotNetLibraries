namespace System.Globalization
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class StringInfo
    {
        [NonSerialized]
        private int[] m_indexes;
        [OptionalField(VersionAdded=2)]
        private string m_str;

        public StringInfo() : this("")
        {
        }

        public StringInfo(string value)
        {
            this.String = value;
        }

        [ComVisible(false)]
        public override bool Equals(object value)
        {
            StringInfo info = value as StringInfo;
            return ((info != null) && this.m_str.Equals(info.m_str));
        }

        internal static int GetCurrentTextElementLen(string str, int index, int len, ref UnicodeCategory ucCurrent, ref int currentCharCount)
        {
            int num;
            if ((index + currentCharCount) == len)
            {
                return currentCharCount;
            }
            UnicodeCategory uc = CharUnicodeInfo.InternalGetUnicodeCategory(str, index + currentCharCount, out num);
            if (((!CharUnicodeInfo.IsCombiningCategory(uc) || CharUnicodeInfo.IsCombiningCategory(ucCurrent)) || ((ucCurrent == UnicodeCategory.Format) || (ucCurrent == UnicodeCategory.Control))) || ((ucCurrent == UnicodeCategory.OtherNotAssigned) || (ucCurrent == UnicodeCategory.Surrogate)))
            {
                int num3 = currentCharCount;
                ucCurrent = uc;
                currentCharCount = num;
                return num3;
            }
            int num2 = index;
            index += currentCharCount + num;
            while (index < len)
            {
                uc = CharUnicodeInfo.InternalGetUnicodeCategory(str, index, out num);
                if (!CharUnicodeInfo.IsCombiningCategory(uc))
                {
                    ucCurrent = uc;
                    currentCharCount = num;
                    break;
                }
                index += num;
            }
            return (index - num2);
        }

        [ComVisible(false)]
        public override int GetHashCode()
        {
            return this.m_str.GetHashCode();
        }

        public static string GetNextTextElement(string str)
        {
            return GetNextTextElement(str, 0);
        }

        [SecuritySafeCritical]
        public static string GetNextTextElement(string str, int index)
        {
            int num2;
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            int length = str.Length;
            if ((index < 0) || (index >= length))
            {
                if (index != length)
                {
                    throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
                }
                return string.Empty;
            }
            UnicodeCategory ucCurrent = CharUnicodeInfo.InternalGetUnicodeCategory(str, index, out num2);
            return str.Substring(index, GetCurrentTextElementLen(str, index, length, ref ucCurrent, ref num2));
        }

        public static TextElementEnumerator GetTextElementEnumerator(string str)
        {
            return GetTextElementEnumerator(str, 0);
        }

        [SecuritySafeCritical]
        public static TextElementEnumerator GetTextElementEnumerator(string str, int index)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            int length = str.Length;
            if ((index < 0) || (index > length))
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            return new TextElementEnumerator(str, index, length);
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            if (this.m_str.Length == 0)
            {
                this.m_indexes = null;
            }
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext ctx)
        {
            this.m_str = string.Empty;
        }

        [SecuritySafeCritical]
        public static int[] ParseCombiningCharacters(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            int length = str.Length;
            int[] sourceArray = new int[length];
            if (length != 0)
            {
                int num4;
                int num2 = 0;
                int index = 0;
                UnicodeCategory ucCurrent = CharUnicodeInfo.InternalGetUnicodeCategory(str, 0, out num4);
                while (index < length)
                {
                    sourceArray[num2++] = index;
                    index += GetCurrentTextElementLen(str, index, length, ref ucCurrent, ref num4);
                }
                if (num2 < length)
                {
                    int[] destinationArray = new int[num2];
                    Array.Copy(sourceArray, destinationArray, num2);
                    return destinationArray;
                }
            }
            return sourceArray;
        }

        public string SubstringByTextElements(int startingTextElement)
        {
            if (this.Indexes != null)
            {
                return this.SubstringByTextElements(startingTextElement, this.Indexes.Length - startingTextElement);
            }
            if (startingTextElement < 0)
            {
                throw new ArgumentOutOfRangeException("startingTextElement", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            }
            throw new ArgumentOutOfRangeException("startingTextElement", Environment.GetResourceString("Arg_ArgumentOutOfRangeException"));
        }

        public string SubstringByTextElements(int startingTextElement, int lengthInTextElements)
        {
            if (startingTextElement < 0)
            {
                throw new ArgumentOutOfRangeException("startingTextElement", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            }
            if ((this.String.Length == 0) || (startingTextElement >= this.Indexes.Length))
            {
                throw new ArgumentOutOfRangeException("startingTextElement", Environment.GetResourceString("Arg_ArgumentOutOfRangeException"));
            }
            if (lengthInTextElements < 0)
            {
                throw new ArgumentOutOfRangeException("lengthInTextElements", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            }
            if (startingTextElement > (this.Indexes.Length - lengthInTextElements))
            {
                throw new ArgumentOutOfRangeException("lengthInTextElements", Environment.GetResourceString("Arg_ArgumentOutOfRangeException"));
            }
            int startIndex = this.Indexes[startingTextElement];
            if ((startingTextElement + lengthInTextElements) == this.Indexes.Length)
            {
                return this.String.Substring(startIndex);
            }
            return this.String.Substring(startIndex, this.Indexes[lengthInTextElements + startingTextElement] - startIndex);
        }

        private int[] Indexes
        {
            get
            {
                if ((this.m_indexes == null) && (0 < this.String.Length))
                {
                    this.m_indexes = ParseCombiningCharacters(this.String);
                }
                return this.m_indexes;
            }
        }

        public int LengthInTextElements
        {
            get
            {
                if (this.Indexes == null)
                {
                    return 0;
                }
                return this.Indexes.Length;
            }
        }

        public string String
        {
            get
            {
                return this.m_str;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("String", Environment.GetResourceString("ArgumentNull_String"));
                }
                this.m_str = value;
                this.m_indexes = null;
            }
        }
    }
}

