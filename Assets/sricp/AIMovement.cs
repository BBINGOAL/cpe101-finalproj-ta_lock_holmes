using UnityEngine;
using UnityEngine.AI;

public class AIMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform target;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // ค้นหา GameObject ชื่อ "Target" ใน Scene และกำหนดให้เป็น Target
        target = GameObject.Find("Target").transform;
    }

    void Update()
    {
        if (target != null)
        {
            agent.SetDestination(target.position);
        }
    }
}
