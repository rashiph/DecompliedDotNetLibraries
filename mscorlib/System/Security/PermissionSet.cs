namespace System.Security
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Security.Util;
    using System.Text;
    using System.Threading;

    [Serializable, ComVisible(true), StrongNameIdentityPermission(SecurityAction.InheritanceDemand, Name="mscorlib", PublicKey="0x00000000000000000400000000000000")]
    public class PermissionSet : ISecurityEncodable, ICollection, IEnumerable, IStackWalk, IDeserializationCallback
    {
        [OptionalField(VersionAdded=2)]
        private bool m_allPermissionsDecoded;
        [OptionalField(VersionAdded=2)]
        private bool m_canUnrestrictedOverride;
        [NonSerialized]
        private bool m_CheckedForNonCas;
        [NonSerialized]
        private bool m_ContainsCas;
        [NonSerialized]
        private bool m_ContainsNonCas;
        [OptionalField(VersionAdded=2)]
        private bool m_ignoreTypeLoadFailures;
        private TokenBasedSet m_normalPermSet;
        [OptionalField(VersionAdded=2)]
        internal TokenBasedSet m_permSet;
        [NonSerialized]
        private TokenBasedSet m_permSetSaved;
        [OptionalField(VersionAdded=2)]
        private string m_serializedPermissionSet;
        private bool m_Unrestricted;
        private TokenBasedSet m_unrestrictedPermSet;
        private bool readableonly;
        internal static readonly PermissionSet s_fullTrust = new PermissionSet(true);
        private const string s_str_IPermission = "IPermission";
        private const string s_str_Permission = "Permission";
        private const string s_str_PermissionIntersection = "PermissionIntersection";
        private const string s_str_PermissionSet = "PermissionSet";
        private const string s_str_PermissionUnion = "PermissionUnion";
        private const string s_str_PermissionUnrestrictedIntersection = "PermissionUnrestrictedIntersection";
        private const string s_str_PermissionUnrestrictedUnion = "PermissionUnrestrictedUnion";
        private const string s_str_Unrestricted = "Unrestricted";

        internal PermissionSet()
        {
            this.Reset();
            this.m_Unrestricted = true;
        }

        internal PermissionSet(bool fUnrestricted) : this()
        {
            this.SetUnrestricted(fUnrestricted);
        }

        public PermissionSet(PermissionState state) : this()
        {
            if (state == PermissionState.Unrestricted)
            {
                this.SetUnrestricted(true);
            }
            else
            {
                if (state != PermissionState.None)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidPermissionState"));
                }
                this.SetUnrestricted(false);
            }
        }

        public PermissionSet(PermissionSet permSet) : this()
        {
            if (permSet == null)
            {
                this.Reset();
            }
            else
            {
                this.m_Unrestricted = permSet.m_Unrestricted;
                this.m_CheckedForNonCas = permSet.m_CheckedForNonCas;
                this.m_ContainsCas = permSet.m_ContainsCas;
                this.m_ContainsNonCas = permSet.m_ContainsNonCas;
                this.m_ignoreTypeLoadFailures = permSet.m_ignoreTypeLoadFailures;
                if (permSet.m_permSet != null)
                {
                    this.m_permSet = new TokenBasedSet(permSet.m_permSet);
                    for (int i = this.m_permSet.GetStartingIndex(); i <= this.m_permSet.GetMaxUsedIndex(); i++)
                    {
                        object item = this.m_permSet.GetItem(i);
                        IPermission permission = item as IPermission;
                        ISecurityElementFactory factory = item as ISecurityElementFactory;
                        if (permission != null)
                        {
                            this.m_permSet.SetItem(i, permission.Copy());
                        }
                        else if (factory != null)
                        {
                            this.m_permSet.SetItem(i, factory.Copy());
                        }
                    }
                }
            }
        }

        private PermissionSet(object trash, object junk)
        {
            this.m_Unrestricted = false;
        }

        public IPermission AddPermission(IPermission perm)
        {
            return this.AddPermissionImpl(perm);
        }

        [SecuritySafeCritical]
        protected virtual IPermission AddPermissionImpl(IPermission perm)
        {
            if (perm == null)
            {
                return null;
            }
            this.m_CheckedForNonCas = false;
            PermissionToken token = PermissionToken.GetToken(perm);
            if (this.IsUnrestricted() && ((token.m_type & PermissionTokenType.IUnrestricted) != 0))
            {
                Type type = perm.GetType();
                object[] args = new object[] { PermissionState.Unrestricted };
                return (IPermission) Activator.CreateInstance(type, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, args, null);
            }
            this.CheckSet();
            IPermission permission = this.GetPermission(token.m_index);
            if (permission != null)
            {
                IPermission item = permission.Union(perm);
                this.m_permSet.SetItem(token.m_index, item);
                return item;
            }
            this.m_permSet.SetItem(token.m_index, perm);
            return perm;
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public void Assert()
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            SecurityRuntime.Assert(this, ref lookForMyCaller);
        }

        internal bool CheckAssertion(PermissionSet target)
        {
            IPermission permission;
            return this.IsSubsetOfHelper(target, IsSubsetOfType.CheckAssertion, out permission, true);
        }

        internal void CheckDecoded(int index)
        {
            if (!this.m_allPermissionsDecoded && (this.m_permSet != null))
            {
                this.GetPermission(index);
            }
        }

        internal void CheckDecoded(PermissionSet demandedSet)
        {
            if (!this.m_allPermissionsDecoded && (this.m_permSet != null))
            {
                PermissionSetEnumeratorInternal enumeratorInternal = demandedSet.GetEnumeratorInternal();
                while (enumeratorInternal.MoveNext())
                {
                    this.CheckDecoded(enumeratorInternal.GetCurrentIndex());
                }
            }
        }

        internal void CheckDecoded(CodeAccessPermission demandedPerm, PermissionToken tokenDemandedPerm)
        {
            if (!this.m_allPermissionsDecoded && (this.m_permSet != null))
            {
                if (tokenDemandedPerm == null)
                {
                    tokenDemandedPerm = PermissionToken.GetToken(demandedPerm);
                }
                this.CheckDecoded(tokenDemandedPerm.m_index);
            }
        }

        internal bool CheckDemand(PermissionSet target, out IPermission firstPermThatFailed)
        {
            return this.IsSubsetOfHelper(target, IsSubsetOfType.CheckDemand, out firstPermThatFailed, true);
        }

        internal bool CheckDeny(PermissionSet deniedSet, out IPermission firstPermThatFailed)
        {
            firstPermThatFailed = null;
            if (((deniedSet != null) && !deniedSet.FastIsEmpty()) && !this.FastIsEmpty())
            {
                if (this.m_Unrestricted && deniedSet.m_Unrestricted)
                {
                    return false;
                }
                PermissionSetEnumeratorInternal internal2 = new PermissionSetEnumeratorInternal(this);
                while (internal2.MoveNext())
                {
                    CodeAccessPermission current = internal2.Current as CodeAccessPermission;
                    if ((current != null) && !current.IsSubsetOf(null))
                    {
                        if (deniedSet.m_Unrestricted)
                        {
                            firstPermThatFailed = current;
                            return false;
                        }
                        CodeAccessPermission permission = (CodeAccessPermission) deniedSet.GetPermission(internal2.GetCurrentIndex());
                        if (!current.CheckDeny(permission))
                        {
                            firstPermThatFailed = current;
                            return false;
                        }
                    }
                }
                if (this.m_Unrestricted)
                {
                    PermissionSetEnumeratorInternal internal3 = new PermissionSetEnumeratorInternal(deniedSet);
                    while (internal3.MoveNext())
                    {
                        if (internal3.Current is IPermission)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        internal bool CheckPermitOnly(PermissionSet target, out IPermission firstPermThatFailed)
        {
            return this.IsSubsetOfHelper(target, IsSubsetOfType.CheckPermitOnly, out firstPermThatFailed, true);
        }

        internal void CheckSet()
        {
            if (this.m_permSet == null)
            {
                this.m_permSet = new TokenBasedSet();
            }
        }

        internal bool Contains(IPermission perm)
        {
            if (perm == null)
            {
                return true;
            }
            if (this.m_Unrestricted)
            {
                return true;
            }
            if (this.FastIsEmpty())
            {
                return false;
            }
            PermissionToken token = PermissionToken.GetToken(perm);
            if (this.m_permSet.GetItem(token.m_index) != null)
            {
                IPermission target = this.GetPermission(token.m_index);
                if (target != null)
                {
                    return perm.IsSubsetOf(target);
                }
            }
            return perm.IsSubsetOf(null);
        }

        public bool ContainsNonCodeAccessPermissions()
        {
            if (!this.m_CheckedForNonCas)
            {
                lock (this)
                {
                    if (this.m_CheckedForNonCas)
                    {
                        return this.m_ContainsNonCas;
                    }
                    this.m_ContainsCas = false;
                    this.m_ContainsNonCas = false;
                    if (this.IsUnrestricted())
                    {
                        this.m_ContainsCas = true;
                    }
                    if (this.m_permSet != null)
                    {
                        PermissionSetEnumeratorInternal internal2 = new PermissionSetEnumeratorInternal(this);
                        while (internal2.MoveNext() && (!this.m_ContainsCas || !this.m_ContainsNonCas))
                        {
                            IPermission current = internal2.Current as IPermission;
                            if (current != null)
                            {
                                if (current is CodeAccessPermission)
                                {
                                    this.m_ContainsCas = true;
                                }
                                else
                                {
                                    this.m_ContainsNonCas = true;
                                }
                            }
                        }
                    }
                    this.m_CheckedForNonCas = true;
                }
            }
            return this.m_ContainsNonCas;
        }

        [Obsolete("This method is obsolete and shoud no longer be used.")]
        public static byte[] ConvertPermissionSet(string inFormat, byte[] inData, string outFormat)
        {
            throw new NotImplementedException();
        }

        public virtual PermissionSet Copy()
        {
            return new PermissionSet(this);
        }

        [SecuritySafeCritical]
        public virtual void CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            PermissionSetEnumeratorInternal internal2 = new PermissionSetEnumeratorInternal(this);
            while (internal2.MoveNext())
            {
                array.SetValue(internal2.Current, index++);
            }
        }

        internal PermissionSet CopyWithNoIdentityPermissions()
        {
            PermissionSet set = new PermissionSet(this);
            set.RemovePermission(typeof(GacIdentityPermission));
            set.RemovePermission(typeof(PublisherIdentityPermission));
            set.RemovePermission(typeof(StrongNameIdentityPermission));
            set.RemovePermission(typeof(UrlIdentityPermission));
            set.RemovePermission(typeof(ZoneIdentityPermission));
            return set;
        }

        internal static SecurityElement CreateEmptyPermissionSetXml()
        {
            SecurityElement element = new SecurityElement("PermissionSet");
            element.AddAttribute("class", "System.Security.PermissionSet");
            element.AddAttribute("version", "1");
            return element;
        }

        private IPermission CreatePerm(object obj)
        {
            return CreatePerm(obj, this.m_ignoreTypeLoadFailures);
        }

        internal static IPermission CreatePerm(object obj, bool ignoreTypeLoadFailures)
        {
            IEnumerator enumerator;
            SecurityElement element = obj as SecurityElement;
            ISecurityElementFactory factory = obj as ISecurityElementFactory;
            if ((element == null) && (factory != null))
            {
                element = factory.CreateSecurityElement();
            }
            IPermission target = null;
            switch (element.Tag)
            {
                case "PermissionUnion":
                    enumerator = element.Children.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        IPermission permission2 = CreatePerm((SecurityElement) enumerator.Current, ignoreTypeLoadFailures);
                        if (target != null)
                        {
                            target = target.Union(permission2);
                        }
                        else
                        {
                            target = permission2;
                        }
                    }
                    return target;

                case "PermissionIntersection":
                    enumerator = element.Children.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        IPermission permission3 = CreatePerm((SecurityElement) enumerator.Current, ignoreTypeLoadFailures);
                        if (target != null)
                        {
                            target = target.Intersect(permission3);
                        }
                        else
                        {
                            target = permission3;
                        }
                        if (target == null)
                        {
                            return null;
                        }
                    }
                    return target;

                case "PermissionUnrestrictedUnion":
                {
                    enumerator = element.Children.GetEnumerator();
                    bool flag = true;
                    while (enumerator.MoveNext())
                    {
                        IPermission perm = CreatePerm((SecurityElement) enumerator.Current, ignoreTypeLoadFailures);
                        if (perm != null)
                        {
                            if ((PermissionToken.GetToken(perm).m_type & PermissionTokenType.IUnrestricted) != 0)
                            {
                                target = XMLUtil.CreatePermission(GetPermissionElement((SecurityElement) enumerator.Current), PermissionState.Unrestricted, ignoreTypeLoadFailures);
                                flag = false;
                                return target;
                            }
                            if (flag)
                            {
                                target = perm;
                            }
                            else
                            {
                                target = perm.Union(target);
                            }
                            flag = false;
                        }
                    }
                    return target;
                }
                case "PermissionUnrestrictedIntersection":
                    enumerator = element.Children.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        IPermission permission5 = CreatePerm((SecurityElement) enumerator.Current, ignoreTypeLoadFailures);
                        if (permission5 == null)
                        {
                            return null;
                        }
                        if ((PermissionToken.GetToken(permission5).m_type & PermissionTokenType.IUnrestricted) != 0)
                        {
                            if (target != null)
                            {
                                target = permission5.Intersect(target);
                            }
                            else
                            {
                                target = permission5;
                            }
                        }
                        else
                        {
                            target = null;
                        }
                        if (target == null)
                        {
                            return null;
                        }
                    }
                    return target;

                case "IPermission":
                case "Permission":
                    return element.ToPermission(ignoreTypeLoadFailures);
            }
            return target;
        }

        internal IPermission CreatePermission(object obj, int index)
        {
            IPermission item = this.CreatePerm(obj);
            if (item == null)
            {
                return null;
            }
            if (this.m_Unrestricted)
            {
                item = null;
            }
            this.CheckSet();
            this.m_permSet.SetItem(index, item);
            if (item != null)
            {
                PermissionToken token = PermissionToken.GetToken(item);
                if ((token != null) && (token.m_index != index))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_UnableToGeneratePermissionSet"));
                }
            }
            return item;
        }

        private static byte[] CreateSerialized(object[] attrs, bool serialize, ref byte[] nonCasBlob, out PermissionSet casPset, HostProtectionResource fullTrustOnlyResources, bool allowEmptyPermissionSets)
        {
            casPset = null;
            PermissionSet nonCasPset = null;
            for (int i = 0; i < attrs.Length; i++)
            {
                if (attrs[i] is PermissionSetAttribute)
                {
                    PermissionSet permSet = ((PermissionSetAttribute) attrs[i]).CreatePermissionSet();
                    if (permSet == null)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_UnableToGeneratePermissionSet"));
                    }
                    PermissionSetEnumeratorInternal internal2 = new PermissionSetEnumeratorInternal(permSet);
                    while (internal2.MoveNext())
                    {
                        IPermission current = (IPermission) internal2.Current;
                        MergePermission(current, serialize, ref casPset, ref nonCasPset);
                    }
                    if (casPset == null)
                    {
                        casPset = new PermissionSet(false);
                    }
                    if (permSet.IsUnrestricted())
                    {
                        casPset.SetUnrestricted(true);
                    }
                }
                else
                {
                    MergePermission(((SecurityAttribute) attrs[i]).CreatePermission(), serialize, ref casPset, ref nonCasPset);
                }
            }
            if (casPset != null)
            {
                casPset.FilterHostProtectionPermissions(fullTrustOnlyResources, HostProtectionResource.None);
                casPset.ContainsNonCodeAccessPermissions();
                if (allowEmptyPermissionSets && casPset.IsEmpty())
                {
                    casPset = null;
                }
            }
            if (nonCasPset != null)
            {
                nonCasPset.FilterHostProtectionPermissions(fullTrustOnlyResources, HostProtectionResource.None);
                nonCasPset.ContainsNonCodeAccessPermissions();
                if (allowEmptyPermissionSets && nonCasPset.IsEmpty())
                {
                    nonCasPset = null;
                }
            }
            byte[] buffer = null;
            nonCasBlob = null;
            if (serialize)
            {
                if (casPset != null)
                {
                    buffer = casPset.EncodeXml();
                }
                if (nonCasPset != null)
                {
                    nonCasBlob = nonCasPset.EncodeXml();
                }
            }
            return buffer;
        }

        [Conditional("_DEBUG")]
        private static void DEBUG_COND_WRITE(bool exp, string str)
        {
        }

        [Conditional("_DEBUG")]
        private static void DEBUG_PRINTSTACK(Exception e)
        {
        }

        [Conditional("_DEBUG")]
        private static void DEBUG_WRITE(string str)
        {
        }

        private void DecodeAllPermissions()
        {
            if (this.m_permSet == null)
            {
                this.m_allPermissionsDecoded = true;
            }
            else
            {
                int maxUsedIndex = this.m_permSet.GetMaxUsedIndex();
                for (int i = 0; i <= maxUsedIndex; i++)
                {
                    this.GetPermission(i);
                }
                this.m_allPermissionsDecoded = true;
            }
        }

        private bool DecodeXml(byte[] data, HostProtectionResource fullTrustOnlyResources, HostProtectionResource inaccessibleResources)
        {
            if ((data != null) && (data.Length > 0))
            {
                this.FromXml(new Parser(data, Tokenizer.ByteTokenEncoding.UnicodeTokens).GetTopElement());
            }
            this.FilterHostProtectionPermissions(fullTrustOnlyResources, inaccessibleResources);
            this.DecodeAllPermissions();
            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public void Demand()
        {
            if (!this.FastIsEmpty())
            {
                this.ContainsNonCodeAccessPermissions();
                if (this.m_ContainsCas)
                {
                    StackCrawlMark lookForMyCallersCaller = StackCrawlMark.LookForMyCallersCaller;
                    CodeAccessSecurityEngine.Check(this.GetCasOnlySet(), ref lookForMyCallersCaller);
                }
                if (this.m_ContainsNonCas)
                {
                    this.DemandNonCAS();
                }
            }
        }

        [SecurityCritical]
        internal void DemandNonCAS()
        {
            this.ContainsNonCodeAccessPermissions();
            if (this.m_ContainsNonCas && (this.m_permSet != null))
            {
                this.CheckSet();
                for (int i = this.m_permSet.GetStartingIndex(); i <= this.m_permSet.GetMaxUsedIndex(); i++)
                {
                    IPermission permission = this.GetPermission(i);
                    if ((permission != null) && !(permission is CodeAccessPermission))
                    {
                        permission.Demand();
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical, Obsolete("Deny is obsolete and will be removed in a future release of the .NET Framework. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
        public void Deny()
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            SecurityRuntime.Deny(this, ref lookForMyCaller);
        }

        internal byte[] EncodeXml()
        {
            MemoryStream output = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(output, Encoding.Unicode);
            writer.Write(this.ToXml().ToString());
            writer.Flush();
            output.Position = 2L;
            int num = ((int) output.Length) - 2;
            byte[] buffer = new byte[num];
            output.Read(buffer, 0, buffer.Length);
            return buffer;
        }

        [ComVisible(false)]
        public override bool Equals(object obj)
        {
            PermissionSet set = obj as PermissionSet;
            if (set == null)
            {
                return false;
            }
            if (this.m_Unrestricted != set.m_Unrestricted)
            {
                return false;
            }
            this.CheckSet();
            set.CheckSet();
            this.DecodeAllPermissions();
            set.DecodeAllPermissions();
            int num = Math.Max(this.m_permSet.GetMaxUsedIndex(), set.m_permSet.GetMaxUsedIndex());
            for (int i = 0; i <= num; i++)
            {
                IPermission item = (IPermission) this.m_permSet.GetItem(i);
                IPermission permission2 = (IPermission) set.m_permSet.GetItem(i);
                if ((item != null) || (permission2 != null))
                {
                    if (item == null)
                    {
                        if (!permission2.IsSubsetOf(null))
                        {
                            return false;
                        }
                    }
                    else if (permission2 == null)
                    {
                        if (!item.IsSubsetOf(null))
                        {
                            return false;
                        }
                    }
                    else if (!item.Equals(permission2))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal bool FastIsEmpty()
        {
            if (this.m_Unrestricted)
            {
                return false;
            }
            if ((this.m_permSet != null) && !this.m_permSet.FastIsEmpty())
            {
                return false;
            }
            return true;
        }

        internal void FilterHostProtectionPermissions(HostProtectionResource fullTrustOnly, HostProtectionResource inaccessible)
        {
            HostProtectionPermission.protectedResources = fullTrustOnly;
            HostProtectionPermission permission = (HostProtectionPermission) this.GetPermission(HostProtectionPermission.GetTokenIndex());
            if (permission != null)
            {
                HostProtectionPermission perm = (HostProtectionPermission) permission.Intersect(new HostProtectionPermission(fullTrustOnly));
                if (perm == null)
                {
                    this.RemovePermission(typeof(HostProtectionPermission));
                }
                else if (perm.Resources != permission.Resources)
                {
                    this.SetPermission(perm);
                }
            }
        }

        public virtual void FromXml(SecurityElement et)
        {
            this.FromXml(et, false, false);
        }

        internal virtual void FromXml(SecurityDocument doc, int position, bool allowInternalOnly)
        {
            if (doc == null)
            {
                throw new ArgumentNullException("doc");
            }
            if (!doc.GetTagForElement(position).Equals("PermissionSet"))
            {
                throw new ArgumentException(string.Format(null, Environment.GetResourceString("Argument_InvalidXMLElement"), new object[] { "PermissionSet", base.GetType().FullName }));
            }
            this.Reset();
            this.m_allPermissionsDecoded = false;
            Exception exception = null;
            string attributeForElement = doc.GetAttributeForElement(position, "Unrestricted");
            if (attributeForElement != null)
            {
                this.m_Unrestricted = (attributeForElement.Equals("True") || attributeForElement.Equals("true")) || attributeForElement.Equals("TRUE");
            }
            else
            {
                this.m_Unrestricted = false;
            }
            ArrayList childrenPositionForElement = doc.GetChildrenPositionForElement(position);
            int count = childrenPositionForElement.Count;
            for (int i = 0; i < count; i++)
            {
                int num3 = (int) childrenPositionForElement[i];
                if (IsPermissionTag(doc.GetTagForElement(num3), allowInternalOnly))
                {
                    try
                    {
                        PermissionToken token;
                        object obj2;
                        string typeStr = doc.GetAttributeForElement(num3, "class");
                        if (typeStr != null)
                        {
                            token = PermissionToken.GetToken(typeStr);
                            if (token == null)
                            {
                                obj2 = this.CreatePerm(doc.GetElement(num3, true));
                                if (obj2 != null)
                                {
                                    token = PermissionToken.GetToken((IPermission) obj2);
                                }
                            }
                            else
                            {
                                obj2 = ((ISecurityElementFactory) new SecurityDocumentElement(doc, num3)).CreateSecurityElement();
                            }
                        }
                        else
                        {
                            IPermission perm = this.CreatePerm(doc.GetElement(num3, true));
                            if (perm == null)
                            {
                                token = null;
                                obj2 = null;
                            }
                            else
                            {
                                token = PermissionToken.GetToken(perm);
                                obj2 = perm;
                            }
                        }
                        if ((token != null) && (obj2 != null))
                        {
                            if (this.m_permSet == null)
                            {
                                this.m_permSet = new TokenBasedSet();
                            }
                            IPermission item = null;
                            if (this.m_permSet.GetItem(token.m_index) != null)
                            {
                                if (this.m_permSet.GetItem(token.m_index) is IPermission)
                                {
                                    item = (IPermission) this.m_permSet.GetItem(token.m_index);
                                }
                                else
                                {
                                    item = this.CreatePerm(this.m_permSet.GetItem(token.m_index));
                                }
                            }
                            if (item != null)
                            {
                                if (obj2 is IPermission)
                                {
                                    obj2 = item.Union((IPermission) obj2);
                                }
                                else
                                {
                                    obj2 = item.Union(this.CreatePerm(obj2));
                                }
                            }
                            if (this.m_Unrestricted && (obj2 is IPermission))
                            {
                                obj2 = null;
                            }
                            this.m_permSet.SetItem(token.m_index, obj2);
                        }
                    }
                    catch (Exception exception2)
                    {
                        if (exception == null)
                        {
                            exception = exception2;
                        }
                    }
                }
            }
            if (exception != null)
            {
                throw exception;
            }
        }

        internal virtual void FromXml(SecurityElement et, bool allowInternalOnly, bool ignoreTypeLoadFailures)
        {
            if (et == null)
            {
                throw new ArgumentNullException("et");
            }
            if (!et.Tag.Equals("PermissionSet"))
            {
                throw new ArgumentException(string.Format(null, Environment.GetResourceString("Argument_InvalidXMLElement"), new object[] { "PermissionSet", base.GetType().FullName }));
            }
            this.Reset();
            this.m_ignoreTypeLoadFailures = ignoreTypeLoadFailures;
            this.m_allPermissionsDecoded = false;
            this.m_Unrestricted = XMLUtil.IsUnrestricted(et);
            if (et.InternalChildren != null)
            {
                int count = et.InternalChildren.Count;
                for (int i = 0; i < count; i++)
                {
                    SecurityElement element = (SecurityElement) et.Children[i];
                    if (IsPermissionTag(element.Tag, allowInternalOnly))
                    {
                        PermissionToken token;
                        object obj2;
                        string typeStr = element.Attribute("class");
                        if (typeStr != null)
                        {
                            token = PermissionToken.GetToken(typeStr);
                            if (token == null)
                            {
                                obj2 = this.CreatePerm(element);
                                if (obj2 != null)
                                {
                                    token = PermissionToken.GetToken((IPermission) obj2);
                                }
                            }
                            else
                            {
                                obj2 = element;
                            }
                        }
                        else
                        {
                            IPermission perm = this.CreatePerm(element);
                            if (perm == null)
                            {
                                token = null;
                                obj2 = null;
                            }
                            else
                            {
                                token = PermissionToken.GetToken(perm);
                                obj2 = perm;
                            }
                        }
                        if ((token != null) && (obj2 != null))
                        {
                            if (this.m_permSet == null)
                            {
                                this.m_permSet = new TokenBasedSet();
                            }
                            if (this.m_permSet.GetItem(token.m_index) != null)
                            {
                                IPermission item;
                                if (this.m_permSet.GetItem(token.m_index) is IPermission)
                                {
                                    item = (IPermission) this.m_permSet.GetItem(token.m_index);
                                }
                                else
                                {
                                    item = this.CreatePerm((SecurityElement) this.m_permSet.GetItem(token.m_index));
                                }
                                if (obj2 is IPermission)
                                {
                                    obj2 = ((IPermission) obj2).Union(item);
                                }
                                else
                                {
                                    obj2 = this.CreatePerm((SecurityElement) obj2).Union(item);
                                }
                            }
                            if (this.m_Unrestricted && (obj2 is IPermission))
                            {
                                obj2 = null;
                            }
                            this.m_permSet.SetItem(token.m_index, obj2);
                        }
                    }
                }
            }
        }

        private PermissionSet GetCasOnlySet()
        {
            if (!this.m_ContainsNonCas)
            {
                return this;
            }
            if (this.IsUnrestricted())
            {
                return this;
            }
            PermissionSet set = new PermissionSet(false);
            PermissionSetEnumeratorInternal internal2 = new PermissionSetEnumeratorInternal(this);
            while (internal2.MoveNext())
            {
                IPermission current = (IPermission) internal2.Current;
                if (current is CodeAccessPermission)
                {
                    set.AddPermission(current);
                }
            }
            set.m_CheckedForNonCas = true;
            set.m_ContainsCas = !set.IsEmpty();
            set.m_ContainsNonCas = false;
            return set;
        }

        public IEnumerator GetEnumerator()
        {
            return this.GetEnumeratorImpl();
        }

        protected virtual IEnumerator GetEnumeratorImpl()
        {
            return new PermissionSetEnumerator(this);
        }

        internal PermissionSetEnumeratorInternal GetEnumeratorInternal()
        {
            return new PermissionSetEnumeratorInternal(this);
        }

        internal IPermission GetFirstPerm()
        {
            IEnumerator enumerator = this.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return null;
            }
            return (enumerator.Current as IPermission);
        }

        [ComVisible(false)]
        public override int GetHashCode()
        {
            int num = this.m_Unrestricted ? -1 : 0;
            if (this.m_permSet != null)
            {
                this.DecodeAllPermissions();
                int maxUsedIndex = this.m_permSet.GetMaxUsedIndex();
                for (int i = this.m_permSet.GetStartingIndex(); i <= maxUsedIndex; i++)
                {
                    IPermission item = (IPermission) this.m_permSet.GetItem(i);
                    if (item != null)
                    {
                        num ^= item.GetHashCode();
                    }
                }
            }
            return num;
        }

        internal IPermission GetPermission(int index)
        {
            if (this.m_permSet == null)
            {
                return null;
            }
            object item = this.m_permSet.GetItem(index);
            if (item == null)
            {
                return null;
            }
            IPermission permission = item as IPermission;
            if (permission == null)
            {
                permission = this.CreatePermission(item, index);
                if (permission == null)
                {
                    return null;
                }
            }
            return permission;
        }

        internal IPermission GetPermission(IPermission perm)
        {
            if (perm == null)
            {
                return null;
            }
            return this.GetPermission(PermissionToken.GetToken(perm));
        }

        internal IPermission GetPermission(PermissionToken permToken)
        {
            if (permToken == null)
            {
                return null;
            }
            return this.GetPermission(permToken.m_index);
        }

        public IPermission GetPermission(Type permClass)
        {
            return this.GetPermissionImpl(permClass);
        }

        private static SecurityElement GetPermissionElement(SecurityElement el)
        {
            string str;
            if (((str = el.Tag) != null) && ((str == "IPermission") || (str == "Permission")))
            {
                return el;
            }
            IEnumerator enumerator = el.Children.GetEnumerator();
            if (enumerator.MoveNext())
            {
                return GetPermissionElement((SecurityElement) enumerator.Current);
            }
            return null;
        }

        protected virtual IPermission GetPermissionImpl(Type permClass)
        {
            if (permClass == null)
            {
                return null;
            }
            return this.GetPermission(PermissionToken.FindToken(permClass));
        }

        internal void InplaceIntersect(PermissionSet other)
        {
            Exception exception = null;
            this.m_CheckedForNonCas = false;
            if (this != other)
            {
                if ((other == null) || other.FastIsEmpty())
                {
                    this.Reset();
                }
                else if (!this.FastIsEmpty())
                {
                    int num = (this.m_permSet == null) ? -1 : this.m_permSet.GetMaxUsedIndex();
                    int num2 = (other.m_permSet == null) ? -1 : other.m_permSet.GetMaxUsedIndex();
                    if (this.IsUnrestricted() && (num < num2))
                    {
                        num = num2;
                        this.CheckSet();
                    }
                    if (other.IsUnrestricted())
                    {
                        other.CheckSet();
                    }
                    for (int i = 0; i <= num; i++)
                    {
                        object item = this.m_permSet.GetItem(i);
                        IPermission permission = item as IPermission;
                        ISecurityElementFactory child = item as ISecurityElementFactory;
                        object obj3 = other.m_permSet.GetItem(i);
                        IPermission target = obj3 as IPermission;
                        ISecurityElementFactory factory2 = obj3 as ISecurityElementFactory;
                        if ((item != null) || (obj3 != null))
                        {
                            if ((child != null) && (factory2 != null))
                            {
                                if (child.GetTag().Equals("PermissionIntersection") || child.GetTag().Equals("PermissionUnrestrictedIntersection"))
                                {
                                    SafeChildAdd((SecurityElement) child, factory2, true);
                                }
                                else
                                {
                                    bool copy = true;
                                    if (this.IsUnrestricted())
                                    {
                                        SecurityElement element = new SecurityElement("PermissionUnrestrictedUnion");
                                        element.AddAttribute("class", child.Attribute("class"));
                                        SafeChildAdd(element, child, false);
                                        child = element;
                                    }
                                    if (other.IsUnrestricted())
                                    {
                                        SecurityElement element2 = new SecurityElement("PermissionUnrestrictedUnion");
                                        element2.AddAttribute("class", factory2.Attribute("class"));
                                        SafeChildAdd(element2, factory2, true);
                                        factory2 = element2;
                                        copy = false;
                                    }
                                    SecurityElement parent = new SecurityElement("PermissionIntersection");
                                    parent.AddAttribute("class", child.Attribute("class"));
                                    SafeChildAdd(parent, child, false);
                                    SafeChildAdd(parent, factory2, copy);
                                    this.m_permSet.SetItem(i, parent);
                                }
                            }
                            else if (item == null)
                            {
                                if (this.IsUnrestricted())
                                {
                                    if (factory2 != null)
                                    {
                                        SecurityElement element4 = new SecurityElement("PermissionUnrestrictedIntersection");
                                        element4.AddAttribute("class", factory2.Attribute("class"));
                                        SafeChildAdd(element4, factory2, true);
                                        this.m_permSet.SetItem(i, element4);
                                    }
                                    else
                                    {
                                        PermissionToken token = (PermissionToken) PermissionToken.s_tokenSet.GetItem(i);
                                        if ((token.m_type & PermissionTokenType.IUnrestricted) != 0)
                                        {
                                            this.m_permSet.SetItem(i, target.Copy());
                                        }
                                    }
                                }
                            }
                            else if (obj3 == null)
                            {
                                if (other.IsUnrestricted())
                                {
                                    if (child != null)
                                    {
                                        SecurityElement element5 = new SecurityElement("PermissionUnrestrictedIntersection");
                                        element5.AddAttribute("class", child.Attribute("class"));
                                        SafeChildAdd(element5, child, false);
                                        this.m_permSet.SetItem(i, element5);
                                    }
                                    else
                                    {
                                        PermissionToken token2 = (PermissionToken) PermissionToken.s_tokenSet.GetItem(i);
                                        if ((token2.m_type & PermissionTokenType.IUnrestricted) == 0)
                                        {
                                            this.m_permSet.SetItem(i, null);
                                        }
                                    }
                                }
                                else
                                {
                                    this.m_permSet.SetItem(i, null);
                                }
                            }
                            else
                            {
                                if (child != null)
                                {
                                    permission = this.CreatePermission(child, i);
                                }
                                if (factory2 != null)
                                {
                                    target = other.CreatePermission(factory2, i);
                                }
                                try
                                {
                                    IPermission permission3;
                                    if (permission == null)
                                    {
                                        permission3 = target;
                                    }
                                    else if (target == null)
                                    {
                                        permission3 = permission;
                                    }
                                    else
                                    {
                                        permission3 = permission.Intersect(target);
                                    }
                                    this.m_permSet.SetItem(i, permission3);
                                }
                                catch (Exception exception2)
                                {
                                    if (exception == null)
                                    {
                                        exception = exception2;
                                    }
                                }
                            }
                        }
                    }
                    this.m_Unrestricted = this.m_Unrestricted && other.m_Unrestricted;
                    if (exception != null)
                    {
                        throw exception;
                    }
                }
            }
        }

        [SecuritySafeCritical]
        internal void InplaceUnion(PermissionSet other)
        {
            if ((this != other) && ((other != null) && !other.FastIsEmpty()))
            {
                this.m_CheckedForNonCas = false;
                this.m_Unrestricted = this.m_Unrestricted || other.m_Unrestricted;
                if (this.m_Unrestricted)
                {
                    this.m_permSet = null;
                }
                else
                {
                    int maxUsedIndex = -1;
                    if (other.m_permSet != null)
                    {
                        maxUsedIndex = other.m_permSet.GetMaxUsedIndex();
                        this.CheckSet();
                    }
                    Exception exception = null;
                    for (int i = 0; i <= maxUsedIndex; i++)
                    {
                        object item = this.m_permSet.GetItem(i);
                        IPermission permission = item as IPermission;
                        ISecurityElementFactory child = item as ISecurityElementFactory;
                        object obj3 = other.m_permSet.GetItem(i);
                        IPermission target = obj3 as IPermission;
                        ISecurityElementFactory factory2 = obj3 as ISecurityElementFactory;
                        if ((item != null) || (obj3 != null))
                        {
                            if ((child != null) && (factory2 != null))
                            {
                                if (child.GetTag().Equals("PermissionUnion") || child.GetTag().Equals("PermissionUnrestrictedUnion"))
                                {
                                    SafeChildAdd((SecurityElement) child, factory2, true);
                                }
                                else
                                {
                                    SecurityElement element;
                                    if (this.IsUnrestricted() || other.IsUnrestricted())
                                    {
                                        element = new SecurityElement("PermissionUnrestrictedUnion");
                                    }
                                    else
                                    {
                                        element = new SecurityElement("PermissionUnion");
                                    }
                                    element.AddAttribute("class", child.Attribute("class"));
                                    SafeChildAdd(element, child, false);
                                    SafeChildAdd(element, factory2, true);
                                    this.m_permSet.SetItem(i, element);
                                }
                            }
                            else if (item == null)
                            {
                                if (factory2 != null)
                                {
                                    this.m_permSet.SetItem(i, factory2.Copy());
                                }
                                else if (target != null)
                                {
                                    PermissionToken token = (PermissionToken) PermissionToken.s_tokenSet.GetItem(i);
                                    if (((token.m_type & PermissionTokenType.IUnrestricted) == 0) || !this.m_Unrestricted)
                                    {
                                        this.m_permSet.SetItem(i, target.Copy());
                                    }
                                }
                            }
                            else if (obj3 != null)
                            {
                                if (child != null)
                                {
                                    permission = this.CreatePermission(child, i);
                                }
                                if (factory2 != null)
                                {
                                    target = other.CreatePermission(factory2, i);
                                }
                                try
                                {
                                    IPermission permission3;
                                    if (permission == null)
                                    {
                                        permission3 = target;
                                    }
                                    else if (target == null)
                                    {
                                        permission3 = permission;
                                    }
                                    else
                                    {
                                        permission3 = permission.Union(target);
                                    }
                                    this.m_permSet.SetItem(i, permission3);
                                }
                                catch (Exception exception2)
                                {
                                    if (exception == null)
                                    {
                                        exception = exception2;
                                    }
                                }
                            }
                        }
                    }
                    if (exception != null)
                    {
                        throw exception;
                    }
                }
            }
        }

        internal SecurityElement InternalToXml()
        {
            SecurityElement element = new SecurityElement("PermissionSet");
            element.AddAttribute("class", base.GetType().FullName);
            element.AddAttribute("version", "1");
            if (this.m_Unrestricted)
            {
                element.AddAttribute("Unrestricted", "true");
            }
            if (this.m_permSet != null)
            {
                int maxUsedIndex = this.m_permSet.GetMaxUsedIndex();
                for (int i = this.m_permSet.GetStartingIndex(); i <= maxUsedIndex; i++)
                {
                    object item = this.m_permSet.GetItem(i);
                    if (item != null)
                    {
                        if (item is IPermission)
                        {
                            if (!this.m_Unrestricted)
                            {
                                element.AddChild(((IPermission) item).ToXml());
                            }
                        }
                        else
                        {
                            element.AddChild((SecurityElement) item);
                        }
                    }
                }
            }
            return element;
        }

        [SecuritySafeCritical]
        public PermissionSet Intersect(PermissionSet other)
        {
            if (((other == null) || other.FastIsEmpty()) || this.FastIsEmpty())
            {
                return null;
            }
            int num = (this.m_permSet == null) ? -1 : this.m_permSet.GetMaxUsedIndex();
            int num2 = (other.m_permSet == null) ? -1 : other.m_permSet.GetMaxUsedIndex();
            int num3 = (num < num2) ? num : num2;
            if (this.IsUnrestricted() && (num3 < num2))
            {
                num3 = num2;
                this.CheckSet();
            }
            if (other.IsUnrestricted() && (num3 < num))
            {
                num3 = num;
                other.CheckSet();
            }
            PermissionSet set = new PermissionSet(false);
            if (num3 > -1)
            {
                set.m_permSet = new TokenBasedSet();
            }
            for (int i = 0; i <= num3; i++)
            {
                object item = this.m_permSet.GetItem(i);
                IPermission permission = item as IPermission;
                ISecurityElementFactory child = item as ISecurityElementFactory;
                object obj3 = other.m_permSet.GetItem(i);
                IPermission target = obj3 as IPermission;
                ISecurityElementFactory factory2 = obj3 as ISecurityElementFactory;
                if ((item != null) || (obj3 != null))
                {
                    if ((child != null) && (factory2 != null))
                    {
                        bool copy = true;
                        bool flag2 = true;
                        SecurityElement parent = new SecurityElement("PermissionIntersection");
                        parent.AddAttribute("class", factory2.Attribute("class"));
                        if (this.IsUnrestricted())
                        {
                            SecurityElement element2 = new SecurityElement("PermissionUnrestrictedUnion");
                            element2.AddAttribute("class", child.Attribute("class"));
                            SafeChildAdd(element2, child, true);
                            flag2 = false;
                            child = element2;
                        }
                        if (other.IsUnrestricted())
                        {
                            SecurityElement element3 = new SecurityElement("PermissionUnrestrictedUnion");
                            element3.AddAttribute("class", factory2.Attribute("class"));
                            SafeChildAdd(element3, factory2, true);
                            copy = false;
                            factory2 = element3;
                        }
                        SafeChildAdd(parent, factory2, copy);
                        SafeChildAdd(parent, child, flag2);
                        set.m_permSet.SetItem(i, parent);
                    }
                    else if (item == null)
                    {
                        if (this.m_Unrestricted)
                        {
                            if (factory2 != null)
                            {
                                SecurityElement element4 = new SecurityElement("PermissionUnrestrictedIntersection");
                                element4.AddAttribute("class", factory2.Attribute("class"));
                                SafeChildAdd(element4, factory2, true);
                                set.m_permSet.SetItem(i, element4);
                            }
                            else if (target != null)
                            {
                                PermissionToken token = (PermissionToken) PermissionToken.s_tokenSet.GetItem(i);
                                if ((token.m_type & PermissionTokenType.IUnrestricted) != 0)
                                {
                                    set.m_permSet.SetItem(i, target.Copy());
                                }
                            }
                        }
                    }
                    else if (obj3 == null)
                    {
                        if (other.m_Unrestricted)
                        {
                            if (child != null)
                            {
                                SecurityElement element5 = new SecurityElement("PermissionUnrestrictedIntersection");
                                element5.AddAttribute("class", child.Attribute("class"));
                                SafeChildAdd(element5, child, true);
                                set.m_permSet.SetItem(i, element5);
                            }
                            else if (permission != null)
                            {
                                PermissionToken token2 = (PermissionToken) PermissionToken.s_tokenSet.GetItem(i);
                                if ((token2.m_type & PermissionTokenType.IUnrestricted) != 0)
                                {
                                    set.m_permSet.SetItem(i, permission.Copy());
                                }
                            }
                        }
                    }
                    else
                    {
                        IPermission permission3;
                        if (child != null)
                        {
                            permission = this.CreatePermission(child, i);
                        }
                        if (factory2 != null)
                        {
                            target = other.CreatePermission(factory2, i);
                        }
                        if (permission == null)
                        {
                            permission3 = target;
                        }
                        else if (target == null)
                        {
                            permission3 = permission;
                        }
                        else
                        {
                            permission3 = permission.Intersect(target);
                        }
                        set.m_permSet.SetItem(i, permission3);
                    }
                }
            }
            set.m_Unrestricted = this.m_Unrestricted && other.m_Unrestricted;
            if (set.FastIsEmpty())
            {
                return null;
            }
            return set;
        }

        [SecuritySafeCritical]
        public bool IsEmpty()
        {
            if (this.m_Unrestricted)
            {
                return false;
            }
            if ((this.m_permSet != null) && !this.m_permSet.FastIsEmpty())
            {
                PermissionSetEnumeratorInternal internal2 = new PermissionSetEnumeratorInternal(this);
                while (internal2.MoveNext())
                {
                    IPermission current = (IPermission) internal2.Current;
                    if (!current.IsSubsetOf(null))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal static bool IsIntersectingAssertedPermissions(PermissionSet assertSet1, PermissionSet assertSet2)
        {
            bool flag = false;
            if ((assertSet1 != null) && (assertSet2 != null))
            {
                PermissionSetEnumeratorInternal internal2 = new PermissionSetEnumeratorInternal(assertSet2);
                while (internal2.MoveNext())
                {
                    CodeAccessPermission current = (CodeAccessPermission) internal2.Current;
                    int currentIndex = internal2.GetCurrentIndex();
                    if (current != null)
                    {
                        CodeAccessPermission permission = (CodeAccessPermission) assertSet1.GetPermission(currentIndex);
                        try
                        {
                            if ((permission != null) && !permission.Equals(current))
                            {
                                flag = true;
                            }
                            continue;
                        }
                        catch (ArgumentException)
                        {
                            flag = true;
                            continue;
                        }
                    }
                }
            }
            return flag;
        }

        internal static bool IsPermissionTag(string tag, bool allowInternalOnly)
        {
            if ((!tag.Equals("Permission") && !tag.Equals("IPermission")) && (!allowInternalOnly || ((!tag.Equals("PermissionUnion") && !tag.Equals("PermissionIntersection")) && (!tag.Equals("PermissionUnrestrictedIntersection") && !tag.Equals("PermissionUnrestrictedUnion")))))
            {
                return false;
            }
            return true;
        }

        [SecuritySafeCritical]
        public bool IsSubsetOf(PermissionSet target)
        {
            IPermission permission;
            return this.IsSubsetOfHelper(target, IsSubsetOfType.Normal, out permission, false);
        }

        internal bool IsSubsetOfHelper(PermissionSet target, IsSubsetOfType type, out IPermission firstPermThatFailed, bool ignoreNonCas)
        {
            firstPermThatFailed = null;
            if ((target == null) || target.FastIsEmpty())
            {
                if (this.IsEmpty())
                {
                    return true;
                }
                firstPermThatFailed = this.GetFirstPerm();
                return false;
            }
            if (this.IsUnrestricted() && !target.IsUnrestricted())
            {
                return false;
            }
            if (this.m_permSet != null)
            {
                target.CheckSet();
                for (int i = this.m_permSet.GetStartingIndex(); i <= this.m_permSet.GetMaxUsedIndex(); i++)
                {
                    IPermission permission = this.GetPermission(i);
                    if ((permission != null) && !permission.IsSubsetOf(null))
                    {
                        IPermission permission2 = target.GetPermission(i);
                        if (!target.m_Unrestricted)
                        {
                            CodeAccessPermission permission3 = permission as CodeAccessPermission;
                            if (permission3 == null)
                            {
                                if (!ignoreNonCas && !permission.IsSubsetOf(permission2))
                                {
                                    firstPermThatFailed = permission;
                                    return false;
                                }
                                continue;
                            }
                            firstPermThatFailed = permission;
                            switch (type)
                            {
                                case IsSubsetOfType.Normal:
                                    if (permission.IsSubsetOf(permission2))
                                    {
                                        break;
                                    }
                                    return false;

                                case IsSubsetOfType.CheckDemand:
                                    if (permission3.CheckDemand((CodeAccessPermission) permission2))
                                    {
                                        break;
                                    }
                                    return false;

                                case IsSubsetOfType.CheckPermitOnly:
                                    if (permission3.CheckPermitOnly((CodeAccessPermission) permission2))
                                    {
                                        break;
                                    }
                                    return false;

                                case IsSubsetOfType.CheckAssertion:
                                    if (permission3.CheckAssert((CodeAccessPermission) permission2))
                                    {
                                        break;
                                    }
                                    return false;
                            }
                            firstPermThatFailed = null;
                        }
                    }
                }
            }
            return true;
        }

        public bool IsUnrestricted()
        {
            return this.m_Unrestricted;
        }

        internal void MergeDeniedSet(PermissionSet denied)
        {
            if (((denied != null) && !denied.FastIsEmpty()) && !this.FastIsEmpty())
            {
                this.m_CheckedForNonCas = false;
                if ((this.m_permSet != null) && (denied.m_permSet != null))
                {
                    int num = (denied.m_permSet.GetMaxUsedIndex() > this.m_permSet.GetMaxUsedIndex()) ? this.m_permSet.GetMaxUsedIndex() : denied.m_permSet.GetMaxUsedIndex();
                    for (int i = 0; i <= num; i++)
                    {
                        IPermission item = denied.m_permSet.GetItem(i) as IPermission;
                        if (item != null)
                        {
                            IPermission permission2 = this.m_permSet.GetItem(i) as IPermission;
                            if ((permission2 == null) && !this.m_Unrestricted)
                            {
                                denied.m_permSet.SetItem(i, null);
                            }
                            else if (((permission2 != null) && (item != null)) && permission2.IsSubsetOf(item))
                            {
                                this.m_permSet.SetItem(i, null);
                                denied.m_permSet.SetItem(i, null);
                            }
                        }
                    }
                }
            }
        }

        private static void MergePermission(IPermission perm, bool separateCasFromNonCas, ref PermissionSet casPset, ref PermissionSet nonCasPset)
        {
            if (perm != null)
            {
                if (!separateCasFromNonCas || (perm is CodeAccessPermission))
                {
                    if (casPset == null)
                    {
                        casPset = new PermissionSet(false);
                    }
                    IPermission permission = casPset.GetPermission(perm);
                    IPermission target = casPset.AddPermission(perm);
                    if ((permission != null) && !permission.IsSubsetOf(target))
                    {
                        throw new NotSupportedException(Environment.GetResourceString("NotSupported_DeclarativeUnion"));
                    }
                }
                else
                {
                    if (nonCasPset == null)
                    {
                        nonCasPset = new PermissionSet(false);
                    }
                    IPermission permission3 = nonCasPset.GetPermission(perm);
                    IPermission permission4 = nonCasPset.AddPermission(perm);
                    if ((permission3 != null) && !permission3.IsSubsetOf(permission4))
                    {
                        throw new NotSupportedException(Environment.GetResourceString("NotSupported_DeclarativeUnion"));
                    }
                }
            }
        }

        private void NormalizePermissionSet()
        {
            PermissionSet set = new PermissionSet(false) {
                m_Unrestricted = this.m_Unrestricted
            };
            if (this.m_permSet != null)
            {
                for (int i = this.m_permSet.GetStartingIndex(); i <= this.m_permSet.GetMaxUsedIndex(); i++)
                {
                    object item = this.m_permSet.GetItem(i);
                    IPermission perm = item as IPermission;
                    ISecurityElementFactory factory = item as ISecurityElementFactory;
                    if (factory != null)
                    {
                        perm = this.CreatePerm(factory);
                    }
                    if (perm != null)
                    {
                        set.SetPermission(perm);
                    }
                }
            }
            this.m_permSet = set.m_permSet;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            if (this.m_serializedPermissionSet != null)
            {
                this.FromXml(SecurityElement.FromString(this.m_serializedPermissionSet));
            }
            else if (this.m_normalPermSet != null)
            {
                this.m_permSet = this.m_normalPermSet.SpecialUnion(this.m_unrestrictedPermSet);
            }
            else if (this.m_unrestrictedPermSet != null)
            {
                this.m_permSet = this.m_unrestrictedPermSet.SpecialUnion(this.m_normalPermSet);
            }
            this.m_serializedPermissionSet = null;
            this.m_normalPermSet = null;
            this.m_unrestrictedPermSet = null;
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext ctx)
        {
            this.Reset();
        }

        [OnSerialized]
        private void OnSerialized(StreamingContext context)
        {
            if ((context.State & ~(StreamingContextStates.CrossAppDomain | StreamingContextStates.Clone)) != 0)
            {
                this.m_serializedPermissionSet = null;
                this.m_permSet = this.m_permSetSaved;
                this.m_permSetSaved = null;
                this.m_unrestrictedPermSet = null;
                this.m_normalPermSet = null;
            }
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext ctx)
        {
            if ((ctx.State & ~(StreamingContextStates.CrossAppDomain | StreamingContextStates.Clone)) != 0)
            {
                this.m_serializedPermissionSet = this.ToString();
                if (this.m_permSet != null)
                {
                    this.m_permSet.SpecialSplit(ref this.m_unrestrictedPermSet, ref this.m_normalPermSet, this.m_ignoreTypeLoadFailures);
                }
                this.m_permSetSaved = this.m_permSet;
                this.m_permSet = null;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public void PermitOnly()
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            SecurityRuntime.PermitOnly(this, ref lookForMyCaller);
        }

        internal static void RemoveAssertedPermissionSet(PermissionSet demandSet, PermissionSet assertSet, out PermissionSet alteredDemandSet)
        {
            alteredDemandSet = null;
            PermissionSetEnumeratorInternal internal2 = new PermissionSetEnumeratorInternal(demandSet);
            while (internal2.MoveNext())
            {
                CodeAccessPermission current = (CodeAccessPermission) internal2.Current;
                int currentIndex = internal2.GetCurrentIndex();
                if (current != null)
                {
                    CodeAccessPermission permission = (CodeAccessPermission) assertSet.GetPermission(currentIndex);
                    try
                    {
                        if (current.CheckAssert(permission))
                        {
                            if (alteredDemandSet == null)
                            {
                                alteredDemandSet = demandSet.Copy();
                            }
                            alteredDemandSet.RemovePermission(currentIndex);
                        }
                        continue;
                    }
                    catch (ArgumentException)
                    {
                        continue;
                    }
                }
            }
        }

        private IPermission RemovePermission(int index)
        {
            if (this.GetPermission(index) == null)
            {
                return null;
            }
            return (IPermission) this.m_permSet.RemoveItem(index);
        }

        public IPermission RemovePermission(Type permClass)
        {
            return this.RemovePermissionImpl(permClass);
        }

        protected virtual IPermission RemovePermissionImpl(Type permClass)
        {
            if (permClass == null)
            {
                return null;
            }
            PermissionToken token = PermissionToken.FindToken(permClass);
            if (token == null)
            {
                return null;
            }
            return this.RemovePermission(token.m_index);
        }

        internal static PermissionSet RemoveRefusedPermissionSet(PermissionSet assertSet, PermissionSet refusedSet, out bool bFailedToCompress)
        {
            PermissionSet set = null;
            bFailedToCompress = false;
            if (assertSet == null)
            {
                return null;
            }
            if (refusedSet != null)
            {
                if (refusedSet.IsUnrestricted())
                {
                    return null;
                }
                PermissionSetEnumeratorInternal internal2 = new PermissionSetEnumeratorInternal(refusedSet);
                while (internal2.MoveNext())
                {
                    CodeAccessPermission current = (CodeAccessPermission) internal2.Current;
                    int currentIndex = internal2.GetCurrentIndex();
                    if (current != null)
                    {
                        CodeAccessPermission permission = (CodeAccessPermission) assertSet.GetPermission(currentIndex);
                        try
                        {
                            if (current.Intersect(permission) == null)
                            {
                                continue;
                            }
                            if (current.Equals(permission))
                            {
                                if (set == null)
                                {
                                    set = assertSet.Copy();
                                }
                                set.RemovePermission(currentIndex);
                                continue;
                            }
                            bFailedToCompress = true;
                            return assertSet;
                        }
                        catch (ArgumentException)
                        {
                            if (set == null)
                            {
                                set = assertSet.Copy();
                            }
                            set.RemovePermission(currentIndex);
                            continue;
                        }
                    }
                }
            }
            if (set != null)
            {
                return set;
            }
            return assertSet;
        }

        internal void Reset()
        {
            this.m_Unrestricted = false;
            this.m_allPermissionsDecoded = true;
            this.m_permSet = null;
            this.m_ignoreTypeLoadFailures = false;
            this.m_CheckedForNonCas = false;
            this.m_ContainsCas = false;
            this.m_ContainsNonCas = false;
            this.m_permSetSaved = null;
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static void RevertAssert()
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            SecurityRuntime.RevertAssert(ref lookForMyCaller);
        }

        internal static void SafeChildAdd(SecurityElement parent, ISecurityElementFactory child, bool copy)
        {
            if (child != parent)
            {
                if (child.GetTag().Equals("IPermission") || child.GetTag().Equals("Permission"))
                {
                    parent.AddChild(child);
                }
                else if (parent.Tag.Equals(child.GetTag()))
                {
                    SecurityElement element = (SecurityElement) child;
                    for (int i = 0; i < element.InternalChildren.Count; i++)
                    {
                        ISecurityElementFactory factory = (ISecurityElementFactory) element.InternalChildren[i];
                        parent.AddChildNoDuplicates(factory);
                    }
                }
                else
                {
                    parent.AddChild(copy ? ((ISecurityElementFactory) child.Copy()) : child);
                }
            }
        }

        public IPermission SetPermission(IPermission perm)
        {
            return this.SetPermissionImpl(perm);
        }

        protected virtual IPermission SetPermissionImpl(IPermission perm)
        {
            if (perm == null)
            {
                return null;
            }
            PermissionToken token = PermissionToken.GetToken(perm);
            if ((token.m_type & PermissionTokenType.IUnrestricted) != 0)
            {
                this.m_Unrestricted = false;
            }
            this.CheckSet();
            this.GetPermission(token.m_index);
            this.m_CheckedForNonCas = false;
            this.m_permSet.SetItem(token.m_index, perm);
            return perm;
        }

        internal void SetUnrestricted(bool unrestricted)
        {
            this.m_Unrestricted = unrestricted;
            if (unrestricted)
            {
                this.m_permSet = null;
            }
        }

        [SecurityCritical]
        private static void SetupSecurity()
        {
            PolicyLevel domainPolicy = PolicyLevel.CreateAppDomainLevel();
            CodeGroup group = new UnionCodeGroup(new AllMembershipCondition(), domainPolicy.GetNamedPermissionSet("Execution"));
            StrongNamePublicKeyBlob blob = new StrongNamePublicKeyBlob("002400000480000094000000060200000024000052534131000400000100010007D1FA57C4AED9F0A32E84AA0FAEFD0DE9E8FD6AEC8F87FB03766C834C99921EB23BE79AD9D5DCC1DD9AD236132102900B723CF980957FC4E177108FC607774F29E8320E92EA05ECE4E821C0A5EFE8F1645C4C0C93C1AB99285D622CAA652C1DFAD63D745D6F2DE5F17E5EAF0FC4963D261C8A12436518206DC093344D5AD293");
            CodeGroup group2 = new UnionCodeGroup(new StrongNameMembershipCondition(blob, null, null), domainPolicy.GetNamedPermissionSet("FullTrust"));
            StrongNamePublicKeyBlob blob2 = new StrongNamePublicKeyBlob("00000000000000000400000000000000");
            CodeGroup group3 = new UnionCodeGroup(new StrongNameMembershipCondition(blob2, null, null), domainPolicy.GetNamedPermissionSet("FullTrust"));
            CodeGroup group4 = new UnionCodeGroup(new GacMembershipCondition(), domainPolicy.GetNamedPermissionSet("FullTrust"));
            group.AddChild(group2);
            group.AddChild(group3);
            group.AddChild(group4);
            domainPolicy.RootCodeGroup = group;
            try
            {
                AppDomain.CurrentDomain.SetAppDomainPolicy(domainPolicy);
            }
            catch (PolicyException)
            {
            }
        }

        void IDeserializationCallback.OnDeserialization(object sender)
        {
            this.NormalizePermissionSet();
            this.m_CheckedForNonCas = false;
        }

        [SecuritySafeCritical]
        public override string ToString()
        {
            return this.ToXml().ToString();
        }

        public virtual SecurityElement ToXml()
        {
            return this.ToXml("System.Security.PermissionSet");
        }

        internal SecurityElement ToXml(string permName)
        {
            SecurityElement element = new SecurityElement("PermissionSet");
            element.AddAttribute("class", permName);
            element.AddAttribute("version", "1");
            PermissionSetEnumeratorInternal internal2 = new PermissionSetEnumeratorInternal(this);
            if (this.m_Unrestricted)
            {
                element.AddAttribute("Unrestricted", "true");
            }
            while (internal2.MoveNext())
            {
                IPermission current = (IPermission) internal2.Current;
                if (!this.m_Unrestricted)
                {
                    element.AddChild(current.ToXml());
                }
            }
            return element;
        }

        [SecuritySafeCritical]
        public PermissionSet Union(PermissionSet other)
        {
            if ((other == null) || other.FastIsEmpty())
            {
                return this.Copy();
            }
            if (this.FastIsEmpty())
            {
                return other.Copy();
            }
            int num = -1;
            PermissionSet set = new PermissionSet {
                m_Unrestricted = this.m_Unrestricted || other.m_Unrestricted
            };
            if (!set.m_Unrestricted)
            {
                this.CheckSet();
                other.CheckSet();
                num = (this.m_permSet.GetMaxUsedIndex() > other.m_permSet.GetMaxUsedIndex()) ? this.m_permSet.GetMaxUsedIndex() : other.m_permSet.GetMaxUsedIndex();
                set.m_permSet = new TokenBasedSet();
                for (int i = 0; i <= num; i++)
                {
                    object item = this.m_permSet.GetItem(i);
                    IPermission permission = item as IPermission;
                    ISecurityElementFactory child = item as ISecurityElementFactory;
                    object obj3 = other.m_permSet.GetItem(i);
                    IPermission target = obj3 as IPermission;
                    ISecurityElementFactory factory2 = obj3 as ISecurityElementFactory;
                    if ((item != null) || (obj3 != null))
                    {
                        if ((child != null) && (factory2 != null))
                        {
                            SecurityElement element;
                            if (this.IsUnrestricted() || other.IsUnrestricted())
                            {
                                element = new SecurityElement("PermissionUnrestrictedUnion");
                            }
                            else
                            {
                                element = new SecurityElement("PermissionUnion");
                            }
                            element.AddAttribute("class", child.Attribute("class"));
                            SafeChildAdd(element, child, true);
                            SafeChildAdd(element, factory2, true);
                            set.m_permSet.SetItem(i, element);
                        }
                        else if (item == null)
                        {
                            if (factory2 != null)
                            {
                                set.m_permSet.SetItem(i, factory2.Copy());
                            }
                            else if (target != null)
                            {
                                PermissionToken token = (PermissionToken) PermissionToken.s_tokenSet.GetItem(i);
                                if (((token.m_type & PermissionTokenType.IUnrestricted) == 0) || !set.m_Unrestricted)
                                {
                                    set.m_permSet.SetItem(i, target.Copy());
                                }
                            }
                        }
                        else if (obj3 == null)
                        {
                            if (child != null)
                            {
                                set.m_permSet.SetItem(i, child.Copy());
                            }
                            else if (permission != null)
                            {
                                PermissionToken token2 = (PermissionToken) PermissionToken.s_tokenSet.GetItem(i);
                                if (((token2.m_type & PermissionTokenType.IUnrestricted) == 0) || !set.m_Unrestricted)
                                {
                                    set.m_permSet.SetItem(i, permission.Copy());
                                }
                            }
                        }
                        else
                        {
                            IPermission permission3;
                            if (child != null)
                            {
                                permission = this.CreatePermission(child, i);
                            }
                            if (factory2 != null)
                            {
                                target = other.CreatePermission(factory2, i);
                            }
                            if (permission == null)
                            {
                                permission3 = target;
                            }
                            else if (target == null)
                            {
                                permission3 = permission;
                            }
                            else
                            {
                                permission3 = permission.Union(target);
                            }
                            set.m_permSet.SetItem(i, permission3);
                        }
                    }
                }
            }
            return set;
        }

        public virtual int Count
        {
            get
            {
                int num = 0;
                if (this.m_permSet != null)
                {
                    num += this.m_permSet.GetCount();
                }
                return num;
            }
        }

        internal bool IgnoreTypeLoadFailures
        {
            set
            {
                this.m_ignoreTypeLoadFailures = value;
            }
        }

        public virtual bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public virtual object SyncRoot
        {
            get
            {
                return this;
            }
        }

        internal enum IsSubsetOfType
        {
            Normal,
            CheckDemand,
            CheckPermitOnly,
            CheckAssertion
        }
    }
}

