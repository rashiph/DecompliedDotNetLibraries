namespace System.ComponentModel.Design.Data
{
    using System;
    using System.Drawing;

    public abstract class DataSourceDescriptor
    {
        protected DataSourceDescriptor()
        {
        }

        public abstract Bitmap Image { get; }

        public abstract bool IsDesignable { get; }

        public abstract string Name { get; }

        public abstract string TypeName { get; }
    }
}

