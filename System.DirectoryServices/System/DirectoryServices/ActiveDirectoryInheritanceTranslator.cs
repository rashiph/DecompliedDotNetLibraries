namespace System.DirectoryServices
{
    using System;
    using System.ComponentModel;
    using System.Security.AccessControl;

    internal sealed class ActiveDirectoryInheritanceTranslator
    {
        internal static InheritanceFlags[] ITToIF;
        internal static PropagationFlags[] ITToPF;

        static ActiveDirectoryInheritanceTranslator()
        {
            InheritanceFlags[] flagsArray = new InheritanceFlags[5];
            flagsArray[1] = InheritanceFlags.ContainerInherit;
            flagsArray[2] = InheritanceFlags.ContainerInherit;
            flagsArray[3] = InheritanceFlags.ContainerInherit;
            flagsArray[4] = InheritanceFlags.ContainerInherit;
            ITToIF = flagsArray;
            PropagationFlags[] flagsArray2 = new PropagationFlags[5];
            flagsArray2[2] = PropagationFlags.InheritOnly;
            flagsArray2[3] = PropagationFlags.NoPropagateInherit;
            flagsArray2[4] = PropagationFlags.InheritOnly | PropagationFlags.NoPropagateInherit;
            ITToPF = flagsArray2;
        }

        internal static ActiveDirectorySecurityInheritance GetEffectiveInheritanceFlags(InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags)
        {
            ActiveDirectorySecurityInheritance none = ActiveDirectorySecurityInheritance.None;
            if ((inheritanceFlags & InheritanceFlags.ContainerInherit) == InheritanceFlags.None)
            {
                return none;
            }
            switch (propagationFlags)
            {
                case PropagationFlags.None:
                    return ActiveDirectorySecurityInheritance.All;

                case PropagationFlags.NoPropagateInherit:
                    return ActiveDirectorySecurityInheritance.SelfAndChildren;

                case PropagationFlags.InheritOnly:
                    return ActiveDirectorySecurityInheritance.Descendents;

                case (PropagationFlags.InheritOnly | PropagationFlags.NoPropagateInherit):
                    return ActiveDirectorySecurityInheritance.Children;
            }
            throw new ArgumentException("propagationFlags");
        }

        internal static InheritanceFlags GetInheritanceFlags(ActiveDirectorySecurityInheritance inheritanceType)
        {
            if ((inheritanceType < ActiveDirectorySecurityInheritance.None) || (inheritanceType > ActiveDirectorySecurityInheritance.Children))
            {
                throw new InvalidEnumArgumentException("inheritanceType", (int) inheritanceType, typeof(ActiveDirectorySecurityInheritance));
            }
            return ITToIF[(int) inheritanceType];
        }

        internal static PropagationFlags GetPropagationFlags(ActiveDirectorySecurityInheritance inheritanceType)
        {
            if ((inheritanceType < ActiveDirectorySecurityInheritance.None) || (inheritanceType > ActiveDirectorySecurityInheritance.Children))
            {
                throw new InvalidEnumArgumentException("inheritanceType", (int) inheritanceType, typeof(ActiveDirectorySecurityInheritance));
            }
            return ITToPF[(int) inheritanceType];
        }
    }
}

