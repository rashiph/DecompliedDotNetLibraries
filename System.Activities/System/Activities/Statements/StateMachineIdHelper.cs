namespace System.Activities.Statements
{
    using System;
    using System.Globalization;

    internal static class StateMachineIdHelper
    {
        internal const char StateIdSeparator = ':';

        public static string GenerateStateId(string parentId, int index)
        {
            return (parentId + ':' + index.ToString(CultureInfo.InvariantCulture));
        }

        public static string GenerateTransitionId(string stateid, int transitionIndex)
        {
            return (stateid + ':' + transitionIndex.ToString(CultureInfo.InvariantCulture));
        }

        public static int GetChildStateIndex(string stateId, string descendantId)
        {
            string[] strArray = descendantId.Split(new char[] { ':' });
            string[] strArray2 = stateId.Split(new char[] { ':' });
            return int.Parse(strArray[strArray2.Length], CultureInfo.InvariantCulture);
        }

        public static bool IsAncestor(string state1Id, string state2Id)
        {
            if (string.IsNullOrEmpty(state2Id))
            {
                return false;
            }
            return state2Id.StartsWith(state1Id + ':', StringComparison.Ordinal);
        }
    }
}

