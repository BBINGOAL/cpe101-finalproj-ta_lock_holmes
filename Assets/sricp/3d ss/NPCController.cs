using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class NPCController : MonoBehaviourPun
{
    private NavMeshAgent agent;
    private Animator animator;
    private bool isDead = false;
    private bool isCounted = false; // ป้องกันการลด botCount ซ้ำ

    public float stopDistance = 1f;
    public Vector2 stopTimeRange = new Vector2(1f, 3f);
    private float stopTimer = 0f;
    private bool isWaiting = false;

    private static Network network; // แคช Network ไว้ในตัวแปร static
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        SetNewDestination();

        // แคช Network ถ้ายังไม่ได้แคช
        if (PhotonNetwork.IsMasterClient && network == null)
        {
            network = FindFirstObjectByType<Network>();
        }
    }

    private void Update()
    {
        if (isDead) return;

        if (animator != null)
        {
            bool isWalking = agent.velocity.magnitude > 0.1f && !isWaiting;
            animator.SetBool("Walking", isWalking);
        }

        if (isWaiting)
        {
            stopTimer -= Time.deltaTime;
            if (stopTimer <= 0f)
            {
                isWaiting = false;
                agent.isStopped = false;
                SetNewDestination();
            }
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= stopDistance)
        {
            StartWaiting();
        }
    }

    private void SetNewDestination()
    {
        Vector3 randomPosition = GetRandomNavMeshPosition();
        agent.SetDestination(randomPosition);
    }

    private Vector3 GetRandomNavMeshPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 10f;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, 20f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return transform.position;
    }

    private void StartWaiting()
    {
        isWaiting = true;
        stopTimer = Random.Range(stopTimeRange.x, stopTimeRange.y);
        agent.isStopped = true;
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        if (animator != null)
        {
            animator.SetBool("Walking", false);
            animator.SetTrigger("Deading");
        }

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_Die", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void RPC_Die()
    {
        if (isDead || isCounted) return; // ป้องกันการเรียกซ้ำ
        isDead = true;
        isCounted = true;      // ทำเครื่องหมายว่าถูกนับแล้ว
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        if (animator != null)
        {
            animator.SetBool("Walking", false);
            animator.SetTrigger("Deading");
        }

        // อัปเดต botCount ใน Network
        if (PhotonNetwork.IsMasterClient)
        {
            network?.UpdateBotCount(-1);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}
