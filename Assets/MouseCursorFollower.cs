using UnityEngine;

public class MouseCursorFollower : MonoBehaviour
{
    void Update()
    {
        FollowMouse();
    }
    private void FollowMouse()
    {
        transform.position = HelperMethods.VectorUtils.TranslatedMousePosition();
    }
}
