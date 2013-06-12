namespace System.Xml.Serialization
{
    using System;
    using System.Collections;

    internal abstract class AccessorMapping : Mapping
    {
        private AttributeAccessor attribute;
        private ChoiceIdentifierAccessor choiceIdentifier;
        private ElementAccessor[] elements;
        private bool ignore;
        private ElementAccessor[] sortedElements;
        private TextAccessor text;
        private System.Xml.Serialization.TypeDesc typeDesc;
        private XmlnsAccessor xmlns;

        protected AccessorMapping()
        {
        }

        internal static bool ElementsMatch(ElementAccessor[] a, ElementAccessor[] b)
        {
            if (a == null)
            {
                return (b == null);
            }
            if (b == null)
            {
                return false;
            }
            if (a.Length != b.Length)
            {
                return false;
            }
            for (int i = 0; i < a.Length; i++)
            {
                if (((a[i].Name != b[i].Name) || (a[i].Namespace != b[i].Namespace)) || ((a[i].Form != b[i].Form) || (a[i].IsNullable != b[i].IsNullable)))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsNeedNullableMember(ElementAccessor element)
        {
            if (element.Mapping is ArrayMapping)
            {
                ArrayMapping mapping = (ArrayMapping) element.Mapping;
                return (((mapping.Elements != null) && (mapping.Elements.Length == 1)) && IsNeedNullableMember(mapping.Elements[0]));
            }
            return (element.IsNullable && element.Mapping.TypeDesc.IsValueType);
        }

        internal bool Match(AccessorMapping mapping)
        {
            if ((this.Elements != null) && (this.Elements.Length > 0))
            {
                if (!ElementsMatch(this.Elements, mapping.Elements))
                {
                    return false;
                }
                if (this.Text == null)
                {
                    return (mapping.Text == null);
                }
            }
            if (this.Attribute != null)
            {
                return (((mapping.Attribute != null) && ((this.Attribute.Name == mapping.Attribute.Name) && (this.Attribute.Namespace == mapping.Attribute.Namespace))) && (this.Attribute.Form == mapping.Attribute.Form));
            }
            if (this.Text != null)
            {
                return (mapping.Text != null);
            }
            return (mapping.Accessor == null);
        }

        internal static void SortMostToLeastDerived(ElementAccessor[] elements)
        {
            Array.Sort(elements, new AccessorComparer());
        }

        internal System.Xml.Serialization.Accessor Accessor
        {
            get
            {
                if (this.xmlns != null)
                {
                    return this.xmlns;
                }
                if (this.attribute != null)
                {
                    return this.attribute;
                }
                if ((this.elements != null) && (this.elements.Length > 0))
                {
                    return this.elements[0];
                }
                return this.text;
            }
        }

        internal AttributeAccessor Attribute
        {
            get
            {
                return this.attribute;
            }
            set
            {
                this.attribute = value;
            }
        }

        internal ChoiceIdentifierAccessor ChoiceIdentifier
        {
            get
            {
                return this.choiceIdentifier;
            }
            set
            {
                this.choiceIdentifier = value;
            }
        }

        internal ElementAccessor[] Elements
        {
            get
            {
                return this.elements;
            }
            set
            {
                this.elements = value;
                this.sortedElements = null;
            }
        }

        internal ElementAccessor[] ElementsSortedByDerivation
        {
            get
            {
                if (this.sortedElements == null)
                {
                    if (this.elements == null)
                    {
                        return null;
                    }
                    this.sortedElements = new ElementAccessor[this.elements.Length];
                    Array.Copy(this.elements, 0, this.sortedElements, 0, this.elements.Length);
                    SortMostToLeastDerived(this.sortedElements);
                }
                return this.sortedElements;
            }
        }

        internal bool Ignore
        {
            get
            {
                return this.ignore;
            }
            set
            {
                this.ignore = value;
            }
        }

        internal bool IsAttribute
        {
            get
            {
                return (this.attribute != null);
            }
        }

        internal bool IsNeedNullable
        {
            get
            {
                if (this.xmlns != null)
                {
                    return false;
                }
                if (this.attribute != null)
                {
                    return false;
                }
                return (((this.elements != null) && (this.elements.Length == 1)) && IsNeedNullableMember(this.elements[0]));
            }
        }

        internal bool IsParticle
        {
            get
            {
                return ((this.elements != null) && (this.elements.Length > 0));
            }
        }

        internal bool IsText
        {
            get
            {
                if (this.text == null)
                {
                    return false;
                }
                if (this.elements != null)
                {
                    return (this.elements.Length == 0);
                }
                return true;
            }
        }

        internal TextAccessor Text
        {
            get
            {
                return this.text;
            }
            set
            {
                this.text = value;
            }
        }

        internal System.Xml.Serialization.TypeDesc TypeDesc
        {
            get
            {
                return this.typeDesc;
            }
            set
            {
                this.typeDesc = value;
            }
        }

        internal XmlnsAccessor Xmlns
        {
            get
            {
                return this.xmlns;
            }
            set
            {
                this.xmlns = value;
            }
        }

        internal class AccessorComparer : IComparer
        {
            public int Compare(object o1, object o2)
            {
                if (o1 == o2)
                {
                    return 0;
                }
                Accessor accessor = (Accessor) o1;
                Accessor accessor2 = (Accessor) o2;
                int weight = accessor.Mapping.TypeDesc.Weight;
                int num2 = accessor2.Mapping.TypeDesc.Weight;
                if (weight == num2)
                {
                    return 0;
                }
                if (weight < num2)
                {
                    return 1;
                }
                return -1;
            }
        }
    }
}

