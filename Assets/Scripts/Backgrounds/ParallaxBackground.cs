using UnityEngine;

public class ParallaxLayerController : MonoBehaviour
{
    private float startPosX;
    public GameObject cam;
    [Range(0f, 1f)]
    public float parallaxEffect;

    void Start()
    {
        if (cam == null)
            cam = Camera.main.gameObject;

        startPosX = transform.position.x;
    }

    void LateUpdate()
    {
        float distX = cam.transform.position.x * parallaxEffect;
        transform.position = new Vector3(startPosX + distX, transform.position.y, transform.position.z);
    }
}