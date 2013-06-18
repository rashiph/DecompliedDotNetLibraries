namespace Microsoft.Internal.Performance
{
    using System;

    internal enum CodeMarkerEvent
    {
        perfMSBuildEngineBuildProjectBegin = 0x2262,
        perfMSBuildEngineBuildProjectEnd = 0x2263,
        perfMSBuildGenerateResourceBegin = 0x226e,
        perfMSBuildGenerateResourceEnd = 0x226f,
        perfMSBuildHostCompileBegin = 0x2264,
        perfMSBuildHostCompileEnd = 0x2265,
        perfMSBuildProjectConstructBegin = 0x2268,
        perfMSBuildProjectConstructEnd = 0x2269,
        perfMSBuildProjectEvaluateBegin = 0x226c,
        perfMSBuildProjectEvaluateEnd = 0x226d,
        perfMSBuildProjectEvaluatePass0End = 0x2270,
        perfMSBuildProjectEvaluatePass1End = 0x2271,
        perfMSBuildProjectEvaluatePass2End = 0x2272,
        perfMSBuildProjectEvaluatePass3End = 0x2273,
        perfMSBuildProjectEvaluatePass4End = 0x2274,
        perfMSBuildProjectLoadFromFileBegin = 0x2266,
        perfMSBuildProjectLoadFromFileEnd = 0x2267,
        perfMSBuildProjectSaveToFileBegin = 0x226a,
        perfMSBuildProjectSaveToFileEnd = 0x226b,
        perfMSBuildRARComputeClosureBegin = 0x2279,
        perfMSBuildRARComputeClosureEnd = 0x227a,
        perfMSBuildRARLogResultsBegin = 0x227b,
        perfMSBuildRARLogResultsEnd = 0x227c,
        perfMSBuildRARRemoveFromExclusionListBegin = 0x2277,
        perfMSBuildRARRemoveFromExclusionListEnd = 0x2278,
        perfMSBuildResolveAssemblyReferenceBegin = 0x2275,
        perfMSBuildResolveAssemblyReferenceEnd = 0x2276
    }
}

