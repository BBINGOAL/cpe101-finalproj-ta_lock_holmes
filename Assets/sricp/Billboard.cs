using UnityEngine;

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main != null)
        {
            // ทำให้ Canvas หันหน้าเข้าหากล้องโดยตรง โดยไม่หมุนตามแกนใด ๆ ของ Player
            transform.LookAt(transform.position + Camera.main.transform.forward);
        }
    }
}
