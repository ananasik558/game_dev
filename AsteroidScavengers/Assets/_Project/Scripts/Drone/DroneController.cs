using UnityEngine;
using UnityEngine.AI; 

public class DroneController : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target;      
    [SerializeField] private float followDistance = 3f;
    [SerializeField] private float followHeight = 2f;
    [SerializeField] private float moveSpeed = 8f;

    [Header("Scan Settings")]
    [SerializeField] private float scanRange = 15f;
    [SerializeField] private LayerMask itemLayer; 

    [Header("Visuals")]
    [SerializeField] private Light scanLight;     
    [SerializeField] private Transform lightPivot;  
    [SerializeField] private ParticleSystem droneVFX;

    private NavMeshAgent agent;
    private PickupItem nearestItem;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.angularSpeed = 360f;
            agent.acceleration = 20f;
            agent.stoppingDistance = 0.5f;

            agent.updateRotation = false;
        }

        if (scanLight == null)
            scanLight = GetComponentInChildren<Light>();

        if (lightPivot == null)
            lightPivot = transform;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void Update()
    {
        if (target != null)
        {
            FollowPlayer();
        }

        ScanForItems();
    }

    void FollowPlayer()
    {
        if (agent == null) return;

        Vector3 offset = -target.forward * followDistance + Vector3.up * followHeight;
        Vector3 destination = target.position + offset;

        agent.SetDestination(destination);

        Vector3 lookDir = target.position - transform.position;
        lookDir.y = 0; 
        if (lookDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
        }
    }

    void ScanForItems()
    {
        Collider[] items = Physics.OverlapSphere(transform.position, scanRange, itemLayer);

        if (items.Length > 0)
        {
            FindNearestItem(items);

            if (scanLight != null)
            {
                scanLight.color = Color.green;
                scanLight.intensity = Mathf.Lerp(scanLight.intensity, 3f, Time.deltaTime * 5f);
            }

            if (droneVFX != null)
            {
                var main = droneVFX.main;
                main.startColor = Color.green;
                droneVFX.emissionRate = 40f; 
            }
        }
        else
        {
            nearestItem = null;

            if (scanLight != null)
            {
                scanLight.color = Color.cyan;
                scanLight.intensity = Mathf.Lerp(scanLight.intensity, 1f, Time.deltaTime * 5f);
            }

            if (droneVFX != null)
            {
                var main = droneVFX.main;
                main.startColor = Color.cyan;
                droneVFX.emissionRate = 10f; 
            }
        }
    }

    void FindNearestItem(Collider[] items)
    {
        float nearestDist = float.MaxValue;
        Transform nearestTrans = null;

        foreach (var col in items)
        {
            PickupItem item = col.GetComponent<PickupItem>();
            if (item != null && !item.IsHeld)
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestItem = item;
                    nearestTrans = col.transform;
                }
            }
        }
        if (nearestTrans != null && lightPivot != null)
        {
            Vector3 dir = (nearestTrans.position - lightPivot.position).normalized;
            Quaternion targetRot = Quaternion.LookRotation(dir);
            lightPivot.rotation = Quaternion.Slerp(lightPivot.rotation, targetRot, 10f * Time.deltaTime);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.2f);
        Gizmos.DrawWireSphere(transform.position, scanRange);
    }
}