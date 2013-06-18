namespace System.Data.OleDb
{
    using System;
    using System.Data.ProviderBase;
    using System.Runtime.CompilerServices;

    internal sealed class ChapterHandle : WrappedIUnknown
    {
        private IntPtr _chapterHandle;
        internal static readonly ChapterHandle DB_NULL_HCHAPTER = new ChapterHandle(IntPtr.Zero);

        private ChapterHandle(IntPtr chapter) : base(null)
        {
            this._chapterHandle = chapter;
        }

        private ChapterHandle(object chapteredRowset, RowBinding binding, int valueOffset) : base(chapteredRowset)
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                this._chapterHandle = binding.InterlockedExchangePointer(valueOffset);
            }
        }

        internal static ChapterHandle CreateChapterHandle(IntPtr chapter)
        {
            if (IntPtr.Zero == chapter)
            {
                return DB_NULL_HCHAPTER;
            }
            return new ChapterHandle(chapter);
        }

        internal static ChapterHandle CreateChapterHandle(object chapteredRowset, RowBinding binding, int valueOffset)
        {
            if ((chapteredRowset != null) && !(IntPtr.Zero == binding.ReadIntPtr(valueOffset)))
            {
                return new ChapterHandle(chapteredRowset, binding, valueOffset);
            }
            return DB_NULL_HCHAPTER;
        }

        protected override bool ReleaseHandle()
        {
            IntPtr ptr = this._chapterHandle;
            this._chapterHandle = IntPtr.Zero;
            if ((IntPtr.Zero != base.handle) && (IntPtr.Zero != ptr))
            {
                Bid.Trace("<oledb.IChapteredRowset.ReleaseChapter|API|OLEDB> Chapter=%Id\n", ptr);
                OleDbHResult result = NativeOledbWrapper.IChapteredRowsetReleaseChapter(base.handle, ptr);
                Bid.Trace("<oledb.IChapteredRowset.ReleaseChapter|API|OLEDB|RET> %08X{HRESULT}\n", result);
            }
            return base.ReleaseHandle();
        }

        internal IntPtr HChapter
        {
            get
            {
                return this._chapterHandle;
            }
        }
    }
}

