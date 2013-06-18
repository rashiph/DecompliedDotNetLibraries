namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Web.Util;

    public sealed class AttributeCollection
    {
        private StateBag _bag;
        private CssStyleCollection _styleColl;

        public AttributeCollection(StateBag bag)
        {
            this._bag = bag;
        }

        public void Add(string key, string value)
        {
            if ((this._styleColl != null) && StringUtil.EqualsIgnoreCase(key, "style"))
            {
                this._styleColl.Value = value;
            }
            else
            {
                this._bag[key] = value;
            }
        }

        public void AddAttributes(HtmlTextWriter writer)
        {
            if (this._bag.Count > 0)
            {
                IDictionaryEnumerator enumerator = this._bag.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    StateItem item = enumerator.Value as StateItem;
                    if (item != null)
                    {
                        string str = item.Value as string;
                        string key = enumerator.Key as string;
                        if ((key != null) && (str != null))
                        {
                            writer.AddAttribute(key, str, true);
                        }
                    }
                }
            }
        }

        public void Clear()
        {
            this._bag.Clear();
            if (this._styleColl != null)
            {
                this._styleColl.Clear();
            }
        }

        public override bool Equals(object o)
        {
            AttributeCollection attributes = o as AttributeCollection;
            if (attributes == null)
            {
                return false;
            }
            if (attributes.Count != this._bag.Count)
            {
                return false;
            }
            foreach (DictionaryEntry entry in this._bag)
            {
                if (this[(string) entry.Key] != attributes[(string) entry.Key])
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            HashCodeCombiner combiner = new HashCodeCombiner();
            foreach (DictionaryEntry entry in this._bag)
            {
                combiner.AddObject(entry.Key);
                combiner.AddObject(entry.Value);
            }
            return combiner.CombinedHash32;
        }

        public void Remove(string key)
        {
            if ((this._styleColl != null) && StringUtil.EqualsIgnoreCase(key, "style"))
            {
                this._styleColl.Clear();
            }
            else
            {
                this._bag.Remove(key);
            }
        }

        public void Render(HtmlTextWriter writer)
        {
            if (this._bag.Count > 0)
            {
                IDictionaryEnumerator enumerator = this._bag.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    StateItem item = enumerator.Value as StateItem;
                    if (item != null)
                    {
                        string str = item.Value as string;
                        string key = enumerator.Key as string;
                        if ((key != null) && (str != null))
                        {
                            writer.WriteAttribute(key, str, true);
                        }
                    }
                }
            }
        }

        public int Count
        {
            get
            {
                return this._bag.Count;
            }
        }

        public CssStyleCollection CssStyle
        {
            get
            {
                if (this._styleColl == null)
                {
                    this._styleColl = new CssStyleCollection(this._bag);
                }
                return this._styleColl;
            }
        }

        public string this[string key]
        {
            get
            {
                if ((this._styleColl != null) && StringUtil.EqualsIgnoreCase(key, "style"))
                {
                    return this._styleColl.Value;
                }
                return (this._bag[key] as string);
            }
            set
            {
                this.Add(key, value);
            }
        }

        public ICollection Keys
        {
            get
            {
                return this._bag.Keys;
            }
        }
    }
}

