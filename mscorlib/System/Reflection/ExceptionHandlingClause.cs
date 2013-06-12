namespace System.Reflection
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class ExceptionHandlingClause
    {
        private int m_catchMetadataToken;
        private int m_filterOffset;
        private ExceptionHandlingClauseOptions m_flags;
        private int m_handlerLength;
        private int m_handlerOffset;
        private MethodBody m_methodBody;
        private int m_tryLength;
        private int m_tryOffset;

        protected ExceptionHandlingClause()
        {
        }

        public override string ToString()
        {
            if (this.Flags == ExceptionHandlingClauseOptions.Clause)
            {
                return string.Format(CultureInfo.CurrentUICulture, "Flags={0}, TryOffset={1}, TryLength={2}, HandlerOffset={3}, HandlerLength={4}, CatchType={5}", new object[] { this.Flags, this.TryOffset, this.TryLength, this.HandlerOffset, this.HandlerLength, this.CatchType });
            }
            if (this.Flags == ExceptionHandlingClauseOptions.Filter)
            {
                return string.Format(CultureInfo.CurrentUICulture, "Flags={0}, TryOffset={1}, TryLength={2}, HandlerOffset={3}, HandlerLength={4}, FilterOffset={5}", new object[] { this.Flags, this.TryOffset, this.TryLength, this.HandlerOffset, this.HandlerLength, this.FilterOffset });
            }
            return string.Format(CultureInfo.CurrentUICulture, "Flags={0}, TryOffset={1}, TryLength={2}, HandlerOffset={3}, HandlerLength={4}", new object[] { this.Flags, this.TryOffset, this.TryLength, this.HandlerOffset, this.HandlerLength });
        }

        public virtual Type CatchType
        {
            get
            {
                if (this.m_flags != ExceptionHandlingClauseOptions.Clause)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("Arg_EHClauseNotClause"));
                }
                Type type = null;
                if (!MetadataToken.IsNullToken(this.m_catchMetadataToken))
                {
                    Type declaringType = this.m_methodBody.m_methodBase.DeclaringType;
                    type = ((declaringType == null) ? this.m_methodBody.m_methodBase.Module : declaringType.Module).ResolveType(this.m_catchMetadataToken, (declaringType == null) ? null : declaringType.GetGenericArguments(), (this.m_methodBody.m_methodBase is MethodInfo) ? this.m_methodBody.m_methodBase.GetGenericArguments() : null);
                }
                return type;
            }
        }

        public virtual int FilterOffset
        {
            get
            {
                if (this.m_flags != ExceptionHandlingClauseOptions.Filter)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("Arg_EHClauseNotFilter"));
                }
                return this.m_filterOffset;
            }
        }

        public virtual ExceptionHandlingClauseOptions Flags
        {
            get
            {
                return this.m_flags;
            }
        }

        public virtual int HandlerLength
        {
            get
            {
                return this.m_handlerLength;
            }
        }

        public virtual int HandlerOffset
        {
            get
            {
                return this.m_handlerOffset;
            }
        }

        public virtual int TryLength
        {
            get
            {
                return this.m_tryLength;
            }
        }

        public virtual int TryOffset
        {
            get
            {
                return this.m_tryOffset;
            }
        }
    }
}

