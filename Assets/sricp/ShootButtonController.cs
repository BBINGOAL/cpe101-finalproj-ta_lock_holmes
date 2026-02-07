using UnityEngine;
using UnityEngine.UI;

public class ShootButtonController : MonoBehaviour
{
    public Button shootButton;       // ปุ่ม UI สำหรับยิง
    public float shootRange = 10f;   // ระยะยิงที่กำหนด
    public LayerMask botLayer;       // เลเยอร์ของ Bot
    public Transform playerTransform; // Transform ของ Player

    private Transform targetBot;     // Bot เป้าหมายที่อยู่ในระยะ

    private void Start()
    {
        // ตรวจสอบว่าปุ่มถูกตั้งค่า
        if (shootButton != null)
        {
            shootButton.gameObject.SetActive(false); // ซ่อนปุ่มตอนเริ่มต้น
            shootButton.onClick.AddListener(OnShootButtonPressed); // ผูกฟังก์ชันกดปุ่ม
        }
    }

    private void Update()
    {
        // ตรวจหา Bot ที่อยู่ในระยะ
        CheckForBotInRange();

        // ถ้ามี Bot อยู่ในระยะ, แสดงปุ่ม
        if (targetBot != null)
        {
            shootButton.gameObject.SetActive(true);
        }
        else
        {
            shootButton.gameObject.SetActive(false);
        }
    }

    private void CheckForBotInRange()
    {
        // ใช้ Physics.OverlapSphere เพื่อตรวจจับ Bot ในระยะ
        Collider[] colliders = Physics.OverlapSphere(playerTransform.position, shootRange, botLayer);

        if (colliders.Length > 0)
        {
            // กำหนดเป้าหมายเป็น Bot ตัวแรกที่ตรวจเจอในระยะ
            targetBot = colliders[0].transform;
        }
        else
        {
            // ถ้าไม่มี Bot ในระยะ
            targetBot = null;
        }
    }

    public void OnShootButtonPressed()
    {
        // ฟังก์ชันเมื่อผู้เล่นกดปุ่มยิง
        if (targetBot != null)
        {
            Debug.Log($"Bot {targetBot.name} Shot!");
            Destroy(targetBot.gameObject); // ลบ Bot ออกจากเกม (หรือเพิ่มการจัดการยิงที่ซับซ้อนกว่า)
        }
    }
}
