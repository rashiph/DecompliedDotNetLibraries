namespace System.Windows.Forms
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DataGridViewColumnDesignTimeVisibleAttribute : Attribute
    {
        public static readonly DataGridViewColumnDesignTimeVisibleAttribute Default = Yes;
        public static readonly DataGridViewColumnDesignTimeVisibleAttribute No = new DataGridViewColumnDesignTimeVisibleAttribute(false);
        private bool visible;
        public static readonly DataGridViewColumnDesignTimeVisibleAttribute Yes = new DataGridViewColumnDesignTimeVisibleAttribute(true);

        public DataGridViewColumnDesignTimeVisibleAttribute()
        {
        }

        public DataGridViewColumnDesignTimeVisibleAttribute(bool visible)
        {
            this.visible = visible;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            DataGridViewColumnDesignTimeVisibleAttribute attribute = obj as DataGridViewColumnDesignTimeVisibleAttribute;
            return ((attribute != null) && (attribute.Visible == this.visible));
        }

        public override int GetHashCode()
        {
            return (typeof(DataGridViewColumnDesignTimeVisibleAttribute).GetHashCode() ^ (this.visible ? -1 : 0));
        }

        public override bool IsDefaultAttribute()
        {
            return (this.Visible == Default.Visible);
        }

        public bool Visible
        {
            get
            {
                return this.visible;
            }
        }
    }
}

