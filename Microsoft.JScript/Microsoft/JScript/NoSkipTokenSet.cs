namespace Microsoft.JScript
{
    using System;

    internal class NoSkipTokenSet
    {
        private TokenSetListItem _tokenSet = null;
        internal static readonly JSToken[] s_ArrayInitNoSkipTokenSet = new JSToken[] { JSToken.RightBracket, JSToken.Comma };
        internal static readonly JSToken[] s_BlockConditionNoSkipTokenSet = new JSToken[] { JSToken.RightParen, JSToken.LeftCurly, JSToken.EndOfLine };
        internal static readonly JSToken[] s_BlockNoSkipTokenSet = new JSToken[] { JSToken.RightCurly };
        internal static readonly JSToken[] s_BracketToken = new JSToken[] { JSToken.RightBracket };
        internal static readonly JSToken[] s_CaseNoSkipTokenSet = new JSToken[] { JSToken.Case, JSToken.Default, JSToken.Colon, JSToken.EndOfLine };
        internal static readonly JSToken[] s_ClassBodyNoSkipTokenSet = new JSToken[] { JSToken.Class, JSToken.Interface, JSToken.Enum, JSToken.Function, JSToken.Var, JSToken.Const, JSToken.Static, JSToken.Public, JSToken.Private, JSToken.Protected };
        internal static readonly JSToken[] s_ClassExtendsNoSkipTokenSet = new JSToken[] { JSToken.LeftCurly, JSToken.Implements };
        internal static readonly JSToken[] s_ClassImplementsNoSkipTokenSet = new JSToken[] { JSToken.LeftCurly, JSToken.Comma };
        internal static readonly JSToken[] s_DoWhileBodyNoSkipTokenSet = new JSToken[] { JSToken.While };
        internal static readonly JSToken[] s_EndOfLineToken = new JSToken[] { JSToken.EndOfLine };
        internal static readonly JSToken[] s_EndOfStatementNoSkipTokenSet = new JSToken[] { JSToken.Semicolon, JSToken.EndOfLine };
        internal static readonly JSToken[] s_EnumBaseTypeNoSkipTokenSet = new JSToken[] { JSToken.LeftCurly };
        internal static readonly JSToken[] s_EnumBodyNoSkipTokenSet = new JSToken[] { JSToken.Identifier };
        internal static readonly JSToken[] s_ExpressionListNoSkipTokenSet = new JSToken[] { JSToken.Comma };
        internal static readonly JSToken[] s_FunctionDeclNoSkipTokenSet = new JSToken[] { JSToken.RightParen, JSToken.LeftCurly, JSToken.Comma };
        internal static readonly JSToken[] s_IfBodyNoSkipTokenSet = new JSToken[] { JSToken.Else };
        internal static readonly JSToken[] s_InterfaceBodyNoSkipTokenSet = new JSToken[] { JSToken.Enum, JSToken.Function, JSToken.Public, JSToken.EndOfLine, JSToken.Semicolon };
        internal static readonly JSToken[] s_MemberExprNoSkipTokenSet = new JSToken[] { JSToken.LeftBracket, JSToken.LeftParen, JSToken.AccessField };
        internal static readonly JSToken[] s_NoTrySkipTokenSet = new JSToken[] { JSToken.Catch, JSToken.Finally };
        internal static readonly JSToken[] s_ObjectInitNoSkipTokenSet = new JSToken[] { JSToken.RightCurly, JSToken.Comma };
        internal static readonly JSToken[] s_PackageBodyNoSkipTokenSet = new JSToken[] { JSToken.Class, JSToken.Interface, JSToken.Enum };
        internal static readonly JSToken[] s_ParenExpressionNoSkipToken = new JSToken[] { JSToken.RightParen };
        internal static readonly JSToken[] s_ParenToken = new JSToken[] { JSToken.RightParen };
        internal static readonly JSToken[] s_PostfixExpressionNoSkipTokenSet = new JSToken[] { JSToken.Increment, JSToken.Decrement };
        internal static readonly JSToken[] s_StartBlockNoSkipTokenSet = new JSToken[] { JSToken.LeftCurly };
        internal static readonly JSToken[] s_StartStatementNoSkipTokenSet = new JSToken[] { JSToken.LeftCurly, JSToken.Var, JSToken.Const, JSToken.If, JSToken.For, JSToken.Do, JSToken.While, JSToken.With, JSToken.Switch, JSToken.Try };
        internal static readonly JSToken[] s_SwitchNoSkipTokenSet = new JSToken[] { JSToken.Case, JSToken.Default };
        internal static readonly JSToken[] s_TopLevelNoSkipTokenSet = new JSToken[] { JSToken.Package, JSToken.Class, JSToken.Interface, JSToken.Enum, JSToken.Function, JSToken.Import };
        internal static readonly JSToken[] s_VariableDeclNoSkipTokenSet = new JSToken[] { JSToken.Comma, JSToken.Semicolon };

        internal NoSkipTokenSet()
        {
        }

        internal void Add(JSToken[] tokens)
        {
            this._tokenSet = new TokenSetListItem(tokens, this._tokenSet);
        }

        internal bool HasToken(JSToken token)
        {
            for (TokenSetListItem item = this._tokenSet; item != null; item = item._next)
            {
                int index = 0;
                int length = item._tokens.Length;
                while (index < length)
                {
                    if (item._tokens[index] == token)
                    {
                        return true;
                    }
                    index++;
                }
            }
            return false;
        }

        internal void Remove(JSToken[] tokens)
        {
            TokenSetListItem item = this._tokenSet;
            TokenSetListItem item2 = null;
            while (item != null)
            {
                if (item._tokens == tokens)
                {
                    if (item2 == null)
                    {
                        this._tokenSet = this._tokenSet._next;
                        return;
                    }
                    item2._next = item._next;
                    return;
                }
                item2 = item;
                item = item._next;
            }
        }

        private class TokenSetListItem
        {
            internal NoSkipTokenSet.TokenSetListItem _next;
            internal JSToken[] _tokens;

            internal TokenSetListItem(JSToken[] tokens, NoSkipTokenSet.TokenSetListItem next)
            {
                this._next = next;
                this._tokens = tokens;
            }
        }
    }
}

