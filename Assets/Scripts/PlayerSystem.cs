using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct PlayerSystem : ISystem
{
    public void OnUpdate(ref SystemState state) {
        var movement = new float3(
            Input.GetAxis("Horizontal"),
            0,
            Input.GetAxis("Vertical")
            );
        movement *= SystemAPI.Time.DeltaTime * 30f;

        foreach (var playerTransform in
            SystemAPI.Query<RefRW<LocalTransform>>()
                .WithAll<Player>()) {
            playerTransform.ValueRW.Position += movement;

            var cameraTransform = Camera.main.transform;
            cameraTransform.position = playerTransform.ValueRO.Position;
            cameraTransform.position -= 10.0f * (Vector3)playerTransform.ValueRO.Forward();
            cameraTransform.position += new Vector3(0, 5f, 0);
            cameraTransform.LookAt(playerTransform.ValueRO.Position);
        }
  
    }
}
