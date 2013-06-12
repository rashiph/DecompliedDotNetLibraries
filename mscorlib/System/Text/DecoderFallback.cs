namespace System.Text
{
    using System;
    using System.Threading;

    [Serializable]
    public abstract class DecoderFallback
    {
        internal bool bIsMicrosoftBestFitFallback;
        private static DecoderFallback exceptionFallback;
        private static DecoderFallback replacementFallback;
        private static object s_InternalSyncObject;

        protected DecoderFallback()
        {
        }

        public abstract DecoderFallbackBuffer CreateFallbackBuffer();

        public static DecoderFallback ExceptionFallback
        {
            get
            {
                if (exceptionFallback == null)
                {
                    lock (InternalSyncObject)
                    {
                        if (exceptionFallback == null)
                        {
                            exceptionFallback = new DecoderExceptionFallback();
                        }
                    }
                }
                return exceptionFallback;
            }
        }

        private static object InternalSyncObject
        {
            get
            {
                if (s_InternalSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange<object>(ref s_InternalSyncObject, obj2, null);
                }
                return s_InternalSyncObject;
            }
        }

        internal bool IsMicrosoftBestFitFallback
        {
            get
            {
                return this.bIsMicrosoftBestFitFallback;
            }
        }

        public abstract int MaxCharCount { get; }

        public static DecoderFallback ReplacementFallback
        {
            get
            {
                if (replacementFallback == null)
                {
                    lock (InternalSyncObject)
                    {
                        if (replacementFallback == null)
                        {
                            replacementFallback = new DecoderReplacementFallback();
                        }
                    }
                }
                return replacementFallback;
            }
        }
    }
}

