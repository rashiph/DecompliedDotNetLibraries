namespace System
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;

    internal class ConfigNode
    {
        private List<DictionaryEntry> m_attributes = new List<DictionaryEntry>(5);
        private List<ConfigNode> m_children = new List<ConfigNode>(5);
        private string m_name;
        private ConfigNode m_parent;
        private string m_value;

        internal ConfigNode(string name, ConfigNode parent)
        {
            this.m_name = name;
            this.m_parent = parent;
        }

        internal int AddAttribute(string key, string value)
        {
            this.m_attributes.Add(new DictionaryEntry(key, value));
            return (this.m_attributes.Count - 1);
        }

        internal void AddChild(ConfigNode child)
        {
            child.m_parent = this;
            this.m_children.Add(child);
        }

        internal void ReplaceAttribute(int index, string key, string value)
        {
            this.m_attributes[index] = new DictionaryEntry(key, value);
        }

        [Conditional("_LOGGING")]
        internal void Trace()
        {
            string text1 = this.m_value;
            ConfigNode parent = this.m_parent;
            for (int i = 0; i < this.m_attributes.Count; i++)
            {
                DictionaryEntry local1 = this.m_attributes[i];
            }
            for (int j = 0; j < this.m_children.Count; j++)
            {
            }
        }

        internal List<DictionaryEntry> Attributes
        {
            get
            {
                return this.m_attributes;
            }
        }

        internal List<ConfigNode> Children
        {
            get
            {
                return this.m_children;
            }
        }

        internal string Name
        {
            get
            {
                return this.m_name;
            }
        }

        internal ConfigNode Parent
        {
            get
            {
                return this.m_parent;
            }
        }

        internal string Value
        {
            get
            {
                return this.m_value;
            }
            set
            {
                this.m_value = value;
            }
        }
    }
}

