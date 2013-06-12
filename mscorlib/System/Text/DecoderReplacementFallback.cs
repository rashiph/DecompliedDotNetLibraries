namespace System.Text
{
    using System;

    [Serializable]
    public sealed class DecoderReplacementFallback : DecoderFallback
    {
        private string strDefault;

        public DecoderReplacementFallback() : this("?")
        {
        }

        public DecoderReplacementFallback(string replacement)
        {
            if (replacement == null)
            {
                throw new ArgumentNullException("replacement");
            }
            bool flag = false;
            for (int i = 0; i < replacement.Length; i++)
            {
                if (char.IsSurrogate(replacement, i))
                {
                    if (char.IsHighSurrogate(replacement, i))
                    {
                        if (flag)
                        {
                            break;
                        }
                        flag = true;
                    }
                    else
                    {
                        if (!flag)
                        {
                            flag = true;
                            break;
                        }
                        flag = false;
                    }
                }
                else if (flag)
                {
                    break;
                }
            }
            if (flag)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidCharSequenceNoIndex", new object[] { "replacement" }));
            }
            this.strDefault = replacement;
        }

        public override DecoderFallbackBuffer CreateFallbackBuffer()
        {
            return new DecoderReplacementFallbackBuffer(this);
        }

        public override bool Equals(object value)
        {
            DecoderReplacementFallback fallback = value as DecoderReplacementFallback;
            return ((fallback != null) && (this.strDefault == fallback.strDefault));
        }

        public override int GetHashCode()
        {
            return this.strDefault.GetHashCode();
        }

        public string DefaultString
        {
            get
            {
                return this.strDefault;
            }
        }

        public override int MaxCharCount
        {
            get
            {
                return this.strDefault.Length;
            }
        }
    }
}

