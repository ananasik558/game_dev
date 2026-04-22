using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private string itemName = "Crate";
    [SerializeField] private float weight = 10f;
    [SerializeField] private Color itemColor = Color.white;

    private Rigidbody rb;
    private bool isHeld = false;
    private Transform heldParent;
    private Vector3 originalScale;

    public string ItemName => itemName;
    public float Weight => weight;
    public bool IsHeld => isHeld;

    [Header("Economy")]
    [SerializeField] private int price = 50;

    public int Price => price;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        originalScale = transform.localScale;

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            
        }
    }

    public void OnPickup(Transform holdPosition)
    {
        isHeld = true;
        heldParent = holdPosition;

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.SetParent(holdPosition);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        gameObject.layer = LayerMask.NameToLayer("Item");
    }

    public void OnDrop(Transform holdTransform, Vector3 throwDirection)
    {
        isHeld = false;

        transform.SetParent(null);

        Vector3 safePos = holdTransform.position + holdTransform.forward * 1.5f + Vector3.up * 1.0f;
        transform.position = safePos;

        rb.isKinematic = false;
        rb.WakeUp(); 

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        float throwSpeed = 6f; 
        rb.AddForce(throwDirection.normalized * throwSpeed, ForceMode.VelocityChange);

        gameObject.layer = LayerMask.NameToLayer("Item");

    }

    public void SetWeight(float newWeight)
    {
        weight = newWeight;
        rb.mass = newWeight;

        float scaleMultiplier = Mathf.Lerp(0.5f, 2f, Mathf.Clamp01(weight / 50f));
        transform.localScale = originalScale * scaleMultiplier;
    }

    void OnValidate()
    {
        if (rb != null)
            rb.mass = weight;
    }
}