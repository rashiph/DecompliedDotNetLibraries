namespace System.Net
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class SSPIAuthType : SSPIInterface
    {
        private static readonly SecurDll Library = (ComNetOS.IsWin9x ? SecurDll.SECUR32 : SecurDll.SECURITY);
        private static SecurityPackageInfoClass[] m_SecurityPackages;

        public int AcceptSecurityContext(ref SafeFreeCredentials credential, ref SafeDeleteContext context, SecurityBuffer inputBuffer, ContextFlags inFlags, Endianness endianness, SecurityBuffer outputBuffer, ref ContextFlags outFlags)
        {
            return SafeDeleteContext.AcceptSecurityContext(Library, ref credential, ref context, inFlags, endianness, inputBuffer, null, outputBuffer, ref outFlags);
        }

        public int AcceptSecurityContext(SafeFreeCredentials credential, ref SafeDeleteContext context, SecurityBuffer[] inputBuffers, ContextFlags inFlags, Endianness endianness, SecurityBuffer outputBuffer, ref ContextFlags outFlags)
        {
            return SafeDeleteContext.AcceptSecurityContext(Library, ref credential, ref context, inFlags, endianness, null, inputBuffers, outputBuffer, ref outFlags);
        }

        public int AcquireCredentialsHandle(string moduleName, CredentialUse usage, ref AuthIdentity authdata, out SafeFreeCredentials outCredential)
        {
            return SafeFreeCredentials.AcquireCredentialsHandle(Library, moduleName, usage, ref authdata, out outCredential);
        }

        public int AcquireCredentialsHandle(string moduleName, CredentialUse usage, ref SecureCredential authdata, out SafeFreeCredentials outCredential)
        {
            return SafeFreeCredentials.AcquireCredentialsHandle(Library, moduleName, usage, ref authdata, out outCredential);
        }

        public int AcquireDefaultCredential(string moduleName, CredentialUse usage, out SafeFreeCredentials outCredential)
        {
            return SafeFreeCredentials.AcquireDefaultCredential(Library, moduleName, usage, out outCredential);
        }

        public int CompleteAuthToken(ref SafeDeleteContext refContext, SecurityBuffer[] inputBuffers)
        {
            if (ComNetOS.IsWin9x)
            {
                throw new NotSupportedException();
            }
            return SafeDeleteContext.CompleteAuthToken(Library, ref refContext, inputBuffers);
        }

        public int DecryptMessage(SafeDeleteContext context, SecurityBufferDescriptor inputOutput, uint sequenceNumber)
        {
            if (ComNetOS.IsWin9x)
            {
                throw ExceptionHelper.MethodNotImplementedException;
            }
            return this.DecryptMessageHelper(context, inputOutput, sequenceNumber);
        }

        private unsafe int DecryptMessageHelper(SafeDeleteContext context, SecurityBufferDescriptor inputOutput, uint sequenceNumber)
        {
            int num = -2146893055;
            bool success = false;
            uint qualityOfProtection = 0;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                context.DangerousAddRef(ref success);
            }
            catch (Exception exception)
            {
                if (success)
                {
                    context.DangerousRelease();
                    success = false;
                }
                if (!(exception is ObjectDisposedException))
                {
                    throw;
                }
            }
            finally
            {
                if (success)
                {
                    num = UnsafeNclNativeMethods.NativeNTSSPI.DecryptMessage(ref context._handle, inputOutput, sequenceNumber, &qualityOfProtection);
                    context.DangerousRelease();
                }
            }
            if ((num == 0) && (qualityOfProtection == 0x80000001))
            {
                throw new InvalidOperationException(SR.GetString("net_auth_message_not_encrypted"));
            }
            return num;
        }

        public int EncryptMessage(SafeDeleteContext context, SecurityBufferDescriptor inputOutput, uint sequenceNumber)
        {
            if (ComNetOS.IsWin9x)
            {
                throw ExceptionHelper.MethodNotImplementedException;
            }
            return this.EncryptMessageHelper(context, inputOutput, sequenceNumber);
        }

        private int EncryptMessageHelper(SafeDeleteContext context, SecurityBufferDescriptor inputOutput, uint sequenceNumber)
        {
            int num = -2146893055;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                context.DangerousAddRef(ref success);
            }
            catch (Exception exception)
            {
                if (success)
                {
                    context.DangerousRelease();
                    success = false;
                }
                if (!(exception is ObjectDisposedException))
                {
                    throw;
                }
            }
            finally
            {
                if (success)
                {
                    num = UnsafeNclNativeMethods.NativeNTSSPI.EncryptMessage(ref context._handle, 0, inputOutput, sequenceNumber);
                    context.DangerousRelease();
                }
            }
            return num;
        }

        public int EnumerateSecurityPackages(out int pkgnum, out SafeFreeContextBuffer pkgArray)
        {
            return SafeFreeContextBuffer.EnumeratePackages(Library, out pkgnum, out pkgArray);
        }

        private static int GetSecurityContextToken(SafeDeleteContext phContext, out SafeCloseHandle safeHandle)
        {
            int num = -2146893055;
            bool success = false;
            safeHandle = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                phContext.DangerousAddRef(ref success);
            }
            catch (Exception exception)
            {
                if (success)
                {
                    phContext.DangerousRelease();
                    success = false;
                }
                if (!(exception is ObjectDisposedException))
                {
                    throw;
                }
            }
            finally
            {
                if (success)
                {
                    num = UnsafeNclNativeMethods.SafeNetHandles.QuerySecurityContextToken(ref phContext._handle, out safeHandle);
                    phContext.DangerousRelease();
                }
            }
            return num;
        }

        public int InitializeSecurityContext(ref SafeFreeCredentials credential, ref SafeDeleteContext context, string targetName, ContextFlags inFlags, Endianness endianness, SecurityBuffer inputBuffer, SecurityBuffer outputBuffer, ref ContextFlags outFlags)
        {
            return SafeDeleteContext.InitializeSecurityContext(Library, ref credential, ref context, targetName, inFlags, endianness, inputBuffer, null, outputBuffer, ref outFlags);
        }

        public int InitializeSecurityContext(SafeFreeCredentials credential, ref SafeDeleteContext context, string targetName, ContextFlags inFlags, Endianness endianness, SecurityBuffer[] inputBuffers, SecurityBuffer outputBuffer, ref ContextFlags outFlags)
        {
            return SafeDeleteContext.InitializeSecurityContext(Library, ref credential, ref context, targetName, inFlags, endianness, null, inputBuffers, outputBuffer, ref outFlags);
        }

        public int MakeSignature(SafeDeleteContext context, SecurityBufferDescriptor inputOutput, uint sequenceNumber)
        {
            if (ComNetOS.IsWin9x)
            {
                throw ExceptionHelper.MethodNotImplementedException;
            }
            return this.MakeSignatureHelper(context, inputOutput, sequenceNumber);
        }

        private int MakeSignatureHelper(SafeDeleteContext context, SecurityBufferDescriptor inputOutput, uint sequenceNumber)
        {
            int num = -2146893055;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                context.DangerousAddRef(ref success);
            }
            catch (Exception exception)
            {
                if (success)
                {
                    context.DangerousRelease();
                    success = false;
                }
                if (!(exception is ObjectDisposedException))
                {
                    throw;
                }
            }
            finally
            {
                if (success)
                {
                    num = UnsafeNclNativeMethods.NativeNTSSPI.EncryptMessage(ref context._handle, 0x80000001, inputOutput, sequenceNumber);
                    context.DangerousRelease();
                }
            }
            return num;
        }

        public unsafe int QueryContextAttributes(SafeDeleteContext context, ContextAttribute attribute, byte[] buffer, Type handleType, out SafeHandle refHandle)
        {
            refHandle = null;
            if (handleType != null)
            {
                if (handleType != typeof(SafeFreeContextBuffer))
                {
                    if (handleType != typeof(SafeFreeCertContext))
                    {
                        throw new ArgumentException(SR.GetString("SSPIInvalidHandleType", new object[] { handleType.FullName }), "handleType");
                    }
                    refHandle = new SafeFreeCertContext();
                }
                else
                {
                    refHandle = SafeFreeContextBuffer.CreateEmptyHandle(Library);
                }
            }
            fixed (byte* numRef = buffer)
            {
                return SafeFreeContextBuffer.QueryContextAttributes(Library, context, attribute, numRef, refHandle);
            }
        }

        public int QueryContextChannelBinding(SafeDeleteContext context, ContextAttribute attribute, out SafeFreeContextBufferChannelBinding binding)
        {
            binding = null;
            throw new NotSupportedException();
        }

        public int QuerySecurityContextToken(SafeDeleteContext phContext, out SafeCloseHandle phToken)
        {
            if (ComNetOS.IsWin9x)
            {
                throw new NotSupportedException();
            }
            return GetSecurityContextToken(phContext, out phToken);
        }

        public int VerifySignature(SafeDeleteContext context, SecurityBufferDescriptor inputOutput, uint sequenceNumber)
        {
            if (ComNetOS.IsWin9x)
            {
                throw ExceptionHelper.MethodNotImplementedException;
            }
            return this.VerifySignatureHelper(context, inputOutput, sequenceNumber);
        }

        private unsafe int VerifySignatureHelper(SafeDeleteContext context, SecurityBufferDescriptor inputOutput, uint sequenceNumber)
        {
            int num = -2146893055;
            bool success = false;
            uint qualityOfProtection = 0;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                context.DangerousAddRef(ref success);
            }
            catch (Exception exception)
            {
                if (success)
                {
                    context.DangerousRelease();
                    success = false;
                }
                if (!(exception is ObjectDisposedException))
                {
                    throw;
                }
            }
            finally
            {
                if (success)
                {
                    num = UnsafeNclNativeMethods.NativeNTSSPI.DecryptMessage(ref context._handle, inputOutput, sequenceNumber, &qualityOfProtection);
                    context.DangerousRelease();
                }
            }
            return num;
        }

        public SecurityPackageInfoClass[] SecurityPackages
        {
            get
            {
                return m_SecurityPackages;
            }
            set
            {
                m_SecurityPackages = value;
            }
        }
    }
}

