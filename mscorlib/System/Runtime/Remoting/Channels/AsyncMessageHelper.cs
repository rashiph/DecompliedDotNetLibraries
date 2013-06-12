namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Reflection;

    internal static class AsyncMessageHelper
    {
        internal static void GetOutArgs(ParameterInfo[] syncParams, object[] syncArgs, object[] endArgs)
        {
            int num = 0;
            for (int i = 0; i < syncParams.Length; i++)
            {
                if (syncParams[i].IsOut || syncParams[i].ParameterType.IsByRef)
                {
                    endArgs[num++] = syncArgs[i];
                }
            }
        }
    }
}

