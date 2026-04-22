using UnityEngine;

[RequireComponent(typeof(WeightSystem))]
public class ItemInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private LayerMask itemLayer;

    [Header("Hold Position")]
    [SerializeField] private Transform holdPosition;

    private Camera playerCamera;
    private WeightSystem weightSystem;
    private PickupItem heldItem;
    private PlayerControls inputActions;

    void Awake()
    {
        playerCamera = Camera.main;
        weightSystem = GetComponent<WeightSystem>();
        inputActions = new PlayerControls();

        if (holdPosition == null)
        {
            GameObject holdObj = new GameObject("HoldPosition");
            holdObj.transform.SetParent(transform);
            holdObj.transform.localPosition = new Vector3(0.5f, 0f, 1f);
            holdObj.transform.localRotation = Quaternion.identity;
            holdPosition = holdObj.transform;
        }
    }

    void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Interact.performed += OnInteract;
        inputActions.Player.Drop.performed += OnDrop;

        inputActions.Player.Sell.performed += OnSell;
    }

    void OnDisable()
    {
        inputActions.Player.Interact.performed -= OnInteract;
        inputActions.Player.Drop.performed -= OnDrop;
        inputActions.Player.Sell.performed -= OnSell;
        inputActions.Disable();
    }

    void OnInteract(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {

        if (heldItem != null)
        {
            return;
        }


        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        Debug.DrawRay(ray.origin, ray.direction * pickupRange, Color.red, 0.5f);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, itemLayer))
        {

            PickupItem item = hit.transform.GetComponent<PickupItem>();

            if (item != null && !item.IsHeld)
            {
                if (weightSystem.AddWeight(item.Weight))
                {
                    heldItem = item;
                    item.OnPickup(holdPosition);
                }
                else
                {
                }
            }
            else
            {
            }
        }
        else
        {
        }
    }

    void OnDrop(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (heldItem == null) return;

        Vector3 force = playerCamera.transform.forward * throwForce;

        weightSystem.RemoveWeight(heldItem.Weight);

        heldItem.OnDrop(holdPosition, force);

        heldItem = null;
    }

    void HighlightNearestItem()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * pickupRange, Color.yellow);
    }

    void OnSell(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        PlayerController controller = GetComponent<PlayerController>();

        if (!controller.isInSellZone)
        {
            Debug.LogWarning("Можно продавать только на Платформе!");
            return;
        }

        if (heldItem == null)
        {
            Debug.LogWarning("Руки пусты! Возьмите предмет (E).");
            return;
        }

        Debug.Log($"Продажа: {heldItem.ItemName} за {heldItem.Price}$");

        GameManager.Instance.AddMoney(heldItem.Price);

        weightSystem.RemoveWeight(heldItem.Weight);

        Destroy(heldItem.gameObject);

        heldItem = null;
    }

}