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
        transform.position = (Vector3) HelperMethods.VectorUtils.TranslatedMousePosition() + new Vector3(0,0,1);
    }
}
