namespace System.Xml.Schema
{
    using MS.Internal.Xml.XPath;
    using System;
    using System.Collections;
    using System.Xml;
    using System.Xml.XPath;

    internal class Asttree
    {
        private ArrayList fAxisArray;
        private bool isField;
        private XmlNamespaceManager nsmgr;
        private string xpathexpr;

        public Asttree(string xPath, bool isField, XmlNamespaceManager nsmgr)
        {
            this.xpathexpr = xPath;
            this.isField = isField;
            this.nsmgr = nsmgr;
            this.CompileXPath(xPath, isField, nsmgr);
        }

        public void CompileXPath(string xPath, bool isField, XmlNamespaceManager nsmgr)
        {
            if ((xPath == null) || (xPath.Length == 0))
            {
                throw new XmlSchemaException("Sch_EmptyXPath", string.Empty);
            }
            string[] strArray = xPath.Split(new char[] { '|' });
            ArrayList list = new ArrayList(strArray.Length);
            this.fAxisArray = new ArrayList(strArray.Length);
            try
            {
                for (int j = 0; j < strArray.Length; j++)
                {
                    Axis axis = (Axis) XPathParser.ParseXPathExpresion(strArray[j]);
                    list.Add(axis);
                }
            }
            catch
            {
                throw new XmlSchemaException("Sch_ICXpathError", xPath);
            }
            for (int i = 0; i < list.Count; i++)
            {
                Axis ast = (Axis) list[i];
                Axis input = ast;
                if (input == null)
                {
                    throw new XmlSchemaException("Sch_ICXpathError", xPath);
                }
                Axis axis4 = input;
                if (!IsAttribute(input))
                {
                    goto Label_013D;
                }
                if (!isField)
                {
                    throw new XmlSchemaException("Sch_SelectorAttr", xPath);
                }
                this.SetURN(input, nsmgr);
                try
                {
                    input = (Axis) input.Input;
                    goto Label_013D;
                }
                catch
                {
                    throw new XmlSchemaException("Sch_ICXpathError", xPath);
                }
            Label_00EB:
                if (IsSelf(input) && (ast != input))
                {
                    axis4.Input = input.Input;
                }
                else
                {
                    axis4 = input;
                    if (IsNameTest(input))
                    {
                        this.SetURN(input, nsmgr);
                    }
                }
                try
                {
                    input = (Axis) input.Input;
                }
                catch
                {
                    throw new XmlSchemaException("Sch_ICXpathError", xPath);
                }
            Label_013D:
                if ((input != null) && (IsNameTest(input) || IsSelf(input)))
                {
                    goto Label_00EB;
                }
                axis4.Input = null;
                if (input == null)
                {
                    if (IsSelf(ast) && (ast.Input != null))
                    {
                        this.fAxisArray.Add(new ForwardAxis(DoubleLinkAxis.ConvertTree((Axis) ast.Input), false));
                    }
                    else
                    {
                        this.fAxisArray.Add(new ForwardAxis(DoubleLinkAxis.ConvertTree(ast), false));
                    }
                }
                else
                {
                    if (!IsDescendantOrSelf(input))
                    {
                        throw new XmlSchemaException("Sch_ICXpathError", xPath);
                    }
                    try
                    {
                        input = (Axis) input.Input;
                    }
                    catch
                    {
                        throw new XmlSchemaException("Sch_ICXpathError", xPath);
                    }
                    if (((input == null) || !IsSelf(input)) || (input.Input != null))
                    {
                        throw new XmlSchemaException("Sch_ICXpathError", xPath);
                    }
                    if (IsSelf(ast) && (ast.Input != null))
                    {
                        this.fAxisArray.Add(new ForwardAxis(DoubleLinkAxis.ConvertTree((Axis) ast.Input), true));
                    }
                    else
                    {
                        this.fAxisArray.Add(new ForwardAxis(DoubleLinkAxis.ConvertTree(ast), true));
                    }
                }
            }
        }

        internal static bool IsAttribute(Axis ast)
        {
            return ((ast.TypeOfAxis == Axis.AxisType.Attribute) && (ast.NodeType == XPathNodeType.Attribute));
        }

        private static bool IsDescendantOrSelf(Axis ast)
        {
            return (((ast.TypeOfAxis == Axis.AxisType.DescendantOrSelf) && (ast.NodeType == XPathNodeType.All)) && ast.AbbrAxis);
        }

        private static bool IsNameTest(Axis ast)
        {
            return ((ast.TypeOfAxis == Axis.AxisType.Child) && (ast.NodeType == XPathNodeType.Element));
        }

        internal static bool IsSelf(Axis ast)
        {
            return (((ast.TypeOfAxis == Axis.AxisType.Self) && (ast.NodeType == XPathNodeType.All)) && ast.AbbrAxis);
        }

        private void SetURN(Axis axis, XmlNamespaceManager nsmgr)
        {
            if (axis.Prefix.Length != 0)
            {
                axis.Urn = nsmgr.LookupNamespace(axis.Prefix);
                if (axis.Urn == null)
                {
                    throw new XmlSchemaException("Sch_UnresolvedPrefix", axis.Prefix);
                }
            }
            else if (axis.Name.Length != 0)
            {
                axis.Urn = null;
            }
            else
            {
                axis.Urn = "";
            }
        }

        internal ArrayList SubtreeArray
        {
            get
            {
                return this.fAxisArray;
            }
        }
    }
}

