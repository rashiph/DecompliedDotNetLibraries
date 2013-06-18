namespace MS.Internal.Xaml.Context
{
    using System;
    using System.Windows.Markup;
    using System.Xaml;

    internal static class ContextServices
    {
        public static object GetTargetProperty(ObjectWriterContext xamlContext)
        {
            IProvideValueTarget parentProperty = xamlContext.ParentProperty as IProvideValueTarget;
            if (parentProperty != null)
            {
                return parentProperty.TargetProperty;
            }
            XamlMember member = xamlContext.ParentProperty;
            if (member == null)
            {
                return null;
            }
            if (member.IsAttachable)
            {
                return member.Setter;
            }
            return member.UnderlyingMember;
        }
    }
}

