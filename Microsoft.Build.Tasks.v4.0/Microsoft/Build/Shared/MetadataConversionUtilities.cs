namespace Microsoft.Build.Shared
{
    using Microsoft.Build.Framework;
    using System;
    using System.Runtime.InteropServices;

    internal static class MetadataConversionUtilities
    {
        internal static bool TryConvertItemMetadataToBool(ITaskItem item, string itemMetadataName)
        {
            bool metadataFound = false;
            return TryConvertItemMetadataToBool(item, itemMetadataName, out metadataFound);
        }

        internal static bool TryConvertItemMetadataToBool(ITaskItem item, string itemMetadataName, out bool metadataFound)
        {
            bool flag;
            string metadata = item.GetMetadata(itemMetadataName);
            if ((metadata == null) || (metadata.Length == 0))
            {
                metadataFound = false;
                return false;
            }
            metadataFound = true;
            try
            {
                flag = ConversionUtilities.ConvertStringToBool(metadata);
            }
            catch (ArgumentException exception)
            {
                throw new ArgumentException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("General.InvalidAttributeMetadata", new object[] { item.ItemSpec, itemMetadataName, metadata, "bool" }), exception);
            }
            return flag;
        }
    }
}

