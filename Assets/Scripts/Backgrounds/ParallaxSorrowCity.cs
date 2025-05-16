using UnityEngine;

public class ParallaxSorrowCity : MonoBehaviour
{
    private Vector3 startPos;
    private Vector3 camStartPos;

    public GameObject cam;
    [Range(0f, 1f)]
    public float parallaxEffect;

    void Start()
    {
        if (cam == null)
            cam = Camera.main.gameObject;

        startPos = transform.position;
        camStartPos = cam.transform.position;
    }

    void LateUpdate()
    {
        float deltaX = cam.transform.position.x - camStartPos.x;
        transform.position = new Vector3(startPos.x + deltaX * parallaxEffect, startPos.y, startPos.z);
    }
}
