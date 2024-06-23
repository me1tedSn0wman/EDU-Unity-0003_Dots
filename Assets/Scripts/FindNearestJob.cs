using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

using Unity.Mathematics;


[BurstCompile]
public struct FindNearestJob : IJob
{
    [ReadOnly] public NativeArray<float3> TargetPositions;
    [ReadOnly] public NativeArray<float3> SeekerPositions;
    public NativeArray<float3> NearestTargetPositions;

    public void Execute() {
        for (int i = 0; i < SeekerPositions.Length; i++) {
            float3 seekerPos = SeekerPositions[i];
            float nearestDistSq = float.MaxValue;
            for (int j = 0; j < TargetPositions.Length; j++) {
                float3 targetPos = TargetPositions[j];
                float distSq = math.distancesq(seekerPos, targetPos);
                if (distSq < nearestDistSq) {
                    nearestDistSq = distSq;
                    NearestTargetPositions[i] = targetPos;
                }
            }
        }
    }
}