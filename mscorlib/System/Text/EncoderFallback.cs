namespace System.Text
{
    using System;
    using System.Threading;

    [Serializable]
    public abstract class EncoderFallback
    {
        internal bool bIsMicrosoftBestFitFallback;
        private static EncoderFallback exceptionFallback;
        private static EncoderFallback replacementFallback;
        private static object s_InternalSyncObject;

        protected EncoderFallback()
        {
        }

        public abstract EncoderFallbackBuffer CreateFallbackBuffer();

        public static EncoderFallback ExceptionFallback
        {
            get
            {
                if (exceptionFallback == null)
                {
                    lock (InternalSyncObject)
                    {
                        if (exceptionFallback == null)
                        {
                            exceptionFallback = new EncoderExceptionFallback();
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

        public abstract int MaxCharCount { get; }

        public static EncoderFallback ReplacementFallback
        {
            get
            {
                if (replacementFallback == null)
                {
                    lock (InternalSyncObject)
                    {
                        if (replacementFallback == null)
                        {
                            replacementFallback = new EncoderReplacementFallback();
                        }
                    }
                }
                return replacementFallback;
            }
        }
    }
}

