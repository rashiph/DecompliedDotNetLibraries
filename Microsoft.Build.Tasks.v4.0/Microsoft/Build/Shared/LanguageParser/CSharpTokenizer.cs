namespace Microsoft.Build.Shared.LanguageParser
{
    using System;
    using System.Collections;
    using System.IO;

    internal sealed class CSharpTokenizer : IEnumerable
    {
        private Stream binaryStream;
        private bool forceANSI;

        internal CSharpTokenizer(Stream binaryStream, bool forceANSI)
        {
            this.binaryStream = binaryStream;
            this.forceANSI = forceANSI;
        }

        public IEnumerator GetEnumerator()
        {
            return new CSharpTokenEnumerator(this.binaryStream, this.forceANSI);
        }

        internal class CharLiteralToken : Token
        {
        }

        internal class CloseScopeToken : OperatorOrPunctuatorToken
        {
        }

        internal class EndOfFileInsideCommentToken : SyntaxErrorToken
        {
        }

        internal class EndOfFileInsideStringToken : SyntaxErrorToken
        {
        }

        internal class NewlineInsideStringToken : SyntaxErrorToken
        {
        }

        internal class NullLiteralToken : Token
        {
        }

        internal class OpenScopeToken : OperatorOrPunctuatorToken
        {
        }

        internal class UnrecognizedStringEscapeToken : SyntaxErrorToken
        {
        }
    }
}

