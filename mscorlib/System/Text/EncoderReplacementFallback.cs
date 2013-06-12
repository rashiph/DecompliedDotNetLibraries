namespace System.Text
{
    using System;
    using System.Runtime;

    [Serializable]
    public sealed class EncoderReplacementFallback : EncoderFallback
    {
        private string strDefault;

        public EncoderReplacementFallback() : this("?")
        {
        }

        public EncoderReplacementFallback(string replacement)
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

        public override EncoderFallbackBuffer CreateFallbackBuffer()
        {
            return new EncoderReplacementFallbackBuffer(this);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override bool Equals(object value)
        {
            EncoderReplacementFallback fallback = value as EncoderReplacementFallback;
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
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.strDefault.Length;
            }
        }
    }
}

