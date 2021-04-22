using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorState : MonoBehaviour
{
    public Texture2D idle, hover, click;
    
    public void SetCursorToIdle()
    {
        if (idle != null)
        {
            Cursor.SetCursor(idle, Vector2.zero, CursorMode.ForceSoftware);
        }
    }

    public void SetCursorToHover()
    {
        if (hover != null)
        {
            Cursor.SetCursor(hover, Vector2.zero, CursorMode.ForceSoftware);
        }
    }

    public void SetCursorToClick()
    {
        if (click != null)
        {
            Cursor.SetCursor(click, Vector2.zero, CursorMode.ForceSoftware);
        }
    }

    public void OnDisable()
    {
        SetCursorToIdle();
    }
}
