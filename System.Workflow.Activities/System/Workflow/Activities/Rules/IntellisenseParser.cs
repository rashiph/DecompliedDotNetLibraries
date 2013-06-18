namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Collections.Generic;

    internal class IntellisenseParser
    {
        private int tokenIndex;
        private List<Token> tokens = new List<Token>();

        internal IntellisenseParser(string inputString)
        {
            Scanner scanner = new Scanner(inputString);
            this.tokens.Add(new Token(TokenID.EndOfInput, 0, null));
            scanner.TokenizeForIntellisense(this.tokens);
        }

        internal ParserContext BackParse()
        {
            this.tokenIndex = this.tokens.Count - 1;
            if (this.tokenIndex < 0)
            {
                return null;
            }
            Token currentToken = this.CurrentToken;
            bool flag = false;
            if (currentToken.TokenID == TokenID.EndOfInput)
            {
                currentToken = this.PrevToken();
            }
            int tokenIndex = this.tokenIndex;
            if (((currentToken.TokenID == TokenID.Identifier) && (((string) currentToken.Value).Length == 1)) && (this.PrevToken().TokenID != TokenID.Dot))
            {
                flag = true;
            }
            else if (currentToken.TokenID == TokenID.Dot)
            {
                flag = this.BackParsePostfix();
            }
            else if ((currentToken.TokenID == TokenID.LParen) && (this.PrevToken().TokenID == TokenID.Identifier))
            {
                if (this.PrevToken().TokenID == TokenID.Dot)
                {
                    flag = this.BackParsePostfix();
                }
                else
                {
                    flag = true;
                }
                if (flag && (this.CurrentToken.TokenID == TokenID.New))
                {
                    this.PrevToken();
                }
            }
            if (!flag)
            {
                return null;
            }
            List<Token> range = this.tokens.GetRange(this.tokenIndex + 1, tokenIndex - this.tokenIndex);
            range.Add(new Token(TokenID.EndOfInput, 0, null));
            return new ParserContext(range);
        }

        private bool BackParseMatchingDelimiter(TokenID openDelimiter)
        {
            TokenID tokenID = this.CurrentToken.TokenID;
            int num = 1;
            for (Token token = this.PrevToken(); token.TokenID != TokenID.EndOfInput; token = this.PrevToken())
            {
                if (token.TokenID == tokenID)
                {
                    num++;
                }
                else if (token.TokenID == openDelimiter)
                {
                    num--;
                    if (num == 0)
                    {
                        this.PrevToken();
                        break;
                    }
                }
            }
            return (num == 0);
        }

        private bool BackParsePostfix()
        {
            while (this.CurrentToken.TokenID == TokenID.Dot)
            {
                switch (this.PrevToken().TokenID)
                {
                    case TokenID.This:
                        this.PrevToken();
                        return true;

                    case TokenID.TypeName:
                    case TokenID.Identifier:
                    {
                        this.PrevToken();
                        continue;
                    }
                    case TokenID.RBracket:
                        break;

                    case TokenID.RParen:
                        if (!this.BackParseMatchingDelimiter(TokenID.LParen))
                        {
                            return false;
                        }
                        if (this.CurrentToken.TokenID == TokenID.Identifier)
                        {
                            this.PrevToken();
                            continue;
                        }
                        return true;

                    case TokenID.Greater:
                        goto Label_00E3;

                    default:
                        return false;
                }
                do
                {
                    if (!this.BackParseMatchingDelimiter(TokenID.LBracket))
                    {
                        return false;
                    }
                }
                while (this.CurrentToken.TokenID == TokenID.RBracket);
                if (this.CurrentToken.TokenID == TokenID.Identifier)
                {
                    this.PrevToken();
                    continue;
                }
                if (this.CurrentToken.TokenID != TokenID.RParen)
                {
                    return false;
                }
                if (!this.BackParseMatchingDelimiter(TokenID.LParen))
                {
                    return false;
                }
                if (this.CurrentToken.TokenID == TokenID.Identifier)
                {
                    this.PrevToken();
                    continue;
                }
                return true;
            Label_00E3:
                if (this.BackParseMatchingDelimiter(TokenID.Less) && (this.CurrentToken.TokenID == TokenID.Identifier))
                {
                    this.PrevToken();
                }
                else
                {
                    return false;
                }
            }
            if (this.CurrentToken.TokenID == TokenID.New)
            {
                this.PrevToken();
            }
            return true;
        }

        private Token PrevToken()
        {
            if (this.tokenIndex > 0)
            {
                this.tokenIndex--;
            }
            return this.CurrentToken;
        }

        private Token CurrentToken
        {
            get
            {
                return this.tokens[this.tokenIndex];
            }
        }
    }
}

