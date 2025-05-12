using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    public Transform player;
    public float followSpeed = 5f;

    void Update()
    {
        if (player == null) return;

        Vector3 targetPos = new Vector3(player.position.x, player.position.y + 2.25f, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }
}