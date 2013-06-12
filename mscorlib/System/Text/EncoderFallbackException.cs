namespace System.Text
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class EncoderFallbackException : ArgumentException
    {
        private char charUnknown;
        private char charUnknownHigh;
        private char charUnknownLow;
        private int index;

        public EncoderFallbackException() : base(Environment.GetResourceString("Arg_ArgumentException"))
        {
            base.SetErrorCode(-2147024809);
        }

        public EncoderFallbackException(string message) : base(message)
        {
            base.SetErrorCode(-2147024809);
        }

        internal EncoderFallbackException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public EncoderFallbackException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2147024809);
        }

        internal EncoderFallbackException(string message, char charUnknown, int index) : base(message)
        {
            this.charUnknown = charUnknown;
            this.index = index;
        }

        internal EncoderFallbackException(string message, char charUnknownHigh, char charUnknownLow, int index) : base(message)
        {
            if (!char.IsHighSurrogate(charUnknownHigh))
            {
                throw new ArgumentOutOfRangeException("charUnknownHigh", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { 0xd800, 0xdbff }));
            }
            if (!char.IsLowSurrogate(charUnknownLow))
            {
                throw new ArgumentOutOfRangeException("CharUnknownLow", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { 0xdc00, 0xdfff }));
            }
            this.charUnknownHigh = charUnknownHigh;
            this.charUnknownLow = charUnknownLow;
            this.index = index;
        }

        public bool IsUnknownSurrogate()
        {
            return (this.charUnknownHigh != '\0');
        }

        public char CharUnknown
        {
            get
            {
                return this.charUnknown;
            }
        }

        public char CharUnknownHigh
        {
            get
            {
                return this.charUnknownHigh;
            }
        }

        public char CharUnknownLow
        {
            get
            {
                return this.charUnknownLow;
            }
        }

        public int Index
        {
            get
            {
                return this.index;
            }
        }
    }
}

