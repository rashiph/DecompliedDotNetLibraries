namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared.LanguageParser;
    using System;
    using System.IO;
    using System.Text;

    internal static class CSharpParserUtilities
    {
        private static ExtractedClassName Extract(CSharpTokenizer tokens)
        {
            ParseState state = new ParseState();
            ExtractedClassName name = new ExtractedClassName();
            foreach (Token token in tokens)
            {
                if (token is KeywordToken)
                {
                    state.Reset();
                    if (token.InnerText == "namespace")
                    {
                        state.ResolvingNamespace = true;
                        if (state.InsideConditionalDirective)
                        {
                            name.IsInsideConditionalBlock = true;
                        }
                    }
                    else if (token.InnerText == "class")
                    {
                        state.ResolvingClass = true;
                        if (state.InsideConditionalDirective)
                        {
                            name.IsInsideConditionalBlock = true;
                        }
                    }
                }
                else if (token is CSharpTokenizer.OpenScopeToken)
                {
                    state.PushNamespacePart(state.Namespace);
                    state.Reset();
                }
                else if (token is CSharpTokenizer.CloseScopeToken)
                {
                    state.Reset();
                    state.PopNamespacePart();
                }
                else if (token is OperatorOrPunctuatorToken)
                {
                    if (state.ResolvingNamespace && (token.InnerText == "."))
                    {
                        state.Namespace = state.Namespace + ".";
                    }
                }
                else if (token is IdentifierToken)
                {
                    if (!state.ResolvingNamespace)
                    {
                        if (state.ResolvingClass)
                        {
                            name.Name = state.ComposeQualifiedClassName(token.InnerText);
                            return name;
                        }
                    }
                    else
                    {
                        state.Namespace = state.Namespace + token.InnerText;
                    }
                }
                else if (token is OpenConditionalDirectiveToken)
                {
                    state.OpenConditionalDirective();
                }
                else if (token is CloseConditionalDirectiveToken)
                {
                    state.CloseConditionalDirective();
                }
            }
            return name;
        }

        internal static ExtractedClassName GetFirstClassNameFullyQualified(Stream binaryStream)
        {
            try
            {
                CSharpTokenizer tokens = new CSharpTokenizer(binaryStream, false);
                return Extract(tokens);
            }
            catch (DecoderFallbackException)
            {
                CSharpTokenizer tokenizer2 = new CSharpTokenizer(binaryStream, true);
                return Extract(tokenizer2);
            }
        }
    }
}

