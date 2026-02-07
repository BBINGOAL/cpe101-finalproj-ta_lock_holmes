using UnityEngine;
using Photon.Pun;

public class BotMove : MonoBehaviourPun {
    public float speed = 5f;  // ความเร็วในการเคลื่อนที่
    public float changeDirectionTime = 2f;  // เวลาสำหรับเปลี่ยนทิศทาง
    public float maxMoveDistance = 10f;  // ระยะทางสูงสุดที่ NPC จะเดินในแต่ละทิศทาง
    public float stopTimeMin = 1f;  // เวลาหยุดพักต่ำสุด
    public float stopTimeMax = 3f;  // เวลาหยุดพักสูงสุด

    public float mapWidth = 50f;  // ความกว้างของแผนที่ (แกน X)
    public float mapHeight = 50f;  // ความสูงของแผนที่ (แกน Z)

    private Vector3 velocity; // ใช้ Vector3 สำหรับการเคลื่อนที่
    private float timer; // ตัวนับเวลา
    private Rigidbody rb;
    private Animator animator; // ตัวแปร Animator สำหรับควบคุมอนิเมชัน
    private Vector3 originalScale; // เก็บขนาดเริ่มต้นของตัวละคร

    private bool isMoving = false; // เช็คว่า NPC กำลังเคลื่อนที่หรือไม่
    private bool isStopped = false; // เช็คว่า NPC หยุดอยู่หรือไม่
    private bool isDead = false;   // เช็คสถานะตาย
    private float distanceTraveled = 0f; // ระยะทางที่เดินไปแล้ว
    private float stopTimer = 0f; // ตัวจับเวลาเวลาหยุดพัก

    private void Awake() {
        rb = GetComponent<Rigidbody>(); // ใช้ Rigidbody สำหรับ 3D
        animator = GetComponent<Animator>(); // ใช้ Animator จาก GameObject
        velocity = Vector3.zero; // เริ่มต้น Vector3
        timer = changeDirectionTime; // ตั้งเวลาสำหรับการเปลี่ยนทิศทาง
        stopTimer = Random.Range(stopTimeMin, stopTimeMax); // สุ่มเวลาในการหยุดพัก
        originalScale = transform.localScale; // เก็บขนาดเริ่มต้นของตัวละคร

        // ตรวจสอบ PhotonView และปิดการควบคุมหากไม่ใช่ของผู้เล่นคนนี้
        if (!photonView.IsMine) {
            Destroy(rb); // ปิด Rigidbody เพื่อไม่ให้เคลื่อนที่โดยผู้เล่นอื่น
        }
    }

    private void Update() {
        // ตรวจสอบว่าเป็นตัวละครของผู้เล่นคนนี้หรือไม่
        if (!photonView.IsMine) return;

        if (isDead) return; // หยุดทุกอย่างถ้าตายแล้ว

        timer -= Time.deltaTime; // ลดเวลาลงทุกๆ frame

        // เปลี่ยนทิศทางการเดินเมื่อถึงเวลาที่กำหนด
        if (!isMoving && timer <= 0 && !isStopped) {
            ChangeDirection(); // เปลี่ยนทิศทาง
            timer = changeDirectionTime; // รีเซ็ตตัวนับเวลา
        }

        // ถ้า NPC หยุดอยู่, ให้ทำการจับเวลาในการหยุด
        if (isStopped) {
            stopTimer -= Time.deltaTime; // ลดเวลาหยุด
            if (stopTimer <= 0) {
                // เมื่อเวลาหยุดหมด, ให้สุ่มทิศทางใหม่และเริ่มเดินต่อ
                isStopped = false;
                stopTimer = Random.Range(stopTimeMin, stopTimeMax); // รีเซ็ตเวลาให้หยุดพัก
                ChangeDirection(); // เปลี่ยนทิศทางใหม่
            }
        }

        // ตั้งค่าตัวแปร Walking ใน Animator
        if (animator != null) {
            animator.SetBool("Walking", isMoving); // Walking จะเป็น true เมื่อ NPC กำลังเคลื่อนที่
        }
    }

    private void FixedUpdate() {
        // ตรวจสอบว่าเป็นตัวละครของผู้เล่นคนนี้หรือไม่
        if (!photonView.IsMine) return;

        if (isMoving) {
            // คำนวณระยะทางที่ NPC เดินไปแล้ว
            float distanceToMove = speed * Time.fixedDeltaTime;
            distanceTraveled += distanceToMove;

            // หาก NPC เดินครบระยะทางที่ต้องการแล้ว ให้หยุด
            if (distanceTraveled >= maxMoveDistance) {
                isMoving = false;
                velocity = Vector3.zero; // หยุดการเคลื่อนที่
                distanceTraveled = 0f; // รีเซ็ตระยะทางที่เดินไป
                isStopped = true; // ตั้งค่าให้ NPC หยุดพัก
            }

            // คำนวณตำแหน่งใหม่ที่ NPC จะเคลื่อนที่ไป
            Vector3 newPosition = rb.position + velocity.normalized * distanceToMove;

            // ตรวจสอบว่าตำแหน่งใหม่จะชนขอบแผนที่หรือไม่
            if (newPosition.x <= -mapWidth / 2f || newPosition.x >= mapWidth / 2f ||
                newPosition.z <= -mapHeight / 2f || newPosition.z >= mapHeight / 2f) {
                // หากชนขอบแผนที่, เปลี่ยนทิศทางใหม่
                ChangeDirection();
            } else {
                // หากไม่ชนขอบแผนที่, เคลื่อนที่ไปตำแหน่งใหม่
                rb.MovePosition(newPosition);
            }
        }
    }

    private void ChangeDirection() {
        // สุ่มทิศทางการเคลื่อนที่ในแกน X และ Z ภายในขอบเขตที่กำหนด
        float randomX = Random.Range(-1f, 1f); // สุ่มทิศทางในแกน X
        float randomZ = Random.Range(-1f, 1f); // สุ่มทิศทางในแกน Z

        // สร้าง vector ใหม่สำหรับทิศทางที่สุ่ม
        velocity = new Vector3(randomX, 0, randomZ);
        isMoving = true; // เริ่มเคลื่อนที่

        // พลิกตัวละครให้หันตามทิศทางการเคลื่อนที่
        if (velocity.x > 0) {
            transform.rotation = Quaternion.Euler(0, 180, 0); // หันหน้าไปทางขวา
        } else if (velocity.x < 0) {
            transform.rotation = Quaternion.Euler(0, 0, 0); // หันหน้าไปทางซ้าย
        }
    }

    // ฟังก์ชันหยุดการเคลื่อนที่และเปลี่ยนสถานะเป็น Deading
  public void StopMovementAndDie() {
    Debug.Log("StopMovementAndDie called.");
    isDead = true;
    isMoving = false;
    velocity = Vector3.zero;

    if (animator != null) {
        Debug.Log("Setting Deading to true in Animator."); // ตรวจสอบว่าทำงานถึงส่วนนี้
        animator.SetBool("Deading", true);
    } else {
        Debug.LogError("Animator is null!"); // ถ้า Animator ไม่ถูกเชื่อมโยง
    }
}
}