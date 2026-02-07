using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerController : MonoBehaviourPun {
    public string shootButtonName = "ShootButton"; // ชื่อของปุ่มใน Hierarchy
    public float shootRange = 5f;                  // ระยะการยิง
    public LayerMask botLayer;                     // เลเยอร์ของ Bot

    private Button shootButton;                    // ตัวแปร Button ที่เชื่อมต่อใน Runtime
    private BotMove botInRange = null;             // Bot ที่อยู่ในระยะยิง

    private void Start() {
        // ค้นหา UI Button ตามชื่อใน Hierarchy
        GameObject buttonObject = GameObject.Find(shootButtonName);
        if (buttonObject != null) {
            shootButton = buttonObject.GetComponent<Button>();
            // ซ่อนปุ่มตอนเริ่มต้น
            shootButton.gameObject.SetActive(false);
            // ผูก Event ให้กับปุ่ม
            shootButton.onClick.AddListener(ShootBot);
        } else {
            Debug.LogError("Shoot Button not found in the Scene!");
        }
    }

    private void Update() {
        // ตรวจสอบว่า Bot อยู่ในระยะการยิงหรือไม่
        CheckBotInRange();

        if (botInRange != null) {
            // แสดงปุ่มยิง UI
            if (shootButton != null) shootButton.gameObject.SetActive(true);
        } else {
            // ซ่อนปุ่มยิงเมื่อไม่มี Bot ในระยะ
            if (shootButton != null) shootButton.gameObject.SetActive(false);
        }
    }

    private void CheckBotInRange() {
        // ตรวจจับ Bot ในระยะยิง
        Collider[] colliders = Physics.OverlapSphere(transform.position, shootRange, botLayer);
        if (colliders.Length > 0) {
            botInRange = colliders[0].GetComponent<BotMove>();
        } else {
            botInRange = null;
        }
    }

    private void ShootBot() {
        // หยุด Bot และทำให้ Bot ตาย
        if (botInRange != null) {
            botInRange.StopMovementAndDie();
        }
    }
}
