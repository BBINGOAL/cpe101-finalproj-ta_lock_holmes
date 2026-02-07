using UnityEngine;

public class MAINMENU : MonoBehaviour
{
    void Start()
    {
        Cursor.visible = true; // ทำให้ Cursor แสดงขึ้นมา
        Cursor.lockState = CursorLockMode.None; // ปลดล็อก Cursor
    }
}

