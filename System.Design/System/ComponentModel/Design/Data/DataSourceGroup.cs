namespace System.ComponentModel.Design.Data
{
    using System;
    using System.Drawing;

    public abstract class DataSourceGroup
    {
        protected DataSourceGroup()
        {
        }

        public abstract DataSourceDescriptorCollection DataSources { get; }

        public abstract Bitmap Image { get; }

        public abstract bool IsDefault { get; }

        public abstract string Name { get; }
    }
}

