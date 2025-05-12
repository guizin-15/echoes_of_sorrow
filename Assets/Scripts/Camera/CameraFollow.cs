using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;            // O jogador
    public Vector3 offset = new Vector3(0f, 0f, -10f); // Distância da câmera
    public float smoothTime = 0.2f;     // Quanto menor, mais rápida a câmera segue

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}