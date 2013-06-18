namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Xml;

    internal class DSMLFilterWriter
    {
        protected void WriteAttrib(string attrName, ADAttribute attrib, XmlWriter mXmlWriter, string strNamespace)
        {
            if (strNamespace != null)
            {
                mXmlWriter.WriteStartElement(attrName, strNamespace);
            }
            else
            {
                mXmlWriter.WriteStartElement(attrName);
            }
            mXmlWriter.WriteAttributeString("name", attrib.Name);
            foreach (ADValue value2 in attrib.Values)
            {
                this.WriteValue("value", value2, mXmlWriter, strNamespace);
            }
            mXmlWriter.WriteEndElement();
        }

        public void WriteFilter(ADFilter filter, bool filterTags, XmlWriter mXmlWriter, string strNamespace)
        {
            if (filterTags)
            {
                if (strNamespace != null)
                {
                    mXmlWriter.WriteStartElement("filter", strNamespace);
                }
                else
                {
                    mXmlWriter.WriteStartElement("filter");
                }
            }
            switch (filter.Type)
            {
                case ADFilter.FilterType.And:
                    if (strNamespace == null)
                    {
                        mXmlWriter.WriteStartElement("and");
                        break;
                    }
                    mXmlWriter.WriteStartElement("and", strNamespace);
                    break;

                case ADFilter.FilterType.Or:
                    if (strNamespace == null)
                    {
                        mXmlWriter.WriteStartElement("or");
                    }
                    else
                    {
                        mXmlWriter.WriteStartElement("or", strNamespace);
                    }
                    foreach (object obj3 in filter.Filter.Or)
                    {
                        this.WriteFilter((ADFilter) obj3, false, mXmlWriter, strNamespace);
                    }
                    mXmlWriter.WriteEndElement();
                    goto Label_03E0;

                case ADFilter.FilterType.Not:
                    if (strNamespace == null)
                    {
                        mXmlWriter.WriteStartElement("not");
                    }
                    else
                    {
                        mXmlWriter.WriteStartElement("not", strNamespace);
                    }
                    this.WriteFilter(filter.Filter.Not, false, mXmlWriter, strNamespace);
                    mXmlWriter.WriteEndElement();
                    goto Label_03E0;

                case ADFilter.FilterType.EqualityMatch:
                    this.WriteAttrib("equalityMatch", filter.Filter.EqualityMatch, mXmlWriter, strNamespace);
                    goto Label_03E0;

                case ADFilter.FilterType.Substrings:
                {
                    ADSubstringFilter substrings = filter.Filter.Substrings;
                    if (strNamespace == null)
                    {
                        mXmlWriter.WriteStartElement("substrings");
                    }
                    else
                    {
                        mXmlWriter.WriteStartElement("substrings", strNamespace);
                    }
                    mXmlWriter.WriteAttributeString("name", substrings.Name);
                    if (substrings.Initial != null)
                    {
                        this.WriteValue("initial", substrings.Initial, mXmlWriter, strNamespace);
                    }
                    if (substrings.Any != null)
                    {
                        foreach (object obj4 in substrings.Any)
                        {
                            this.WriteValue("any", (ADValue) obj4, mXmlWriter, strNamespace);
                        }
                    }
                    if (substrings.Final != null)
                    {
                        this.WriteValue("final", substrings.Final, mXmlWriter, strNamespace);
                    }
                    mXmlWriter.WriteEndElement();
                    goto Label_03E0;
                }
                case ADFilter.FilterType.GreaterOrEqual:
                    this.WriteAttrib("greaterOrEqual", filter.Filter.GreaterOrEqual, mXmlWriter, strNamespace);
                    goto Label_03E0;

                case ADFilter.FilterType.LessOrEqual:
                    this.WriteAttrib("lessOrEqual", filter.Filter.LessOrEqual, mXmlWriter, strNamespace);
                    goto Label_03E0;

                case ADFilter.FilterType.Present:
                    if (strNamespace == null)
                    {
                        mXmlWriter.WriteStartElement("present");
                    }
                    else
                    {
                        mXmlWriter.WriteStartElement("present", strNamespace);
                    }
                    mXmlWriter.WriteAttributeString("name", filter.Filter.Present);
                    mXmlWriter.WriteEndElement();
                    goto Label_03E0;

                case ADFilter.FilterType.ApproxMatch:
                    this.WriteAttrib("approxMatch", filter.Filter.ApproxMatch, mXmlWriter, strNamespace);
                    goto Label_03E0;

                case ADFilter.FilterType.ExtensibleMatch:
                {
                    ADExtenMatchFilter extensibleMatch = filter.Filter.ExtensibleMatch;
                    if (strNamespace == null)
                    {
                        mXmlWriter.WriteStartElement("extensibleMatch");
                    }
                    else
                    {
                        mXmlWriter.WriteStartElement("extensibleMatch", strNamespace);
                    }
                    if ((extensibleMatch.Name != null) && (extensibleMatch.Name.Length != 0))
                    {
                        mXmlWriter.WriteAttributeString("name", extensibleMatch.Name);
                    }
                    if ((extensibleMatch.MatchingRule != null) && (extensibleMatch.MatchingRule.Length != 0))
                    {
                        mXmlWriter.WriteAttributeString("matchingRule", extensibleMatch.MatchingRule);
                    }
                    mXmlWriter.WriteAttributeString("dnAttributes", XmlConvert.ToString(extensibleMatch.DNAttributes));
                    this.WriteValue("value", extensibleMatch.Value, mXmlWriter, strNamespace);
                    mXmlWriter.WriteEndElement();
                    goto Label_03E0;
                }
                default:
                    throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("InvalidFilterType", new object[] { filter.Type }));
            }
            foreach (object obj2 in filter.Filter.And)
            {
                this.WriteFilter((ADFilter) obj2, false, mXmlWriter, strNamespace);
            }
            mXmlWriter.WriteEndElement();
        Label_03E0:
            if (filterTags)
            {
                mXmlWriter.WriteEndElement();
            }
        }

        protected void WriteValue(string valueElt, ADValue value, XmlWriter mXmlWriter, string strNamespace)
        {
            if (strNamespace != null)
            {
                mXmlWriter.WriteStartElement(valueElt, strNamespace);
            }
            else
            {
                mXmlWriter.WriteStartElement(valueElt);
            }
            if (value.IsBinary && (value.BinaryVal != null))
            {
                mXmlWriter.WriteAttributeString("xsi", "type", "http://www.w3.org/2001/XMLSchema-instance", "xsd:base64Binary");
                mXmlWriter.WriteBase64(value.BinaryVal, 0, value.BinaryVal.Length);
            }
            else
            {
                mXmlWriter.WriteString(value.StringVal);
            }
            mXmlWriter.WriteEndElement();
        }
    }
}

