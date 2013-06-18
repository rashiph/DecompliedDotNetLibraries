namespace System.ServiceModel.Security
{
    using System;
    using System.ComponentModel;
    using System.Security.Principal;
    using System.ServiceModel;

    internal static class TokenImpersonationLevelHelper
    {
        private static TokenImpersonationLevel[] TokenImpersonationLevelOrder;

        static TokenImpersonationLevelHelper()
        {
            TokenImpersonationLevel[] levelArray = new TokenImpersonationLevel[5];
            levelArray[1] = TokenImpersonationLevel.Anonymous;
            levelArray[2] = TokenImpersonationLevel.Identification;
            levelArray[3] = TokenImpersonationLevel.Impersonation;
            levelArray[4] = TokenImpersonationLevel.Delegation;
            TokenImpersonationLevelOrder = levelArray;
        }

        internal static int Compare(TokenImpersonationLevel x, TokenImpersonationLevel y)
        {
            int num = 0;
            if (x == y)
            {
                return num;
            }
            switch (x)
            {
                case TokenImpersonationLevel.Identification:
                    return -1;

                case TokenImpersonationLevel.Impersonation:
                    switch (y)
                    {
                        case TokenImpersonationLevel.Identification:
                            return 1;

                        case TokenImpersonationLevel.Delegation:
                            return -1;
                    }
                    break;

                case TokenImpersonationLevel.Delegation:
                    return 1;

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("x", (int) x, typeof(TokenImpersonationLevel)));
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("y", (int) y, typeof(TokenImpersonationLevel)));
        }

        internal static bool IsDefined(TokenImpersonationLevel value)
        {
            if (((value != TokenImpersonationLevel.None) && (value != TokenImpersonationLevel.Anonymous)) && ((value != TokenImpersonationLevel.Identification) && (value != TokenImpersonationLevel.Impersonation)))
            {
                return (value == TokenImpersonationLevel.Delegation);
            }
            return true;
        }

        internal static bool IsGreaterOrEqual(TokenImpersonationLevel x, TokenImpersonationLevel y)
        {
            Validate(x);
            Validate(y);
            if (x == y)
            {
                return true;
            }
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < TokenImpersonationLevelOrder.Length; i++)
            {
                if (x == TokenImpersonationLevelOrder[i])
                {
                    num = i;
                }
                if (y == TokenImpersonationLevelOrder[i])
                {
                    num2 = i;
                }
            }
            return (num > num2);
        }

        internal static string ToString(TokenImpersonationLevel impersonationLevel)
        {
            if (impersonationLevel == TokenImpersonationLevel.Identification)
            {
                return "identification";
            }
            if (impersonationLevel == TokenImpersonationLevel.None)
            {
                return "none";
            }
            if (impersonationLevel == TokenImpersonationLevel.Anonymous)
            {
                return "anonymous";
            }
            if (impersonationLevel == TokenImpersonationLevel.Impersonation)
            {
                return "impersonation";
            }
            if (impersonationLevel != TokenImpersonationLevel.Delegation)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("impersonationLevel", (int) impersonationLevel, typeof(TokenImpersonationLevel)));
            }
            return "delegation";
        }

        internal static void Validate(TokenImpersonationLevel value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int) value, typeof(TokenImpersonationLevel)));
            }
        }
    }
}

