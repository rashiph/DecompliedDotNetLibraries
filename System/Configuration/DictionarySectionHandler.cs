namespace System.Configuration
{
    using System;
    using System.Collections;
    using System.Xml;

    public class DictionarySectionHandler : IConfigurationSectionHandler
    {
        public virtual object Create(object parent, object context, XmlNode section)
        {
            Hashtable hashtable;
            if (parent == null)
            {
                hashtable = new Hashtable(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                hashtable = (Hashtable) ((Hashtable) parent).Clone();
            }
            HandlerBase.CheckForUnrecognizedAttributes(section);
            foreach (XmlNode node in section.ChildNodes)
            {
                if (!HandlerBase.IsIgnorableAlsoCheckForNonElement(node))
                {
                    if (node.Name == "add")
                    {
                        string str2;
                        HandlerBase.CheckForChildNodes(node);
                        string str = HandlerBase.RemoveRequiredAttribute(node, this.KeyAttributeName);
                        if (this.ValueRequired)
                        {
                            str2 = HandlerBase.RemoveRequiredAttribute(node, this.ValueAttributeName);
                        }
                        else
                        {
                            str2 = HandlerBase.RemoveAttribute(node, this.ValueAttributeName);
                        }
                        HandlerBase.CheckForUnrecognizedAttributes(node);
                        if (str2 == null)
                        {
                            str2 = "";
                        }
                        hashtable[str] = str2;
                    }
                    else if (node.Name == "remove")
                    {
                        HandlerBase.CheckForChildNodes(node);
                        string key = HandlerBase.RemoveRequiredAttribute(node, this.KeyAttributeName);
                        HandlerBase.CheckForUnrecognizedAttributes(node);
                        hashtable.Remove(key);
                    }
                    else if (node.Name.Equals("clear"))
                    {
                        HandlerBase.CheckForChildNodes(node);
                        HandlerBase.CheckForUnrecognizedAttributes(node);
                        hashtable.Clear();
                    }
                    else
                    {
                        HandlerBase.ThrowUnrecognizedElement(node);
                    }
                }
            }
            return hashtable;
        }

        protected virtual string KeyAttributeName
        {
            get
            {
                return "key";
            }
        }

        protected virtual string ValueAttributeName
        {
            get
            {
                return "value";
            }
        }

        internal virtual bool ValueRequired
        {
            get
            {
                return false;
            }
        }
    }
}

