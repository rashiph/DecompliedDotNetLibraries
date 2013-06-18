namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Utilities;
    using System;
    using System.IO;
    using System.Text;

    public class CreateVisualBasicManifestResourceName : CreateManifestResourceName
    {
        protected override string CreateManifestName(string fileName, string linkFileName, string rootNamespace, string dependentUponFileName, Stream binaryStream)
        {
            return CreateManifestNameImpl(fileName, linkFileName, base.PrependCultureAsDirectory, rootNamespace, dependentUponFileName, binaryStream, base.Log);
        }

        internal static string CreateManifestNameImpl(string fileName, string linkFileName, bool prependCultureAsDirectory, string rootNamespace, string dependentUponFileName, Stream binaryStream, TaskLoggingHelper log)
        {
            string str = linkFileName;
            if ((str == null) || (str.Length == 0))
            {
                str = fileName;
            }
            Culture.ItemCultureInfo itemCultureInfo = Culture.GetItemCultureInfo(str, dependentUponFileName);
            StringBuilder builder = new StringBuilder();
            if (binaryStream != null)
            {
                ExtractedClassName firstClassNameFullyQualified = VisualBasicParserUtilities.GetFirstClassNameFullyQualified(binaryStream);
                if (firstClassNameFullyQualified.IsInsideConditionalBlock && (log != null))
                {
                    log.LogWarningWithCodeFromResources("CreateManifestResourceName.DefinitionFoundWithinConditionalDirective", new object[] { dependentUponFileName, str });
                }
                if ((firstClassNameFullyQualified.Name != null) && (firstClassNameFullyQualified.Name.Length > 0))
                {
                    if ((rootNamespace != null) && (rootNamespace.Length > 0))
                    {
                        builder.Append(rootNamespace).Append(".").Append(firstClassNameFullyQualified.Name);
                    }
                    else
                    {
                        builder.Append(firstClassNameFullyQualified.Name);
                    }
                    if ((itemCultureInfo.culture != null) && (itemCultureInfo.culture.Length > 0))
                    {
                        builder.Append(".").Append(itemCultureInfo.culture);
                    }
                }
            }
            if (builder.Length == 0)
            {
                if ((rootNamespace != null) && (rootNamespace.Length > 0))
                {
                    builder.Append(rootNamespace).Append(".");
                }
                string extension = Path.GetExtension(itemCultureInfo.cultureNeutralFilename);
                if (((string.Compare(extension, ".resx", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(extension, ".restext", StringComparison.OrdinalIgnoreCase) == 0)) || (string.Compare(extension, ".resources", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    builder.Append(Path.GetFileNameWithoutExtension(itemCultureInfo.cultureNeutralFilename));
                    if ((itemCultureInfo.culture != null) && (itemCultureInfo.culture.Length > 0))
                    {
                        builder.Append(".").Append(itemCultureInfo.culture);
                    }
                    if (string.Equals(extension, ".resources", StringComparison.OrdinalIgnoreCase))
                    {
                        builder.Append(extension);
                    }
                }
                else
                {
                    builder.Append(Path.GetFileName(itemCultureInfo.cultureNeutralFilename));
                    if ((prependCultureAsDirectory && (itemCultureInfo.culture != null)) && (itemCultureInfo.culture.Length > 0))
                    {
                        builder.Insert(0, Path.DirectorySeparatorChar);
                        builder.Insert(0, itemCultureInfo.culture);
                    }
                }
            }
            return builder.ToString();
        }

        protected override bool IsSourceFile(string fileName)
        {
            return (string.Compare(Path.GetExtension(fileName), ".vb", StringComparison.OrdinalIgnoreCase) == 0);
        }
    }
}

