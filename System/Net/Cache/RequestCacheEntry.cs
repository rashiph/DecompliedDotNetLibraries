namespace System.Net.Cache
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Text;

    internal class RequestCacheEntry
    {
        private StringCollection m_EntryMetadata;
        private DateTime m_ExpiresUtc;
        private int m_HitCount;
        private bool m_IsPartialEntry;
        private bool m_IsPrivateEntry;
        private DateTime m_LastAccessedUtc;
        private DateTime m_LastModifiedUtc;
        private DateTime m_LastSynchronizedUtc;
        private TimeSpan m_MaxStale;
        private long m_StreamSize;
        private StringCollection m_SystemMetadata;
        private int m_UsageCount;

        internal RequestCacheEntry()
        {
            this.m_ExpiresUtc = this.m_LastAccessedUtc = this.m_LastModifiedUtc = this.m_LastSynchronizedUtc = DateTime.MinValue;
        }

        internal RequestCacheEntry(_WinInetCache.Entry entry, bool isPrivateEntry)
        {
            this.m_IsPrivateEntry = isPrivateEntry;
            this.m_StreamSize = (entry.Info.SizeHigh << 0x20) | entry.Info.SizeLow;
            this.m_ExpiresUtc = entry.Info.ExpireTime.IsNull ? DateTime.MinValue : DateTime.FromFileTimeUtc(entry.Info.ExpireTime.ToLong());
            this.m_HitCount = entry.Info.HitRate;
            this.m_LastAccessedUtc = entry.Info.LastAccessTime.IsNull ? DateTime.MinValue : DateTime.FromFileTimeUtc(entry.Info.LastAccessTime.ToLong());
            this.m_LastModifiedUtc = entry.Info.LastModifiedTime.IsNull ? DateTime.MinValue : DateTime.FromFileTimeUtc(entry.Info.LastModifiedTime.ToLong());
            this.m_LastSynchronizedUtc = entry.Info.LastSyncTime.IsNull ? DateTime.MinValue : DateTime.FromFileTimeUtc(entry.Info.LastSyncTime.ToLong());
            this.m_MaxStale = TimeSpan.FromSeconds((double) entry.Info.U.ExemptDelta);
            if (this.m_MaxStale == WinInetCache.s_MaxTimeSpanForInt32)
            {
                this.m_MaxStale = TimeSpan.MaxValue;
            }
            this.m_UsageCount = entry.Info.UseCount;
            this.m_IsPartialEntry = (entry.Info.EntryType & _WinInetCache.EntryType.Sparse) != 0;
        }

        internal virtual string ToString(bool verbose)
        {
            StringBuilder builder = new StringBuilder(0x200);
            builder.Append("\r\nIsPrivateEntry   = ").Append(this.IsPrivateEntry);
            builder.Append("\r\nIsPartialEntry   = ").Append(this.IsPartialEntry);
            builder.Append("\r\nStreamSize       = ").Append(this.StreamSize);
            builder.Append("\r\nExpires          = ").Append((this.ExpiresUtc == DateTime.MinValue) ? "" : this.ExpiresUtc.ToString("r", CultureInfo.CurrentCulture));
            builder.Append("\r\nLastAccessed     = ").Append((this.LastAccessedUtc == DateTime.MinValue) ? "" : this.LastAccessedUtc.ToString("r", CultureInfo.CurrentCulture));
            builder.Append("\r\nLastModified     = ").Append((this.LastModifiedUtc == DateTime.MinValue) ? "" : this.LastModifiedUtc.ToString("r", CultureInfo.CurrentCulture));
            builder.Append("\r\nLastSynchronized = ").Append((this.LastSynchronizedUtc == DateTime.MinValue) ? "" : this.LastSynchronizedUtc.ToString("r", CultureInfo.CurrentCulture));
            builder.Append("\r\nMaxStale(sec)    = ").Append((this.MaxStale == TimeSpan.MinValue) ? "" : ((int) this.MaxStale.TotalSeconds).ToString(NumberFormatInfo.CurrentInfo));
            builder.Append("\r\nHitCount         = ").Append(this.HitCount.ToString(NumberFormatInfo.CurrentInfo));
            builder.Append("\r\nUsageCount       = ").Append(this.UsageCount.ToString(NumberFormatInfo.CurrentInfo));
            builder.Append("\r\n");
            if (verbose)
            {
                builder.Append("EntryMetadata:\r\n");
                if (this.m_EntryMetadata != null)
                {
                    foreach (string str in this.m_EntryMetadata)
                    {
                        builder.Append(str).Append("\r\n");
                    }
                }
                builder.Append("---\r\nSystemMetadata:\r\n");
                if (this.m_SystemMetadata != null)
                {
                    foreach (string str2 in this.m_SystemMetadata)
                    {
                        builder.Append(str2).Append("\r\n");
                    }
                }
            }
            return builder.ToString();
        }

        internal StringCollection EntryMetadata
        {
            get
            {
                return this.m_EntryMetadata;
            }
            set
            {
                this.m_EntryMetadata = value;
            }
        }

        internal DateTime ExpiresUtc
        {
            get
            {
                return this.m_ExpiresUtc;
            }
            set
            {
                this.m_ExpiresUtc = value;
            }
        }

        internal int HitCount
        {
            get
            {
                return this.m_HitCount;
            }
            set
            {
                this.m_HitCount = value;
            }
        }

        internal bool IsPartialEntry
        {
            get
            {
                return this.m_IsPartialEntry;
            }
            set
            {
                this.m_IsPartialEntry = value;
            }
        }

        internal bool IsPrivateEntry
        {
            get
            {
                return this.m_IsPrivateEntry;
            }
            set
            {
                this.m_IsPrivateEntry = value;
            }
        }

        internal DateTime LastAccessedUtc
        {
            get
            {
                return this.m_LastAccessedUtc;
            }
            set
            {
                this.m_LastAccessedUtc = value;
            }
        }

        internal DateTime LastModifiedUtc
        {
            get
            {
                return this.m_LastModifiedUtc;
            }
            set
            {
                this.m_LastModifiedUtc = value;
            }
        }

        internal DateTime LastSynchronizedUtc
        {
            get
            {
                return this.m_LastSynchronizedUtc;
            }
            set
            {
                this.m_LastSynchronizedUtc = value;
            }
        }

        internal TimeSpan MaxStale
        {
            get
            {
                return this.m_MaxStale;
            }
            set
            {
                this.m_MaxStale = value;
            }
        }

        internal long StreamSize
        {
            get
            {
                return this.m_StreamSize;
            }
            set
            {
                this.m_StreamSize = value;
            }
        }

        internal StringCollection SystemMetadata
        {
            get
            {
                return this.m_SystemMetadata;
            }
            set
            {
                this.m_SystemMetadata = value;
            }
        }

        internal int UsageCount
        {
            get
            {
                return this.m_UsageCount;
            }
            set
            {
                this.m_UsageCount = value;
            }
        }
    }
}

