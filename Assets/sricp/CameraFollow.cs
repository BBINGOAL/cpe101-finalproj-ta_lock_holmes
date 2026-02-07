using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;           // ตัวละครที่กล้องจะติดตาม
    public float distance = 5f;        // ระยะห่างระหว่างกล้องกับตัวละคร
    public float heightOffset = 2f;    // ความสูงของกล้องเหนือเป้าหมาย
    public float mouseSensitivity = 2f; // ความไวของเมาส์
    public float rotationSmoothTime = 0.1f; // เวลาสำหรับความลื่นไหลในการหมุนกล้อง
    public Vector2 pitchMinMax = new Vector2(-40, 85); // ขอบเขตการหมุนกล้องในแกน X (แนวตั้ง)

    private float yaw;                 // การหมุนในแกน Y (แนวนอน)
    private float pitch;               // การหมุนในแกน X (แนวตั้ง)
    private Vector3 currentRotation;  // การหมุนปัจจุบันของกล้อง
    private Vector3 smoothVelocity;   // ความเร็วของการลื่นไหล

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // ลองค้นหา Player ที่ Spawn โดยอัตโนมัติ
        if (target == null)
        {
            FindPlayerTarget();
        }
    }

    private void LateUpdate()
    {
        // หากไม่มีเป้าหมาย ให้ลองค้นหาใหม่
        if (target == null)
        {
            FindPlayerTarget();
            return;
        }

        // รับค่าการเคลื่อนที่ของเมาส์
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // ปรับค่าการหมุนกล้อง
        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y); // จำกัดการหมุนในแนวตั้ง

        // คำนวณการหมุนกล้อง
        Vector3 targetRotation = new Vector3(pitch, yaw, 0);
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref smoothVelocity, rotationSmoothTime);

        // หมุนกล้อง
        transform.eulerAngles = currentRotation;

        // ย้ายกล้องไปยังตำแหน่งที่เหมาะสม
        Vector3 targetPosition = target.position + Vector3.up * heightOffset - transform.forward * distance;
        transform.position = targetPosition;
    }

    private void FindPlayerTarget()
    {
        GameObject player = GameObject.FindWithTag("Player"); // ค้นหา Player ด้วย Tag
        if (player != null)
        {
            target = player.transform; // ตั้งค่า Target เป็น Player ที่เจอ
        }
    }
}
