namespace System.Xaml.Hosting
{
    using System;
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Text;
    using System.Web;
    using System.Web.Compilation;
    using System.Xaml;
    using System.Xml;

    [BuildProviderAppliesTo(BuildProviderAppliesTo.Web)]
    public class XamlBuildProvider : BuildProvider
    {
        private void AppendTypeName(XamlType xamlType, StringBuilder sb)
        {
            if (!string.IsNullOrEmpty(xamlType.PreferredXamlNamespace))
            {
                sb.Append("{");
                sb.Append(xamlType.PreferredXamlNamespace);
                sb.Append("}");
            }
            sb.Append(xamlType.Name);
            if (xamlType.IsGeneric)
            {
                sb.Append("(");
                for (int i = 0; i < xamlType.TypeArguments.Count; i++)
                {
                    this.AppendTypeName(xamlType.TypeArguments[i], sb);
                    if (i < (xamlType.TypeArguments.Count - 1))
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append(")");
            }
        }

        public override Type GetGeneratedType(CompilerResults results)
        {
            Type type;
            try
            {
                using (Stream stream = base.OpenStream())
                {
                    XamlXmlReader reader2 = new XamlXmlReader(XmlReader.Create(stream));
                    while (reader2.Read())
                    {
                        if (reader2.NodeType == XamlNodeType.StartObject)
                        {
                            if (reader2.Type.IsUnknown)
                            {
                                StringBuilder sb = new StringBuilder();
                                this.AppendTypeName(reader2.Type, sb);
                                throw FxTrace.Exception.AsError(new TypeLoadException(System.Xaml.Hosting.SR.CouldNotResolveType(sb)));
                            }
                            return reader2.Type.UnderlyingType;
                        }
                    }
                    throw FxTrace.Exception.AsError(new HttpCompileException(System.Xaml.Hosting.SR.UnexpectedEof));
                }
            }
            catch (XamlParseException exception)
            {
                throw FxTrace.Exception.AsError(new HttpCompileException(exception.Message, exception));
            }
            return type;
        }

        public override BuildProviderResultFlags GetResultFlags(CompilerResults results)
        {
            return BuildProviderResultFlags.ShutdownAppDomainOnChange;
        }
    }
}

