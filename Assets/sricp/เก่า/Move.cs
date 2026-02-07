using UnityEngine;
using Photon.Pun;

public class Move : MonoBehaviourPun {
    public float speed = 15f;

    Vector3 velocity; // ใช้ Vector3 สำหรับ 3D

    Rigidbody rb;
    Animator animator; // ตัวแปร Animator สำหรับควบคุมอนิเมชัน
    private Vector3 originalScale; // เก็บขนาดเริ่มต้นของตัวละคร

    private void Awake() {
        rb = GetComponent<Rigidbody>(); // ใช้ Rigidbody สำหรับ 3D
        animator = GetComponent<Animator>(); // ใช้ Animator จาก GameObject
        velocity = Vector3.zero; // เริ่มต้น Vector3

        // เก็บขนาดเริ่มต้นของตัวละคร
        originalScale = transform.localScale;

        // ตรวจสอบ PhotonView และปิดการควบคุมหากไม่ใช่ของผู้เล่นคนนี้
        if (!photonView.IsMine) {
            Destroy(rb); // ปิด Rigidbody เพื่อไม่ให้เคลื่อนที่โดยผู้เล่นอื่น
        }
    }

    private void Update() {
        // ตรวจสอบว่าเป็นตัวละครของผู้เล่นคนนี้หรือไม่
        if (!photonView.IsMine) return;

        // รับข้อมูลการเคลื่อนที่ในแกน X และ Z
        velocity.x = Input.GetAxisRaw("Horizontal");
        velocity.z = Input.GetAxisRaw("Vertical");

        // ตั้งค่าตัวแปร Walking ใน Animator
        if (animator != null) {
            animator.SetBool("Walking", velocity != Vector3.zero); // Walking จะเป็น true เมื่อมีการเคลื่อนที่
        }

        // พลิกตัวละครตามการเคลื่อนที่ในแกน X
        if (velocity.x > 0) {
            transform.rotation = Quaternion.Euler(0, 180, 0); // หันหน้าไปทางขวา
        } else if (velocity.x < 0) {
            transform.rotation = Quaternion.Euler(0, 0, 0); // หันหน้าไปทางซ้าย
        }
    }

    private void FixedUpdate() {
        // ตรวจสอบว่าเป็นตัวละครของผู้เล่นคนนี้หรือไม่
        if (!photonView.IsMine) return;

        // เคลื่อนที่ Rigidbody ตามทิศทางที่กำหนดโดยไม่เปลี่ยนการหมุน (Rotation)
        rb.MovePosition(rb.position + velocity.normalized * speed * Time.fixedDeltaTime);
    }
}
