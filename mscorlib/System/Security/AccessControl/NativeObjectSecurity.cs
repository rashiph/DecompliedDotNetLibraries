namespace System.Security.AccessControl
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Principal;

    public abstract class NativeObjectSecurity : CommonObjectSecurity
    {
        private object _exceptionContext;
        private ExceptionFromErrorCode _exceptionFromErrorCode;
        private readonly ResourceType _resourceType;
        private readonly uint ProtectedDiscretionaryAcl;
        private readonly uint ProtectedSystemAcl;
        private readonly uint UnprotectedDiscretionaryAcl;
        private readonly uint UnprotectedSystemAcl;

        [SecuritySafeCritical]
        protected NativeObjectSecurity(bool isContainer, ResourceType resourceType) : base(isContainer)
        {
            this.ProtectedDiscretionaryAcl = 0x80000000;
            this.ProtectedSystemAcl = 0x40000000;
            this.UnprotectedDiscretionaryAcl = 0x20000000;
            this.UnprotectedSystemAcl = 0x10000000;
            this._resourceType = resourceType;
        }

        [SecurityCritical]
        internal NativeObjectSecurity(ResourceType resourceType, CommonSecurityDescriptor securityDescriptor) : this(resourceType, securityDescriptor, null)
        {
        }

        [SecurityCritical]
        internal NativeObjectSecurity(ResourceType resourceType, CommonSecurityDescriptor securityDescriptor, ExceptionFromErrorCode exceptionFromErrorCode) : base(securityDescriptor)
        {
            this.ProtectedDiscretionaryAcl = 0x80000000;
            this.ProtectedSystemAcl = 0x40000000;
            this.UnprotectedDiscretionaryAcl = 0x20000000;
            this.UnprotectedSystemAcl = 0x10000000;
            this._resourceType = resourceType;
            this._exceptionFromErrorCode = exceptionFromErrorCode;
        }

        [SecuritySafeCritical]
        protected NativeObjectSecurity(bool isContainer, ResourceType resourceType, SafeHandle handle, AccessControlSections includeSections) : this(isContainer, resourceType, handle, includeSections, null, null)
        {
        }

        [SecuritySafeCritical]
        protected NativeObjectSecurity(bool isContainer, ResourceType resourceType, ExceptionFromErrorCode exceptionFromErrorCode, object exceptionContext) : this(isContainer, resourceType)
        {
            this._exceptionContext = exceptionContext;
            this._exceptionFromErrorCode = exceptionFromErrorCode;
        }

        [SecuritySafeCritical]
        protected NativeObjectSecurity(bool isContainer, ResourceType resourceType, string name, AccessControlSections includeSections) : this(isContainer, resourceType, name, includeSections, null, null)
        {
        }

        [SecuritySafeCritical]
        protected NativeObjectSecurity(bool isContainer, ResourceType resourceType, SafeHandle handle, AccessControlSections includeSections, ExceptionFromErrorCode exceptionFromErrorCode, object exceptionContext) : this(resourceType, CreateInternal(resourceType, isContainer, null, handle, includeSections, false, exceptionFromErrorCode, exceptionContext), exceptionFromErrorCode)
        {
        }

        [SecuritySafeCritical]
        protected NativeObjectSecurity(bool isContainer, ResourceType resourceType, string name, AccessControlSections includeSections, ExceptionFromErrorCode exceptionFromErrorCode, object exceptionContext) : this(resourceType, CreateInternal(resourceType, isContainer, name, null, includeSections, true, exceptionFromErrorCode, exceptionContext), exceptionFromErrorCode)
        {
        }

        [SecurityCritical]
        private static CommonSecurityDescriptor CreateInternal(ResourceType resourceType, bool isContainer, string name, SafeHandle handle, AccessControlSections includeSections, bool createByName, ExceptionFromErrorCode exceptionFromErrorCode, object exceptionContext)
        {
            RawSecurityDescriptor descriptor;
            if (createByName && (name == null))
            {
                throw new ArgumentNullException("name");
            }
            if (!createByName && (handle == null))
            {
                throw new ArgumentNullException("handle");
            }
            int errorCode = System.Security.AccessControl.Win32.GetSecurityInfo(resourceType, name, handle, includeSections, out descriptor);
            if (errorCode == 0)
            {
                return new CommonSecurityDescriptor(isContainer, false, descriptor, true);
            }
            Exception exception = null;
            if (exceptionFromErrorCode != null)
            {
                exception = exceptionFromErrorCode(errorCode, name, handle, exceptionContext);
            }
            if (exception == null)
            {
                switch (errorCode)
                {
                    case 5:
                        exception = new UnauthorizedAccessException();
                        goto Label_0132;

                    case 0x51b:
                        exception = new InvalidOperationException(Environment.GetResourceString("AccessControl_InvalidOwner"));
                        goto Label_0132;

                    case 0x51c:
                        exception = new InvalidOperationException(Environment.GetResourceString("AccessControl_InvalidGroup"));
                        goto Label_0132;

                    case 0x57:
                        exception = new InvalidOperationException(Environment.GetResourceString("AccessControl_UnexpectedError", new object[] { errorCode }));
                        goto Label_0132;

                    case 0x7b:
                        exception = new ArgumentException(Environment.GetResourceString("Argument_InvalidName"), "name");
                        goto Label_0132;

                    case 2:
                        exception = (name == null) ? new FileNotFoundException() : new FileNotFoundException(name);
                        goto Label_0132;

                    case 0x546:
                        exception = new NotSupportedException(Environment.GetResourceString("AccessControl_NoAssociatedSecurity"));
                        goto Label_0132;
                }
                exception = new InvalidOperationException(Environment.GetResourceString("AccessControl_UnexpectedError", new object[] { errorCode }));
            }
        Label_0132:
            throw exception;
        }

        [SecuritySafeCritical]
        protected sealed override void Persist(SafeHandle handle, AccessControlSections includeSections)
        {
            this.Persist(handle, includeSections, this._exceptionContext);
        }

        [SecuritySafeCritical]
        protected sealed override void Persist(string name, AccessControlSections includeSections)
        {
            this.Persist(name, includeSections, this._exceptionContext);
        }

        [SecuritySafeCritical]
        protected void Persist(SafeHandle handle, AccessControlSections includeSections, object exceptionContext)
        {
            if (handle == null)
            {
                throw new ArgumentNullException("handle");
            }
            this.Persist(null, handle, includeSections, exceptionContext);
        }

        [SecuritySafeCritical]
        protected void Persist(string name, AccessControlSections includeSections, object exceptionContext)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            this.Persist(name, null, includeSections, exceptionContext);
        }

        [SecurityCritical]
        private void Persist(string name, SafeHandle handle, AccessControlSections includeSections, object exceptionContext)
        {
            base.WriteLock();
            try
            {
                SecurityInfos securityInformation = 0;
                SecurityIdentifier owner = null;
                SecurityIdentifier group = null;
                SystemAcl sacl = null;
                DiscretionaryAcl dacl = null;
                if (((includeSections & AccessControlSections.Owner) != AccessControlSections.None) && (base._securityDescriptor.Owner != null))
                {
                    securityInformation |= SecurityInfos.Owner;
                    owner = base._securityDescriptor.Owner;
                }
                if (((includeSections & AccessControlSections.Group) != AccessControlSections.None) && (base._securityDescriptor.Group != null))
                {
                    securityInformation |= SecurityInfos.Group;
                    group = base._securityDescriptor.Group;
                }
                if ((includeSections & AccessControlSections.Audit) != AccessControlSections.None)
                {
                    securityInformation |= SecurityInfos.SystemAcl;
                    if ((base._securityDescriptor.IsSystemAclPresent && (base._securityDescriptor.SystemAcl != null)) && (base._securityDescriptor.SystemAcl.Count > 0))
                    {
                        sacl = base._securityDescriptor.SystemAcl;
                    }
                    else
                    {
                        sacl = null;
                    }
                    if ((base._securityDescriptor.ControlFlags & ControlFlags.SystemAclProtected) != ControlFlags.None)
                    {
                        securityInformation |= (SecurityInfos) this.ProtectedSystemAcl;
                    }
                    else
                    {
                        securityInformation |= (SecurityInfos) this.UnprotectedSystemAcl;
                    }
                }
                if (((includeSections & AccessControlSections.Access) != AccessControlSections.None) && base._securityDescriptor.IsDiscretionaryAclPresent)
                {
                    securityInformation |= SecurityInfos.DiscretionaryAcl;
                    if (base._securityDescriptor.DiscretionaryAcl.EveryOneFullAccessForNullDacl)
                    {
                        dacl = null;
                    }
                    else
                    {
                        dacl = base._securityDescriptor.DiscretionaryAcl;
                    }
                    if ((base._securityDescriptor.ControlFlags & ControlFlags.DiscretionaryAclProtected) != ControlFlags.None)
                    {
                        securityInformation |= (SecurityInfos) this.ProtectedDiscretionaryAcl;
                    }
                    else
                    {
                        securityInformation |= (SecurityInfos) this.UnprotectedDiscretionaryAcl;
                    }
                }
                if (securityInformation == 0)
                {
                    return;
                }
                int errorCode = System.Security.AccessControl.Win32.SetSecurityInfo(this._resourceType, name, handle, securityInformation, owner, group, sacl, dacl);
                if (errorCode == 0)
                {
                    goto Label_0249;
                }
                Exception exception = null;
                if (this._exceptionFromErrorCode != null)
                {
                    exception = this._exceptionFromErrorCode(errorCode, name, handle, exceptionContext);
                }
                if (exception == null)
                {
                    switch (errorCode)
                    {
                        case 5:
                            exception = new UnauthorizedAccessException();
                            goto Label_0246;

                        case 0x51b:
                            exception = new InvalidOperationException(Environment.GetResourceString("AccessControl_InvalidOwner"));
                            goto Label_0246;

                        case 0x51c:
                            exception = new InvalidOperationException(Environment.GetResourceString("AccessControl_InvalidGroup"));
                            goto Label_0246;

                        case 0x7b:
                            exception = new ArgumentException(Environment.GetResourceString("Argument_InvalidName"), "name");
                            goto Label_0246;

                        case 6:
                            exception = new NotSupportedException(Environment.GetResourceString("AccessControl_InvalidHandle"));
                            goto Label_0246;

                        case 2:
                            exception = new FileNotFoundException();
                            goto Label_0246;

                        case 0x546:
                            exception = new NotSupportedException(Environment.GetResourceString("AccessControl_NoAssociatedSecurity"));
                            goto Label_0246;
                    }
                    exception = new InvalidOperationException(Environment.GetResourceString("AccessControl_UnexpectedError", new object[] { errorCode }));
                }
            Label_0246:
                throw exception;
            Label_0249:
                base.OwnerModified = false;
                base.GroupModified = false;
                base.AccessRulesModified = false;
                base.AuditRulesModified = false;
            }
            finally
            {
                base.WriteUnlock();
            }
        }

        [SecuritySafeCritical]
        internal protected delegate Exception ExceptionFromErrorCode(int errorCode, string name, SafeHandle handle, object context);
    }
}

