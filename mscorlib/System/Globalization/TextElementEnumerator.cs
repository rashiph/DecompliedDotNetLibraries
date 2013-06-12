namespace System.Globalization
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class TextElementEnumerator : IEnumerator
    {
        [OptionalField(VersionAdded=2)]
        private int charLen;
        [NonSerialized]
        private int currTextElementLen;
        private int endIndex;
        private int index;
        private int nextTextElementLen;
        private int startIndex;
        private string str;
        [NonSerialized]
        private int strLen;
        [OptionalField(VersionAdded=2)]
        private UnicodeCategory uc;

        internal TextElementEnumerator(string str, int startIndex, int strLen)
        {
            this.str = str;
            this.startIndex = startIndex;
            this.strLen = strLen;
            this.Reset();
        }

        public string GetTextElement()
        {
            if (this.index == this.startIndex)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumNotStarted"));
            }
            if (this.index > this.strLen)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumEnded"));
            }
            return this.str.Substring(this.index - this.currTextElementLen, this.currTextElementLen);
        }

        [SecuritySafeCritical]
        public bool MoveNext()
        {
            if (this.index >= this.strLen)
            {
                this.index = this.strLen + 1;
                return false;
            }
            this.currTextElementLen = StringInfo.GetCurrentTextElementLen(this.str, this.index, this.strLen, ref this.uc, ref this.charLen);
            this.index += this.currTextElementLen;
            return true;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            this.strLen = this.endIndex + 1;
            this.currTextElementLen = this.nextTextElementLen;
            if (this.charLen == -1)
            {
                this.uc = CharUnicodeInfo.InternalGetUnicodeCategory(this.str, this.index, out this.charLen);
            }
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext ctx)
        {
            this.charLen = -1;
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext ctx)
        {
            this.endIndex = this.strLen - 1;
            this.nextTextElementLen = this.currTextElementLen;
        }

        [SecuritySafeCritical]
        public void Reset()
        {
            this.index = this.startIndex;
            if (this.index < this.strLen)
            {
                this.uc = CharUnicodeInfo.InternalGetUnicodeCategory(this.str, this.index, out this.charLen);
            }
        }

        public object Current
        {
            get
            {
                return this.GetTextElement();
            }
        }

        public int ElementIndex
        {
            get
            {
                if (this.index == this.startIndex)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumNotStarted"));
                }
                return (this.index - this.currTextElementLen);
            }
        }
    }
}

