using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    [SerializeField] Camera cam;

    [SerializeField] Texture2D mouseTexture;

    private void Awake() {
        Vector2 cursorHotspot = new Vector2 (mouseTexture.width / 4 + 50, 5);
        Cursor.SetCursor(mouseTexture, cursorHotspot, CursorMode.Auto);
    }

    void LateUpdate()
    {
        Vector3 mousePositionInWorldSpace = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePositionInWorldSpace.z = 0;
        transform.position = mousePositionInWorldSpace;
    }
}
