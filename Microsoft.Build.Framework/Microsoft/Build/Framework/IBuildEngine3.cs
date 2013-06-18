namespace Microsoft.Build.Framework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public interface IBuildEngine3 : IBuildEngine2, IBuildEngine
    {
        BuildEngineResult BuildProjectFilesInParallel(string[] projectFileNames, string[] targetNames, IDictionary[] globalProperties, IList<string>[] removeGlobalProperties, string[] toolsVersion, bool returnTargetOutputs);
        void Reacquire();
        void Yield();
    }
}

