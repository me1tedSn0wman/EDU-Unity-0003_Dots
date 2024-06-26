using UnityEngine;

namespace TryJobsSystem
{
    public class Seeker : MonoBehaviour
    {
        public Vector3 direction;

        public void Update()
        {
            transform.localPosition += direction * Time.deltaTime;
        }
    }
}