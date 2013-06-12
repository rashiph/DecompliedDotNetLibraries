namespace MS.Internal.Xml.Cache
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.XPath;

    internal abstract class XPathNodeHelper
    {
        protected XPathNodeHelper()
        {
        }

        public static bool GetAttribute(ref XPathNode[] pageNode, ref int idxNode, string localName, string namespaceName)
        {
            XPathNode[] nodeArray = pageNode;
            int index = idxNode;
            if (nodeArray[index].HasAttribute)
            {
                GetChild(ref nodeArray, ref index);
                do
                {
                    if (nodeArray[index].NameMatch(localName, namespaceName))
                    {
                        pageNode = nodeArray;
                        idxNode = index;
                        return true;
                    }
                    index = nodeArray[index].GetSibling(out nodeArray);
                }
                while ((index != 0) && (nodeArray[index].NodeType == XPathNodeType.Attribute));
            }
            return false;
        }

        private static void GetChild(ref XPathNode[] pageNode, ref int idxNode)
        {
            if (++idxNode >= pageNode.Length)
            {
                pageNode = pageNode[0].PageInfo.NextPage;
                idxNode = 1;
            }
        }

        public static bool GetContentChild(ref XPathNode[] pageNode, ref int idxNode)
        {
            XPathNode[] nodeArray = pageNode;
            int index = idxNode;
            if (!nodeArray[index].HasContentChild)
            {
                return false;
            }
            GetChild(ref nodeArray, ref index);
            while (nodeArray[index].NodeType == XPathNodeType.Attribute)
            {
                index = nodeArray[index].GetSibling(out nodeArray);
            }
            pageNode = nodeArray;
            idxNode = index;
            return true;
        }

        public static bool GetContentChild(ref XPathNode[] pageNode, ref int idxNode, XPathNodeType typ)
        {
            XPathNode[] nodeArray = pageNode;
            int index = idxNode;
            if (nodeArray[index].HasContentChild)
            {
                int contentKindMask = XPathNavigator.GetContentKindMask(typ);
                GetChild(ref nodeArray, ref index);
                do
                {
                    if (((((int) 1) << nodeArray[index].NodeType) & contentKindMask) != 0)
                    {
                        if (typ == XPathNodeType.Attribute)
                        {
                            return false;
                        }
                        pageNode = nodeArray;
                        idxNode = index;
                        return true;
                    }
                    index = nodeArray[index].GetSibling(out nodeArray);
                }
                while (index != 0);
            }
            return false;
        }

        public static bool GetContentFollowing(ref XPathNode[] pageCurrent, ref int idxCurrent, XPathNode[] pageEnd, int idxEnd, XPathNodeType typ)
        {
            XPathNode[] nextPage = pageCurrent;
            int index = idxCurrent;
            int contentKindMask = XPathNavigator.GetContentKindMask(typ);
            index++;
        Label_0012:
            if ((nextPage != pageEnd) || (index > idxEnd))
            {
                while (index < nextPage[0].PageInfo.NodeCount)
                {
                    if (((((int) 1) << nextPage[index].NodeType) & contentKindMask) != 0)
                    {
                        goto Label_0081;
                    }
                    index++;
                }
                nextPage = nextPage[0].PageInfo.NextPage;
                index = 1;
                if (nextPage != null)
                {
                    goto Label_0012;
                }
            }
            else
            {
                while (index != idxEnd)
                {
                    if (((((int) 1) << nextPage[index].NodeType) & contentKindMask) != 0)
                    {
                        goto Label_0081;
                    }
                    index++;
                }
            }
            return false;
        Label_0081:
            pageCurrent = nextPage;
            idxCurrent = index;
            return true;
        }

        public static bool GetContentSibling(ref XPathNode[] pageNode, ref int idxNode)
        {
            XPathNode[] nodeArray = pageNode;
            int index = idxNode;
            if (!nodeArray[index].IsAttrNmsp)
            {
                index = nodeArray[index].GetSibling(out nodeArray);
                if (index != 0)
                {
                    pageNode = nodeArray;
                    idxNode = index;
                    return true;
                }
            }
            return false;
        }

        public static bool GetContentSibling(ref XPathNode[] pageNode, ref int idxNode, XPathNodeType typ)
        {
            XPathNode[] nodeArray = pageNode;
            int index = idxNode;
            int contentKindMask = XPathNavigator.GetContentKindMask(typ);
            if (nodeArray[index].NodeType != XPathNodeType.Attribute)
            {
                do
                {
                    index = nodeArray[index].GetSibling(out nodeArray);
                    if (index == 0)
                    {
                        goto Label_004B;
                    }
                }
                while (((((int) 1) << nodeArray[index].NodeType) & contentKindMask) == 0);
                pageNode = nodeArray;
                idxNode = index;
                return true;
            }
        Label_004B:
            return false;
        }

        public static bool GetElementChild(ref XPathNode[] pageNode, ref int idxNode, string localName, string namespaceName)
        {
            XPathNode[] nodeArray = pageNode;
            int index = idxNode;
            if (nodeArray[index].HasElementChild)
            {
                GetChild(ref nodeArray, ref index);
                do
                {
                    if (nodeArray[index].ElementMatch(localName, namespaceName))
                    {
                        pageNode = nodeArray;
                        idxNode = index;
                        return true;
                    }
                    index = nodeArray[index].GetSibling(out nodeArray);
                }
                while (index != 0);
            }
            return false;
        }

        public static bool GetElementFollowing(ref XPathNode[] pageCurrent, ref int idxCurrent, XPathNode[] pageEnd, int idxEnd, string localName, string namespaceName)
        {
            int num3;
            XPathNode[] pageNode = pageCurrent;
            int index = idxCurrent;
            if ((pageNode[index].NodeType != XPathNodeType.Element) || (pageNode[index].LocalName != localName))
            {
                index++;
            Label_00C3:
                if ((pageNode != pageEnd) || (index > idxEnd))
                {
                    while (index < pageNode[0].PageInfo.NodeCount)
                    {
                        if (pageNode[index].ElementMatch(localName, namespaceName))
                        {
                            goto Label_012C;
                        }
                        index++;
                    }
                    pageNode = pageNode[0].PageInfo.NextPage;
                    index = 1;
                    if (pageNode != null)
                    {
                        goto Label_00C3;
                    }
                }
                else
                {
                    while (index != idxEnd)
                    {
                        if (pageNode[index].ElementMatch(localName, namespaceName))
                        {
                            goto Label_012C;
                        }
                        index++;
                    }
                }
                return false;
            }
            int pageNumber = 0;
            if (pageEnd != null)
            {
                pageNumber = pageEnd[0].PageInfo.PageNumber;
                num3 = pageNode[0].PageInfo.PageNumber;
                if ((num3 > pageNumber) || ((num3 == pageNumber) && (index >= idxEnd)))
                {
                    pageEnd = null;
                }
            }
            do
            {
                index = pageNode[index].GetSimilarElement(out pageNode);
                if (index == 0)
                {
                    goto Label_00BD;
                }
                if (pageEnd != null)
                {
                    num3 = pageNode[0].PageInfo.PageNumber;
                    if ((num3 > pageNumber) || ((num3 == pageNumber) && (index >= idxEnd)))
                    {
                        goto Label_00BD;
                    }
                }
            }
            while ((pageNode[index].LocalName != localName) || !(pageNode[index].NamespaceUri == namespaceName));
            goto Label_012C;
        Label_00BD:
            return false;
        Label_012C:
            pageCurrent = pageNode;
            idxCurrent = index;
            return true;
        }

        public static bool GetElementSibling(ref XPathNode[] pageNode, ref int idxNode, string localName, string namespaceName)
        {
            XPathNode[] nodeArray = pageNode;
            int index = idxNode;
            if (nodeArray[index].NodeType != XPathNodeType.Attribute)
            {
                do
                {
                    index = nodeArray[index].GetSibling(out nodeArray);
                    if (index == 0)
                    {
                        goto Label_003F;
                    }
                }
                while (!nodeArray[index].ElementMatch(localName, namespaceName));
                pageNode = nodeArray;
                idxNode = index;
                return true;
            }
        Label_003F:
            return false;
        }

        public static bool GetFirstAttribute(ref XPathNode[] pageNode, ref int idxNode)
        {
            if (pageNode[idxNode].HasAttribute)
            {
                GetChild(ref pageNode, ref idxNode);
                return true;
            }
            return false;
        }

        public static bool GetFollowing(ref XPathNode[] pageNode, ref int idxNode)
        {
            XPathNode[] nextPage = pageNode;
            int num = idxNode;
            do
            {
                if (++num < nextPage[0].PageInfo.NodeCount)
                {
                    pageNode = nextPage;
                    idxNode = num;
                    return true;
                }
                nextPage = nextPage[0].PageInfo.NextPage;
                num = 0;
            }
            while (nextPage != null);
            return false;
        }

        public static int GetInScopeNamespaces(XPathNode[] pageElem, int idxElem, out XPathNode[] pageNmsp)
        {
            if (pageElem[idxElem].NodeType == XPathNodeType.Element)
            {
                XPathDocument document = pageElem[idxElem].Document;
                while (!pageElem[idxElem].HasNamespaceDecls)
                {
                    idxElem = pageElem[idxElem].GetParent(out pageElem);
                    if (idxElem == 0)
                    {
                        return document.GetXmlNamespaceNode(out pageNmsp);
                    }
                }
                return document.LookupNamespaces(pageElem, idxElem, out pageNmsp);
            }
            pageNmsp = null;
            return 0;
        }

        public static int GetLocalNamespaces(XPathNode[] pageElem, int idxElem, out XPathNode[] pageNmsp)
        {
            if (pageElem[idxElem].HasNamespaceDecls)
            {
                return pageElem[idxElem].Document.LookupNamespaces(pageElem, idxElem, out pageNmsp);
            }
            pageNmsp = null;
            return 0;
        }

        public static int GetLocation(XPathNode[] pageNode, int idxNode)
        {
            return ((pageNode[0].PageInfo.PageNumber << 0x10) | idxNode);
        }

        public static bool GetNextAttribute(ref XPathNode[] pageNode, ref int idxNode)
        {
            XPathNode[] nodeArray;
            int sibling = pageNode[idxNode].GetSibling(out nodeArray);
            if ((sibling != 0) && (nodeArray[sibling].NodeType == XPathNodeType.Attribute))
            {
                pageNode = nodeArray;
                idxNode = sibling;
                return true;
            }
            return false;
        }

        public static bool GetNonDescendant(ref XPathNode[] pageNode, ref int idxNode)
        {
            XPathNode[] nodeArray = pageNode;
            int index = idxNode;
            do
            {
                if (nodeArray[index].HasSibling)
                {
                    pageNode = nodeArray;
                    idxNode = nodeArray[index].GetSibling(out pageNode);
                    return true;
                }
                index = nodeArray[index].GetParent(out nodeArray);
            }
            while (index != 0);
            return false;
        }

        public static bool GetParent(ref XPathNode[] pageNode, ref int idxNode)
        {
            XPathNode[] nodeArray = pageNode;
            int index = idxNode;
            index = nodeArray[index].GetParent(out nodeArray);
            if (index != 0)
            {
                pageNode = nodeArray;
                idxNode = index;
                return true;
            }
            return false;
        }

        public static bool GetPreviousContentSibling(ref XPathNode[] pageNode, ref int idxNode)
        {
            XPathNode[] nodeArray = pageNode;
            int index = idxNode;
            index = nodeArray[index].GetParent(out nodeArray);
            if (index != 0)
            {
                XPathNode[] previousPage;
                int num2 = idxNode - 1;
                if (num2 == 0)
                {
                    previousPage = pageNode[0].PageInfo.PreviousPage;
                    num2 = previousPage.Length - 1;
                }
                else
                {
                    previousPage = pageNode;
                }
                if ((index == num2) && (nodeArray == previousPage))
                {
                    return false;
                }
                XPathNode[] nodeArray3 = previousPage;
                int parent = num2;
                do
                {
                    previousPage = nodeArray3;
                    num2 = parent;
                    parent = nodeArray3[parent].GetParent(out nodeArray3);
                }
                while ((parent != index) || (nodeArray3 != nodeArray));
                if (previousPage[num2].NodeType != XPathNodeType.Attribute)
                {
                    pageNode = previousPage;
                    idxNode = num2;
                    return true;
                }
            }
            return false;
        }

        public static bool GetPreviousContentSibling(ref XPathNode[] pageNode, ref int idxNode, XPathNodeType typ)
        {
            XPathNode[] nodeArray = pageNode;
            int index = idxNode;
            int contentKindMask = XPathNavigator.GetContentKindMask(typ);
            do
            {
                if (!GetPreviousContentSibling(ref nodeArray, ref index))
                {
                    return false;
                }
            }
            while (((((int) 1) << nodeArray[index].NodeType) & contentKindMask) == 0);
            pageNode = nodeArray;
            idxNode = index;
            return true;
        }

        public static bool GetPreviousElementSibling(ref XPathNode[] pageNode, ref int idxNode, string localName, string namespaceName)
        {
            XPathNode[] nodeArray = pageNode;
            int index = idxNode;
            if (nodeArray[index].NodeType != XPathNodeType.Attribute)
            {
                do
                {
                    if (!GetPreviousContentSibling(ref nodeArray, ref index))
                    {
                        goto Label_0038;
                    }
                }
                while (!nodeArray[index].ElementMatch(localName, namespaceName));
                pageNode = nodeArray;
                idxNode = index;
                return true;
            }
        Label_0038:
            return false;
        }

        public static bool GetTextFollowing(ref XPathNode[] pageCurrent, ref int idxCurrent, XPathNode[] pageEnd, int idxEnd)
        {
            XPathNode[] nextPage = pageCurrent;
            int index = idxCurrent;
            index++;
        Label_000A:
            if ((nextPage != pageEnd) || (index > idxEnd))
            {
                while (index < nextPage[0].PageInfo.NodeCount)
                {
                    if (nextPage[index].IsText || ((nextPage[index].NodeType == XPathNodeType.Element) && nextPage[index].HasCollapsedText))
                    {
                        goto Label_00AB;
                    }
                    index++;
                }
                nextPage = nextPage[0].PageInfo.NextPage;
                index = 1;
                if (nextPage != null)
                {
                    goto Label_000A;
                }
            }
            else
            {
                while (index != idxEnd)
                {
                    if (nextPage[index].IsText || ((nextPage[index].NodeType == XPathNodeType.Element) && nextPage[index].HasCollapsedText))
                    {
                        goto Label_00AB;
                    }
                    index++;
                }
            }
            return false;
        Label_00AB:
            pageCurrent = nextPage;
            idxCurrent = index;
            return true;
        }
    }
}

