using UnityEngine;
using UnityEngine.AI;

public class DroneController : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float leadDistance = 5f;     
    [SerializeField] private float leadHeight = 3f;       
    [SerializeField] private float lateralOffset = 1.5f;  
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float predictionFactor = 1.5f; 

    [Header("Scan Settings")]
    [SerializeField] private float scanRange = 20f;
    [SerializeField] private LayerMask itemLayer;
    [SerializeField] private float scanConeAngle = 60f;   

    [Header("Visuals")]
    [SerializeField] private Light scanLight;
    [SerializeField] private Transform lightPivot;
    [SerializeField] private ParticleSystem droneVFX;

    private NavMeshAgent agent;
    private PickupItem nearestItem;
    private Camera playerCamera;
    private Vector3 lastPlayerPosition;
    private float playerSpeed;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.angularSpeed = 360f;
            agent.acceleration = 30f;
            agent.stoppingDistance = 0.3f;
            agent.updateRotation = false;
        }

        playerCamera = Camera.main;

        if (scanLight == null)
            scanLight = GetComponentInChildren<Light>();

        if (lightPivot == null)
            lightPivot = transform;

        lastPlayerPosition = target != null ? target.position : transform.position;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void Update()
    {
        if (target != null)
        {
            playerSpeed = Vector3.Distance(target.position, lastPlayerPosition) / Time.deltaTime;
            lastPlayerPosition = target.position;

            FollowPlayer();
        }

        ScanForItems();
    }

    void FollowPlayer()
    {
        if (agent == null || target == null) return;

        Vector3 playerForward = target.forward;

        float dynamicLead = leadDistance + (playerSpeed * predictionFactor);
        Vector3 predictedPos = target.position + playerForward * dynamicLead;

        Vector3 offset = Vector3.up * leadHeight + target.right * lateralOffset;
        Vector3 destination = predictedPos + offset;

        if (destination.y - target.position.y > leadHeight + 2f)
        {
            destination.y = target.position.y + leadHeight;
        }

        if (playerCamera != null)
        {
            Vector3 toDrone = (destination - playerCamera.transform.position).normalized;
            float dot = Vector3.Dot(playerCamera.transform.forward, toDrone);

            if (dot < 0.3f)
            {
                destination += target.right * 2f;
            }
        }

        agent.SetDestination(destination);

        Vector3 lookTarget = target.position + playerForward * 10f;
        Vector3 lookDir = (lookTarget - transform.position);
        lookDir.y = 0;

        if (lookDir.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 15f * Time.deltaTime);
        }
    }

    void ScanForItems()
    {
        Collider[] items = Physics.OverlapSphere(transform.position, scanRange, itemLayer);

        if (target != null)
        {
            Collider[] forwardItems = Physics.OverlapSphere(transform.position + target.forward * 5f, scanRange * 0.7f, itemLayer);

            if (forwardItems.Length > items.Length)
                items = forwardItems;
        }

        if (items.Length > 0)
        {
            FindNearestItem(items);

            if (scanLight != null)
            {
                scanLight.color = Color.green;
                scanLight.intensity = Mathf.Lerp(scanLight.intensity, 3f, Time.deltaTime * 5f);
            }

            if (droneVFX != null && droneVFX.isPlaying)
            {
                var main = droneVFX.main;
                main.startColor = Color.green;
                var emission = droneVFX.emission;
                emission.rateOverTime = 40f;
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

            if (droneVFX != null && droneVFX.isPlaying)
            {
                var main = droneVFX.main;
                main.startColor = Color.cyan;
                var emission = droneVFX.emission;
                emission.rateOverTime = 10f;
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
                float distancePenalty = 0f;
                if (target != null)
                {
                    Vector3 toItem = (col.transform.position - target.position).normalized;
                    float dot = Vector3.Dot(target.forward, toItem);
                    if (dot < 0) distancePenalty = 10f;
                }

                float dist = Vector3.Distance(transform.position, col.transform.position) + distancePenalty;

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
            lightPivot.rotation = Quaternion.Slerp(lightPivot.rotation, targetRot, 12f * Time.deltaTime);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.1f);
        Gizmos.DrawWireSphere(transform.position, scanRange);

        if (target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + target.forward * scanRange);
        }
    }
}