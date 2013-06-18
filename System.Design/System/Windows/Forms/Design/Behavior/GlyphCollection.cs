namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class GlyphCollection : CollectionBase
    {
        public GlyphCollection()
        {
        }

        public GlyphCollection(GlyphCollection value)
        {
            this.AddRange(value);
        }

        public GlyphCollection(Glyph[] value)
        {
            this.AddRange(value);
        }

        public int Add(Glyph value)
        {
            return base.List.Add(value);
        }

        public void AddRange(Glyph[] value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                this.Add(value[i]);
            }
        }

        public void AddRange(GlyphCollection value)
        {
            for (int i = 0; i < value.Count; i++)
            {
                this.Add(value[i]);
            }
        }

        public bool Contains(Glyph value)
        {
            return base.List.Contains(value);
        }

        public void CopyTo(Glyph[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(Glyph value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, Glyph value)
        {
            base.List.Insert(index, value);
        }

        public void Remove(Glyph value)
        {
            base.List.Remove(value);
        }

        public Glyph this[int index]
        {
            get
            {
                return (Glyph) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

