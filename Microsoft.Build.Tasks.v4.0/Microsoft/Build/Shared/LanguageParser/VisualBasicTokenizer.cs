namespace Microsoft.Build.Shared.LanguageParser
{
    using System;
    using System.Collections;
    using System.IO;

    internal sealed class VisualBasicTokenizer : IEnumerable
    {
        private Stream binaryStream;
        private bool forceANSI;

        internal VisualBasicTokenizer(Stream binaryStream, bool forceANSI)
        {
            this.binaryStream = binaryStream;
            this.forceANSI = forceANSI;
        }

        public IEnumerator GetEnumerator()
        {
            return new VisualBasicTokenEnumerator(this.binaryStream, this.forceANSI);
        }

        internal class ExpectedValidOctalDigitToken : SyntaxErrorToken
        {
        }

        internal class LineContinuationToken : WhitespaceToken
        {
        }

        internal class LineTerminatorToken : Token
        {
        }

        internal class OctalIntegerLiteralToken : IntegerLiteralToken
        {
        }

        internal class SeparatorToken : Token
        {
        }
    }
}

