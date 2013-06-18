namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.Reflection;

    internal static class IndexerHelper
    {
        public static void CacheMethod<TOperand, TItem>(Collection<InArgument> indices, ref MethodInfo getMethod, ref MethodInfo setMethod)
        {
            Type[] types = new Type[indices.Count];
            for (int i = 0; i < indices.Count; i++)
            {
                types[i] = indices[i].ArgumentType;
            }
            getMethod = typeof(TOperand).GetMethod("get_Item", types);
            if ((getMethod != null) && !getMethod.IsSpecialName)
            {
                getMethod = null;
            }
            Type[] typeArray2 = new Type[indices.Count + 1];
            for (int j = 0; j < indices.Count; j++)
            {
                typeArray2[j] = indices[j].ArgumentType;
            }
            typeArray2[typeArray2.Length - 1] = typeof(TItem);
            setMethod = typeof(TOperand).GetMethod("set_Item", typeArray2);
            if ((setMethod != null) && !setMethod.IsSpecialName)
            {
                setMethod = null;
            }
        }

        public static void OnGetArguments<TItem>(Collection<InArgument> indices, OutArgument<Location<TItem>> result, CodeActivityMetadata metadata)
        {
            for (int i = 0; i < indices.Count; i++)
            {
                RuntimeArgument argument = new RuntimeArgument("Index" + i, indices[i].ArgumentType, ArgumentDirection.In, true);
                metadata.Bind(indices[i], argument);
                metadata.AddArgument(argument);
            }
            RuntimeArgument argument2 = new RuntimeArgument("Result", typeof(Location<TItem>), ArgumentDirection.Out);
            metadata.Bind(result, argument2);
            metadata.AddArgument(argument2);
        }
    }
}

