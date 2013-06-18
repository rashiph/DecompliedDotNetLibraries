namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;

    public interface INeedEngine
    {
        VsaEngine GetEngine();
        void SetEngine(VsaEngine engine);
    }
}

