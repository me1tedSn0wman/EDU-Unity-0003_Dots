using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;

[BurstCompile]
public struct FindNearestJobParallelSecond : IJobParallelFor
{
    [ReadOnly] public NativeArray<float3> TargetPositions;
    [ReadOnly] public NativeArray<float3> SeekerPositions;

    public NativeArray<float3> NearestTargetPositions;

    public void Execute(int index) { 
        float3 seekerPos = SeekerPositions[index];

        int startIdx = TargetPositions.BinarySearch(seekerPos, new AxisXComparer { });

        if (startIdx < 0) startIdx = ~startIdx;
        if (startIdx >= TargetPositions.Length) startIdx = TargetPositions.Length - 1;

        float3 nearestTargetPos = TargetPositions[startIdx];
        float nearestDistSq = math.distancesq(seekerPos, nearestTargetPos);

        Search(seekerPos, startIdx +1, TargetPositions.Length,+1, ref nearestTargetPos, ref nearestDistSq);

        Search(seekerPos, startIdx - 1, -1, -1, ref nearestTargetPos, ref nearestDistSq);


    }

    void Search(float3 seekerPos, int startIdx, int endIdx, int step, ref float3 nearestTargetPos, ref float nearestDistSq) {
        for (int i = startIdx; i != endIdx; i += step) {
            float3 targetPos = TargetPositions[i];

            float xdiff = seekerPos.x - targetPos.x;

            if ((xdiff * xdiff) > nearestDistSq) break;

            float distSq = math.distancesq(targetPos, seekerPos);

            if (distSq < nearestDistSq) {
                nearestDistSq = distSq;
                nearestTargetPos = targetPos;
            }
        }
    }
}

public struct AxisXComparer : IComparer<float3> {
    public int Compare(float3 a, float3 b) {
        return a.x.CompareTo(b.x);
    }
}
