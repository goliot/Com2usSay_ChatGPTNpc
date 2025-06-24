using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // 따라갈 대상 (Player)
    public float smoothTime = 0.2f;

    private Vector3 velocity = Vector3.zero;
    private Vector3 offset;

    void Start()
    {
        if (target == null) return;

        // 플레이어를 정중앙에 두기 위한 offset 계산
        offset = transform.position - target.position;
        offset.z = -10f; // 2D 카메라는 항상 z -10
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
