using UnityEngine;

public class Target : MonoBehaviour
{
    public Vector3 direction;

    public void Update()
    {
        transform.localPosition += direction * Time.deltaTime;
    }
}
