namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared.LanguageParser;
    using System;
    using System.IO;
    using System.Text;

    internal static class VisualBasicParserUtilities
    {
        private static ExtractedClassName Extract(VisualBasicTokenizer tokens)
        {
            ParseState state = new ParseState();
            ExtractedClassName name = new ExtractedClassName();
            foreach (Token token in tokens)
            {
                if (token is KeywordToken)
                {
                    state.Reset();
                    if (token.EqualsIgnoreCase("namespace"))
                    {
                        state.ResolvingNamespace = true;
                        if (state.InsideConditionalDirective)
                        {
                            name.IsInsideConditionalBlock = true;
                        }
                    }
                    else if (token.EqualsIgnoreCase("class"))
                    {
                        state.ResolvingClass = true;
                        if (state.InsideConditionalDirective)
                        {
                            name.IsInsideConditionalBlock = true;
                        }
                    }
                    else if (token.EqualsIgnoreCase("end"))
                    {
                        state.PopNamespacePart();
                    }
                }
                else if (token is VisualBasicTokenizer.LineTerminatorToken)
                {
                    if (state.ResolvingNamespace)
                    {
                        state.PushNamespacePart(state.Namespace);
                    }
                    state.Reset();
                }
                else if (token is VisualBasicTokenizer.SeparatorToken)
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
                VisualBasicTokenizer tokens = new VisualBasicTokenizer(binaryStream, false);
                return Extract(tokens);
            }
            catch (DecoderFallbackException)
            {
                VisualBasicTokenizer tokenizer2 = new VisualBasicTokenizer(binaryStream, true);
                return Extract(tokenizer2);
            }
        }
    }
}

