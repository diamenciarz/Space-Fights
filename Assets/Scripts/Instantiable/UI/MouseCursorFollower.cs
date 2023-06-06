using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MouseCursorFollower : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
    }
    void Update()
    {
        FollowMouse();
    }
    private void FollowMouse()
    {
        spriteRenderer.enabled = true;
        transform.position = HelperMethods.VectorUtils.TranslatedMousePosition();
    }
}
