using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

public class FindNearest : MonoBehaviour
{
    static readonly ProfilerMarker s_PreparePerfMarkerFirst = new ProfilerMarker("FindTargetFirst");
    static readonly ProfilerMarker s_PreparePerfMarkerSecond = new ProfilerMarker("FindTargetSecond");
    static readonly ProfilerMarker s_PreparePerfMarkerThird = new ProfilerMarker("FindTargetThird");
    static readonly ProfilerMarker s_PreparePerfMarkerForth = new ProfilerMarker("FindTargetForth");

    NativeArray<float3> TargetPositions;
    NativeArray<float3> SeekerPositions;
    NativeArray<float3> NearestTargetPositions;

    public void Start()
    {
        Spawner spawner = Object.FindObjectOfType<Spawner>();

        TargetPositions = new NativeArray<float3>(spawner.NumTargets, Allocator.Persistent);
        SeekerPositions = new NativeArray<float3>(spawner.NumSeekers, Allocator.Persistent);
        NearestTargetPositions = new NativeArray<float3>(spawner.NumSeekers, Allocator.Persistent);
    }

    public void OnDestroy()
    {
        TargetPositions.Dispose();
        SeekerPositions.Dispose();
        NearestTargetPositions.Dispose();
    }

    public void Update()
    {
        s_PreparePerfMarkerFirst.Begin();
        UpdateFirstTry();
        s_PreparePerfMarkerFirst.End();

        s_PreparePerfMarkerSecond.Begin();
        UpdateSecondTry();
        s_PreparePerfMarkerSecond.End();

        s_PreparePerfMarkerThird.Begin();
        UpdateThirdTry();
        s_PreparePerfMarkerThird.End();

        s_PreparePerfMarkerForth.Begin();
        UpdateForthTry();
        s_PreparePerfMarkerForth.End();

    }

    public void UpdateFirstTry() {
        foreach (var seekerTransform in Spawner.SeekerTransforms)
        {
            Vector3 seekerPos = seekerTransform.localPosition;
            Vector3 nearestTargetPos = default;
            float nearestDistSq = float.MaxValue;
            foreach (var targetTransform in Spawner.TargetTransforms)
            {
                Vector3 offset = targetTransform.localPosition - seekerPos;
                float distSq = offset.sqrMagnitude;

                if (distSq < nearestDistSq)
                {
                    nearestDistSq = distSq;
                    nearestTargetPos = targetTransform.localPosition;
                }
            }

            Debug.DrawLine(seekerPos, nearestTargetPos);
        }
    }

    public void UpdateSecondTry() {
        for (int i = 0; i < TargetPositions.Length; i++) {
            TargetPositions[i] = Spawner.TargetTransforms[i].localPosition;
        }

        for (int i = 0; i < SeekerPositions.Length; i++) {
            SeekerPositions[i] = Spawner.SeekerTransforms[i].localPosition;
        }

        FindNearestJob findJob = new FindNearestJob
        {
            TargetPositions = TargetPositions,
            SeekerPositions = SeekerPositions,
            NearestTargetPositions = NearestTargetPositions,
        };

        JobHandle findHandle = findJob.Schedule();

        findHandle.Complete();

        for (int i = 0; i < SeekerPositions.Length; i++) {
            Debug.DrawLine(SeekerPositions[i], NearestTargetPositions[i]);
        }
    }

    public void UpdateThirdTry()
    {
        for (int i = 0; i < TargetPositions.Length; i++)
        {
            TargetPositions[i] = Spawner.TargetTransforms[i].localPosition;
        }

        for (int i = 0; i < SeekerPositions.Length; i++)
        {
            SeekerPositions[i] = Spawner.SeekerTransforms[i].localPosition;
        }

        FindNearestJobParallel findJob = new FindNearestJobParallel
        {
            TargetPositions = TargetPositions,
            SeekerPositions = SeekerPositions,
            NearestTargetPositions = NearestTargetPositions,
        };

        JobHandle findHandle = findJob.Schedule(SeekerPositions.Length, 100);

        findHandle.Complete();

        for (int i = 0; i < SeekerPositions.Length; i++)
        {
            Debug.DrawLine(SeekerPositions[i], NearestTargetPositions[i]);
        }
    }

    public void UpdateForthTry() {
        for (int i = 0; i < TargetPositions.Length; i++)
        {
            TargetPositions[i] = Spawner.TargetTransforms[i].localPosition;
        }

        for (int i = 0; i < SeekerPositions.Length; i++)
        {
            SeekerPositions[i] = Spawner.SeekerTransforms[i].localPosition;
        }
        SortJob<float3, AxisXComparer> sortJob = TargetPositions.SortJob(new AxisXComparer { });
        JobHandle sortHandle = sortJob.Schedule();

        FindNearestJobParallelSecond findJob = new FindNearestJobParallelSecond
        {
            TargetPositions = TargetPositions,
            SeekerPositions = SeekerPositions,
            NearestTargetPositions = NearestTargetPositions,
        };

        JobHandle findHandle = findJob.Schedule(SeekerPositions.Length, 100);

        findHandle.Complete();

        for (int i = 0; i < SeekerPositions.Length; i++)
        {
            Debug.DrawLine(SeekerPositions[i], NearestTargetPositions[i]);
        }
    }
}
