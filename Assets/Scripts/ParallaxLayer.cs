using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private float parallaxFactor = 0.5f;

    private Transform cam;
    private Vector3 lastCamPos;

    private float spriteWidth;

    void Start()
    {
        cam = Camera.main.transform;
        lastCamPos = cam.position;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        spriteWidth = sr.bounds.size.x;
    }

    void LateUpdate()
    {
        Vector3 deltaMovement = cam.position - lastCamPos;

        transform.position += new Vector3(
            deltaMovement.x * parallaxFactor,
            deltaMovement.y * parallaxFactor,
            0
        );

        lastCamPos = cam.position;

        // Reposiciona quando sair da tela
        float distance = cam.position.x - transform.position.x;

        if (distance > spriteWidth)
        {
            transform.position += Vector3.right * spriteWidth;
        }
        else if (distance < -spriteWidth)
        {
            transform.position += Vector3.left * spriteWidth;
        }
    }
}