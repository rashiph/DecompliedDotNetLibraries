namespace Microsoft.JScript
{
    using System;

    internal sealed class JSKeyword
    {
        private int length;
        private string name;
        private JSKeyword next;
        private JSToken token;

        private JSKeyword(JSToken token, string name)
        {
            this.name = name;
            this.next = null;
            this.token = token;
            this.length = this.name.Length;
        }

        private JSKeyword(JSToken token, string name, JSKeyword next)
        {
            this.name = name;
            this.next = next;
            this.token = token;
            this.length = this.name.Length;
        }

        internal static string CanBeIdentifier(JSToken keyword)
        {
            switch (keyword)
            {
                case JSToken.Package:
                    return "package";

                case JSToken.Internal:
                    return "internal";

                case JSToken.Abstract:
                    return "abstract";

                case JSToken.Public:
                    return "public";

                case JSToken.Static:
                    return "static";

                case JSToken.Private:
                    return "private";

                case JSToken.Protected:
                    return "protected";

                case JSToken.Final:
                    return "final";

                case JSToken.Event:
                    return "event";

                case JSToken.Void:
                    return "void";

                case JSToken.Get:
                    return "get";

                case JSToken.Implements:
                    return "implements";

                case JSToken.Interface:
                    return "interface";

                case JSToken.Set:
                    return "set";

                case JSToken.Assert:
                    return "assert";

                case JSToken.Boolean:
                    return "boolean";

                case JSToken.Byte:
                    return "byte";

                case JSToken.Char:
                    return "char";

                case JSToken.Decimal:
                    return "decimal";

                case JSToken.Double:
                    return "double";

                case JSToken.Enum:
                    return "enum";

                case JSToken.Ensure:
                    return "ensure";

                case JSToken.Float:
                    return "float";

                case JSToken.Goto:
                    return "goto";

                case JSToken.Int:
                    return "int";

                case JSToken.Invariant:
                    return "invariant";

                case JSToken.Long:
                    return "long";

                case JSToken.Namespace:
                    return "namespace";

                case JSToken.Native:
                    return "native";

                case JSToken.Require:
                    return "require";

                case JSToken.Sbyte:
                    return "sbyte";

                case JSToken.Short:
                    return "short";

                case JSToken.Synchronized:
                    return "synchronized";

                case JSToken.Transient:
                    return "transient";

                case JSToken.Throws:
                    return "throws";

                case JSToken.Volatile:
                    return "volatile";

                case JSToken.Ushort:
                    return "ushort";

                case JSToken.Uint:
                    return "uint";

                case JSToken.Ulong:
                    return "ulong";

                case JSToken.Use:
                    return "use";
            }
            return null;
        }

        internal JSToken GetKeyword(Context token, int length)
        {
            for (JSKeyword next = this; next != null; next = next.next)
            {
                if (length == next.length)
                {
                    int num = 1;
                    for (int i = token.startPos + 1; num < length; i++)
                    {
                        char ch = next.name[num];
                        char ch2 = token.source_string[i];
                        if (ch != ch2)
                        {
                            if (ch2 < ch)
                            {
                                return JSToken.Identifier;
                            }
                            next = next.next;
                            continue;
                        }
                        num++;
                    }
                    return next.token;
                }
                if (length < next.length)
                {
                    return JSToken.Identifier;
                }
            }
            return JSToken.Identifier;
        }

        internal static JSKeyword[] InitKeywords()
        {
            JSKeyword[] keywordArray = new JSKeyword[0x1a];
            JSKeyword next = new JSKeyword(JSToken.Abstract, "abstract");
            next = new JSKeyword(JSToken.Assert, "assert", next);
            keywordArray[0] = next;
            next = new JSKeyword(JSToken.Boolean, "boolean");
            next = new JSKeyword(JSToken.Break, "break", next);
            next = new JSKeyword(JSToken.Byte, "byte", next);
            keywordArray[1] = next;
            next = new JSKeyword(JSToken.Continue, "continue");
            next = new JSKeyword(JSToken.Const, "const", next);
            next = new JSKeyword(JSToken.Class, "class", next);
            next = new JSKeyword(JSToken.Catch, "catch", next);
            next = new JSKeyword(JSToken.Char, "char", next);
            next = new JSKeyword(JSToken.Case, "case", next);
            keywordArray[2] = next;
            next = new JSKeyword(JSToken.Debugger, "debugger");
            next = new JSKeyword(JSToken.Default, "default", next);
            next = new JSKeyword(JSToken.Double, "double", next);
            next = new JSKeyword(JSToken.Delete, "delete", next);
            next = new JSKeyword(JSToken.Do, "do", next);
            keywordArray[3] = next;
            next = new JSKeyword(JSToken.Extends, "extends");
            next = new JSKeyword(JSToken.Export, "export", next);
            next = new JSKeyword(JSToken.Ensure, "ensure", next);
            next = new JSKeyword(JSToken.Event, "event", next);
            next = new JSKeyword(JSToken.Enum, "enum", next);
            next = new JSKeyword(JSToken.Else, "else", next);
            keywordArray[4] = next;
            next = new JSKeyword(JSToken.Function, "function");
            next = new JSKeyword(JSToken.Finally, "finally", next);
            next = new JSKeyword(JSToken.Float, "float", next);
            next = new JSKeyword(JSToken.Final, "final", next);
            next = new JSKeyword(JSToken.False, "false", next);
            next = new JSKeyword(JSToken.For, "for", next);
            keywordArray[5] = next;
            next = new JSKeyword(JSToken.Goto, "goto");
            next = new JSKeyword(JSToken.Get, "get", next);
            keywordArray[6] = next;
            next = new JSKeyword(JSToken.Instanceof, "instanceof");
            next = new JSKeyword(JSToken.Implements, "implements", next);
            next = new JSKeyword(JSToken.Invariant, "invariant", next);
            next = new JSKeyword(JSToken.Interface, "interface", next);
            next = new JSKeyword(JSToken.Internal, "internal", next);
            next = new JSKeyword(JSToken.Import, "import", next);
            next = new JSKeyword(JSToken.Int, "int", next);
            next = new JSKeyword(JSToken.In, "in", next);
            next = new JSKeyword(JSToken.If, "if", next);
            keywordArray[8] = next;
            next = new JSKeyword(JSToken.Long, "long");
            keywordArray[11] = next;
            next = new JSKeyword(JSToken.Namespace, "namespace");
            next = new JSKeyword(JSToken.Native, "native", next);
            next = new JSKeyword(JSToken.Null, "null", next);
            next = new JSKeyword(JSToken.New, "new", next);
            keywordArray[13] = next;
            next = new JSKeyword(JSToken.Protected, "protected");
            next = new JSKeyword(JSToken.Private, "private", next);
            next = new JSKeyword(JSToken.Package, "package", next);
            next = new JSKeyword(JSToken.Public, "public", next);
            keywordArray[15] = next;
            next = new JSKeyword(JSToken.Require, "require");
            next = new JSKeyword(JSToken.Return, "return", next);
            keywordArray[0x11] = next;
            next = new JSKeyword(JSToken.Synchronized, "synchronized");
            next = new JSKeyword(JSToken.Switch, "switch", next);
            next = new JSKeyword(JSToken.Static, "static", next);
            next = new JSKeyword(JSToken.Super, "super", next);
            next = new JSKeyword(JSToken.Short, "short", next);
            next = new JSKeyword(JSToken.Set, "set", next);
            keywordArray[0x12] = next;
            next = new JSKeyword(JSToken.Transient, "transient");
            next = new JSKeyword(JSToken.Typeof, "typeof", next);
            next = new JSKeyword(JSToken.Throws, "throws", next);
            next = new JSKeyword(JSToken.Throw, "throw", next);
            next = new JSKeyword(JSToken.True, "true", next);
            next = new JSKeyword(JSToken.This, "this", next);
            next = new JSKeyword(JSToken.Try, "try", next);
            keywordArray[0x13] = next;
            next = new JSKeyword(JSToken.Volatile, "volatile");
            next = new JSKeyword(JSToken.Void, "void", next);
            next = new JSKeyword(JSToken.Var, "var", next);
            keywordArray[0x15] = next;
            next = new JSKeyword(JSToken.Use, "use");
            keywordArray[20] = next;
            next = new JSKeyword(JSToken.While, "while");
            next = new JSKeyword(JSToken.With, "with", next);
            keywordArray[0x16] = next;
            return keywordArray;
        }
    }
}

