using UnityEngine;

public class Ship : MonoBehaviour
{
    [SerializeField] protected string shipName;
    public string Name => shipName;

    protected ShipMovement movement;
    protected FiringSystem firingSystem;
    protected Pirate shipOwner;

    public ShipMovement Movement => movement;
    public FiringSystem FiringSystem => firingSystem;
    public Pirate ShipOwner => shipOwner;

    protected virtual void Awake()
    {
        movement = GetComponent<ShipMovement>();
        firingSystem = GetComponent<FiringSystem>();
        shipOwner = GetComponent<Pirate>();
    }

    protected virtual void Start()
    {
        if (string.IsNullOrEmpty(shipName))
        {
            shipName = gameObject.name;
        }
    }

    // Add other ship-specific functionality here
}
