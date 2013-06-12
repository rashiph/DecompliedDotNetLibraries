namespace Microsoft.SqlServer.Server
{
    using System;

    internal class SmiXetterAccessMap
    {
        private const bool _ = false;
        private static bool[,] __isGetterAccessValid = new bool[,] { 
            { 
                false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, true, true, true, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, 
                false
             }, { 
                false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, true, true, true, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, true, true, true, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, true, true, true, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, 
                false
             }, 
            { 
                false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, true, true, true, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, true, true, true, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                true, true, true, true, true, true, true, true, true, true, true, true, true, true, false, true, 
                true
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, true, false, false, false, false, false, false, false, false, false, false, false, true, false, 
                false
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, 
                false
             }, 
            { 
                false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, 
                false
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                true
             }
         };
        private static bool[,] __isSetterAccessValid = new bool[,] { 
            { 
                false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, false, true, true, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, 
                false
             }, { 
                false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, false, true, true, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, false, true, true, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, false, true, true, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, 
                false
             }, 
            { 
                false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, false, true, true, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, false, true, true, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                true, true, true, true, true, true, true, true, true, true, true, true, true, true, false, true, 
                true
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, true, false, true, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, 
                false
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, 
                false
             }, 
            { 
                false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, 
                false
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, 
                false
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                true
             }
         };
        private const bool X = true;

        internal static bool IsGetterAccessValid(SmiMetaData metaData, SmiXetterTypeCode xetterType)
        {
            return __isGetterAccessValid[(int) metaData.SqlDbType, (int) xetterType];
        }

        internal static bool IsSetterAccessValid(SmiMetaData metaData, SmiXetterTypeCode xetterType)
        {
            return __isSetterAccessValid[(int) metaData.SqlDbType, (int) xetterType];
        }
    }
}

