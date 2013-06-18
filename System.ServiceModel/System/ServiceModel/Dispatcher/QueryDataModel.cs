namespace System.ServiceModel.Dispatcher
{
    using System;

    internal static class QueryDataModel
    {
        internal static QueryAxis[] axes = new QueryAxis[] { new QueryAxis(QueryAxisType.None, AxisDirection.Forward, QueryNodeType.Any, QueryNodeType.Any), new QueryAxis(QueryAxisType.Ancestor, AxisDirection.Reverse, QueryNodeType.Element, QueryNodeType.Ancestor), new QueryAxis(QueryAxisType.AncestorOrSelf, AxisDirection.Reverse, QueryNodeType.Element, QueryNodeType.All), new QueryAxis(QueryAxisType.Attribute, AxisDirection.Forward, QueryNodeType.Attribute, QueryNodeType.Attribute), new QueryAxis(QueryAxisType.Child, AxisDirection.Forward, QueryNodeType.Element, QueryNodeType.ChildNodes), new QueryAxis(QueryAxisType.Descendant, AxisDirection.Forward, QueryNodeType.Element, QueryNodeType.ChildNodes), new QueryAxis(QueryAxisType.DescendantOrSelf, AxisDirection.Forward, QueryNodeType.Element, QueryNodeType.All), new QueryAxis(QueryAxisType.Following, AxisDirection.Forward, QueryNodeType.Element, QueryNodeType.ChildNodes), new QueryAxis(QueryAxisType.FollowingSibling, AxisDirection.Forward, QueryNodeType.Element, QueryNodeType.ChildNodes), new QueryAxis(QueryAxisType.Namespace, AxisDirection.Forward, QueryNodeType.Namespace, QueryNodeType.Namespace), new QueryAxis(QueryAxisType.Parent, AxisDirection.Reverse, QueryNodeType.Element, QueryNodeType.Ancestor), new QueryAxis(QueryAxisType.Preceding, AxisDirection.Reverse, QueryNodeType.Element, QueryNodeType.ChildNodes), new QueryAxis(QueryAxisType.PrecedingSibling, AxisDirection.Reverse, QueryNodeType.Element, QueryNodeType.All), new QueryAxis(QueryAxisType.Self, AxisDirection.Forward, QueryNodeType.Element, QueryNodeType.All) };
        internal static string Wildcard = "*";

        internal static QueryAxis GetAxis(QueryAxisType type)
        {
            return axes[(int) type];
        }

        internal static bool IsAttribute(string ns)
        {
            return (0 != string.CompareOrdinal("http://www.w3.org/2000/xmlns/", ns));
        }
    }
}

