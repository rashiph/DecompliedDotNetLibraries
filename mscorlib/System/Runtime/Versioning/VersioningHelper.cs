namespace System.Runtime.Versioning
{
    using Microsoft.Win32;
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Text;

    public static class VersioningHelper
    {
        private const ResourceScope ResTypeMask = (ResourceScope.Library | ResourceScope.AppDomain | ResourceScope.Process | ResourceScope.Machine);
        private const ResourceScope VisibilityMask = (ResourceScope.Assembly | ResourceScope.Private);

        [SecuritySafeCritical]
        private static string GetCLRInstanceString()
        {
            return GetRuntimeId().ToString(CultureInfo.InvariantCulture);
        }

        private static SxSRequirements GetRequirements(ResourceScope consumeAsScope, ResourceScope calleeScope)
        {
            ResourceScope scope3;
            SxSRequirements none = SxSRequirements.None;
            switch ((calleeScope & (ResourceScope.Library | ResourceScope.AppDomain | ResourceScope.Process | ResourceScope.Machine)))
            {
                case ResourceScope.Machine:
                    switch ((consumeAsScope & (ResourceScope.Library | ResourceScope.AppDomain | ResourceScope.Process | ResourceScope.Machine)))
                    {
                        case ResourceScope.Machine:
                            goto Label_00A6;

                        case ResourceScope.Process:
                            none |= SxSRequirements.ProcessID;
                            goto Label_00A6;

                        case ResourceScope.AppDomain:
                            none |= SxSRequirements.CLRInstanceID | SxSRequirements.ProcessID | SxSRequirements.AppDomainID;
                            goto Label_00A6;
                    }
                    break;

                case ResourceScope.Process:
                    if ((consumeAsScope & ResourceScope.AppDomain) != ResourceScope.None)
                    {
                        none |= SxSRequirements.CLRInstanceID | SxSRequirements.AppDomainID;
                    }
                    goto Label_00A6;

                case ResourceScope.AppDomain:
                    goto Label_00A6;

                default:
                    throw new ArgumentException(Environment.GetResourceString("Argument_BadResourceScopeTypeBits", new object[] { calleeScope }), "calleeScope");
            }
            throw new ArgumentException(Environment.GetResourceString("Argument_BadResourceScopeTypeBits", new object[] { consumeAsScope }), "consumeAsScope");
        Label_00A6:
            scope3 = calleeScope & (ResourceScope.Assembly | ResourceScope.Private);
            if (scope3 != ResourceScope.None)
            {
                if (scope3 != ResourceScope.Private)
                {
                    if (scope3 != ResourceScope.Assembly)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_BadResourceScopeVisibilityBits", new object[] { calleeScope }), "calleeScope");
                    }
                    if ((consumeAsScope & ResourceScope.Private) != ResourceScope.None)
                    {
                        none |= SxSRequirements.TypeName;
                    }
                }
                return none;
            }
            switch ((consumeAsScope & (ResourceScope.Assembly | ResourceScope.Private)))
            {
                case ResourceScope.None:
                    return none;

                case ResourceScope.Private:
                    return (none | (SxSRequirements.TypeName | SxSRequirements.AssemblyName));

                case ResourceScope.Assembly:
                    return (none | SxSRequirements.AssemblyName);
            }
            throw new ArgumentException(Environment.GetResourceString("Argument_BadResourceScopeVisibilityBits", new object[] { consumeAsScope }), "consumeAsScope");
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        private static extern int GetRuntimeId();
        public static string MakeVersionSafeName(string name, ResourceScope from, ResourceScope to)
        {
            return MakeVersionSafeName(name, from, to, null);
        }

        [SecuritySafeCritical]
        public static string MakeVersionSafeName(string name, ResourceScope from, ResourceScope to, Type type)
        {
            ResourceScope scope = from & (ResourceScope.Library | ResourceScope.AppDomain | ResourceScope.Process | ResourceScope.Machine);
            ResourceScope scope2 = to & (ResourceScope.Library | ResourceScope.AppDomain | ResourceScope.Process | ResourceScope.Machine);
            if (scope > scope2)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ResourceScopeWrongDirection", new object[] { scope, scope2 }), "from");
            }
            SxSRequirements requirements = GetRequirements(to, from);
            if (((requirements & (SxSRequirements.TypeName | SxSRequirements.AssemblyName)) != SxSRequirements.None) && (type == null))
            {
                throw new ArgumentNullException("type", Environment.GetResourceString("ArgumentNull_TypeRequiredByResourceScope"));
            }
            StringBuilder builder = new StringBuilder(name);
            char ch = '_';
            if ((requirements & SxSRequirements.ProcessID) != SxSRequirements.None)
            {
                builder.Append(ch);
                builder.Append('p');
                builder.Append(Win32Native.GetCurrentProcessId());
            }
            if ((requirements & SxSRequirements.CLRInstanceID) != SxSRequirements.None)
            {
                string cLRInstanceString = GetCLRInstanceString();
                builder.Append(ch);
                builder.Append('r');
                builder.Append(cLRInstanceString);
            }
            if ((requirements & SxSRequirements.AppDomainID) != SxSRequirements.None)
            {
                builder.Append(ch);
                builder.Append("ad");
                builder.Append(AppDomain.CurrentDomain.Id);
            }
            if ((requirements & SxSRequirements.TypeName) != SxSRequirements.None)
            {
                builder.Append(ch);
                builder.Append(type.Name);
            }
            if ((requirements & SxSRequirements.AssemblyName) != SxSRequirements.None)
            {
                builder.Append(ch);
                builder.Append(type.Assembly.FullName);
            }
            return builder.ToString();
        }
    }
}

