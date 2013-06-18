namespace System.DirectoryServices
{
    using System;
    using System.ComponentModel;

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class SortOption
    {
        private string propertyName;
        private SortDirection sortDirection;

        public SortOption()
        {
        }

        public SortOption(string propertyName, SortDirection direction)
        {
            this.PropertyName = propertyName;
            this.Direction = this.sortDirection;
        }

        [DSDescription("DSSortDirection"), DefaultValue(0)]
        public SortDirection Direction
        {
            get
            {
                return this.sortDirection;
            }
            set
            {
                if ((value < SortDirection.Ascending) || (value > SortDirection.Descending))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(SortDirection));
                }
                this.sortDirection = value;
            }
        }

        [DSDescription("DSSortName"), DefaultValue((string) null)]
        public string PropertyName
        {
            get
            {
                return this.propertyName;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.propertyName = value;
            }
        }
    }
}

