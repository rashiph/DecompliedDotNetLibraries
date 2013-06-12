namespace System.Web
{
    using System;
    using System.Collections;
    using System.Reflection;

    [Serializable]
    public sealed class ParserErrorCollection : CollectionBase
    {
        public ParserErrorCollection()
        {
        }

        public ParserErrorCollection(ParserError[] value)
        {
            this.AddRange(value);
        }

        public int Add(ParserError value)
        {
            return base.List.Add(value);
        }

        public void AddRange(ParserError[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            for (int i = 0; i < value.Length; i++)
            {
                this.Add(value[i]);
            }
        }

        public void AddRange(ParserErrorCollection value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            foreach (ParserError error in value)
            {
                this.Add(error);
            }
        }

        public bool Contains(ParserError value)
        {
            return base.List.Contains(value);
        }

        public void CopyTo(ParserError[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(ParserError value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, ParserError value)
        {
            base.List.Insert(index, value);
        }

        public void Remove(ParserError value)
        {
            base.List.Remove(value);
        }

        public ParserError this[int index]
        {
            get
            {
                return (ParserError) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

