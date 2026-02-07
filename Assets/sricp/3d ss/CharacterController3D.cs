using UnityEngine;
using Photon.Pun;

public class CharacterController3D : MonoBehaviourPun
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f;
    public Transform cameraTransform;

    [SerializeField] private float killRange = 2.5f; // ระยะการฆ่า
    [SerializeField] private LayerMask npcLayerMask; // Layer สำหรับ NPC
    [SerializeField] private LayerMask murderLayerMask; // Layer สำหรับ Murder

    private Vector3 moveDirection;
    private Rigidbody rb;
    private Animator animator;
    private bool isCounted = false; // ป้องกันการเรียกซ้ำซ้อน

    private static Network network; // แคช Network ไว้ในตัวแปร static

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient && network == null)
        {
            network = FindFirstObjectByType<Network>();
        }
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (!photonView.IsMine)
        {
            Destroy(rb);
        }
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (cameraTransform != null)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            moveDirection = forward * vertical + right * horizontal;
        }
        else
        {
            moveDirection = new Vector3(horizontal, 0f, vertical);
        }

        if (animator != null)
        {
            bool isWalking = moveDirection.magnitude > 0f;
            animator.SetBool("Walking", isWalking);
        }

        if (Input.GetMouseButtonDown(0)) // คลิกซ้าย
        {
            SetKillingAnimation(true);
            KillInRange(npcLayerMask, "NPC"); // ฆ่า NPC
            KillInRange(murderLayerMask, "Murder"); // ฆ่า Murder
        }
        else if (Input.GetMouseButtonUp(0)) // ปล่อยคลิกซ้าย
        {
            SetKillingAnimation(false);
        }
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        if (moveDirection.magnitude == 0f) return;

        Vector3 normalizedDirection = moveDirection.normalized;
        Quaternion targetRotation = Quaternion.LookRotation(normalizedDirection, Vector3.up);

        rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

        Vector3 targetPosition = rb.position + normalizedDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
    }

    private void SetKillingAnimation(bool isKilling)
    {
        if (animator != null)
        {
            animator.SetBool("Killing", isKilling);
            Debug.Log("Killing Animation: " + isKilling);
        }
    }

 private void KillInRange(LayerMask targetLayer, string targetType)
{
    Collider[] hitColliders = Physics.OverlapSphere(transform.position, killRange, targetLayer);
    Debug.Log($"Found {hitColliders.Length} {targetType}s in range.");
    foreach (var collider in hitColliders)
    {
        PhotonView targetPhotonView = collider.GetComponent<PhotonView>(); // ดึง PhotonView ของเป้าหมาย
        if (targetPhotonView != null)
        {
            targetPhotonView.RPC("RPC_Die", RpcTarget.AllBuffered); // เรียก RPC_Die ในทุก Client
            Debug.Log($"Killed {targetType}: {collider.name}");
        }
    }
}


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Debug.Log("Drawing Kill Range Gizmo...");
        Gizmos.DrawWireSphere(transform.position, killRange);
    }

    // ฟังก์ชัน Die() ต้องอยู่ในคลาส
   public void Die()
{
    if(photonView.IsMine && !isCounted) // ตรวจสอบว่าเป็นเจ้าของ Object และยังไม่ถูกนับ
    {
            isCounted = true; // ทำเครื่องหมายว่าถูกนับแล้ว
            Debug.Log("[CharacterController3D] Die() called.");

            photonView.RPC("RPC_Die", RpcTarget.AllBuffered);
    }
}

[PunRPC]
private void RPC_Die()
{
     if (isCounted) return; // ป้องกันการเรียกซ้ำ
     isCounted = true;      // ทำเครื่องหมายว่าถูกนับแล้ว
     Debug.Log($"{gameObject.name} has died.");
    
    // หยุดการเคลื่อนไหว
    if (rb != null) rb.isKinematic = true;
    if (animator != null)
    {
        animator.SetBool("Walking", false);
        animator.SetBool("Deading", true); // แสดงอนิเมชันการตาย
    }

    if (PhotonNetwork.IsMasterClient)
    {
            network?.UpdatePlayerCount(-1);
    }

        enabled = false; // ปิดการควบคุม
}
}
