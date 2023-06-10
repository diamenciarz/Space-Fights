using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MouseCursorFollower : MonoBehaviour
{
    private void Start()
    {
        transform.position = (Vector3)HelperMethods.VectorUtils.TranslatedMousePosition() + new Vector3(0, 0, 1);
    }
    void Update()
    {
        FollowMouse();
    }
    private void FollowMouse()
    {
        transform.position = (Vector3) HelperMethods.VectorUtils.TranslatedMousePosition() + new Vector3(0,0,1);
    }
}
