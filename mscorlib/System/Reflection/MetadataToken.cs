namespace System.Reflection
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    internal struct MetadataToken
    {
        public int Value;
        public static implicit operator int(MetadataToken token)
        {
            return token.Value;
        }

        public static implicit operator MetadataToken(int token)
        {
            return new MetadataToken(token);
        }

        public static bool IsTokenOfType(int token, params MetadataTokenType[] types)
        {
            for (int i = 0; i < types.Length; i++)
            {
                if ((token & ((int) 0xff000000L)) == types[i])
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsNullToken(int token)
        {
            return ((token & 0xffffff) == 0);
        }

        public MetadataToken(int token)
        {
            this.Value = token;
        }

        public bool IsGlobalTypeDefToken
        {
            get
            {
                return (this.Value == 0x2000001);
            }
        }
        public MetadataTokenType TokenType
        {
            get
            {
                return (((MetadataTokenType) this.Value) & ((MetadataTokenType) ((int) 0xff000000L)));
            }
        }
        public bool IsTypeRef
        {
            get
            {
                return (this.TokenType == MetadataTokenType.TypeRef);
            }
        }
        public bool IsTypeDef
        {
            get
            {
                return (this.TokenType == MetadataTokenType.TypeDef);
            }
        }
        public bool IsFieldDef
        {
            get
            {
                return (this.TokenType == MetadataTokenType.FieldDef);
            }
        }
        public bool IsMethodDef
        {
            get
            {
                return (this.TokenType == MetadataTokenType.MethodDef);
            }
        }
        public bool IsMemberRef
        {
            get
            {
                return (this.TokenType == MetadataTokenType.MemberRef);
            }
        }
        public bool IsEvent
        {
            get
            {
                return (this.TokenType == MetadataTokenType.Event);
            }
        }
        public bool IsProperty
        {
            get
            {
                return (this.TokenType == MetadataTokenType.Property);
            }
        }
        public bool IsParamDef
        {
            get
            {
                return (this.TokenType == MetadataTokenType.ParamDef);
            }
        }
        public bool IsTypeSpec
        {
            get
            {
                return (this.TokenType == MetadataTokenType.TypeSpec);
            }
        }
        public bool IsMethodSpec
        {
            get
            {
                return (this.TokenType == MetadataTokenType.MethodSpec);
            }
        }
        public bool IsString
        {
            get
            {
                return (this.TokenType == MetadataTokenType.String);
            }
        }
        public bool IsSignature
        {
            get
            {
                return (this.TokenType == MetadataTokenType.Signature);
            }
        }
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "0x{0:x8}", new object[] { this.Value });
        }
    }
}

