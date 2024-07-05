using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct CannonBallSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

        var cannonBallJob = new CannonBallJob
        {
            ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged),
            DeltaTime = SystemAPI.Time.DeltaTime
        };

        cannonBallJob.Schedule();
    }
}

[BurstCompile]
public partial struct CannonBallJob : IJobEntity
{
    public EntityCommandBuffer ECB;
    public float DeltaTime;

    void Execute(Entity entity, ref CannonBall cannonBall, ref LocalTransform transform) {
        var gravity = new float3(0f, -9.82f, 0f);

        transform.Position += cannonBall.Velocity * DeltaTime;

        if (transform.Position.y <= 0.0f) { 
            ECB.DestroyEntity(entity);
        }

        cannonBall.Velocity += gravity * DeltaTime;
    }
}
