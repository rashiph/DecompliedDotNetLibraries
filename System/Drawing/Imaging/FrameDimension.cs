namespace System.Drawing.Imaging
{
    using System;

    public sealed class FrameDimension
    {
        private System.Guid guid;
        private static FrameDimension page = new FrameDimension(new System.Guid("{7462dc86-6180-4c7e-8e3f-ee7333a7a483}"));
        private static FrameDimension resolution = new FrameDimension(new System.Guid("{84236f7b-3bd3-428f-8dab-4ea1439ca315}"));
        private static FrameDimension time = new FrameDimension(new System.Guid("{6aedbd6d-3fb5-418a-83a6-7f45229dc872}"));

        public FrameDimension(System.Guid guid)
        {
            this.guid = guid;
        }

        public override bool Equals(object o)
        {
            FrameDimension dimension = o as FrameDimension;
            if (dimension == null)
            {
                return false;
            }
            return (this.guid == dimension.guid);
        }

        public override int GetHashCode()
        {
            return this.guid.GetHashCode();
        }

        public override string ToString()
        {
            if (this == time)
            {
                return "Time";
            }
            if (this == resolution)
            {
                return "Resolution";
            }
            if (this == page)
            {
                return "Page";
            }
            return ("[FrameDimension: " + this.guid + "]");
        }

        public System.Guid Guid
        {
            get
            {
                return this.guid;
            }
        }

        public static FrameDimension Page
        {
            get
            {
                return page;
            }
        }

        public static FrameDimension Resolution
        {
            get
            {
                return resolution;
            }
        }

        public static FrameDimension Time
        {
            get
            {
                return time;
            }
        }
    }
}

