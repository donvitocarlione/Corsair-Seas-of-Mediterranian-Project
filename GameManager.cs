// GameManager.cs
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Player playerPrefab;
    [SerializeField] private InputManager inputManagerPrefab;
    [SerializeField] private ShipSelectionUI shipSelectionUIPrefab;

    private Player _player;
    private InputManager _inputManager;
    private ShipSelectionUI _shipSelectionUI;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeCore();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeCore()
    {
        // Controlled initialization order
        _player = Instantiate(playerPrefab);
        _inputManager = Instantiate(inputManagerPrefab, transform);
        _shipSelectionUI = Instantiate(shipSelectionUIPrefab,transform);

        _player.GetComponent<Player>().Initialize(null); // you should pass the data to this method to make it work properly
        _player.GetComponent<Player>().inputManager = _inputManager;
        _player.GetComponent<Player>().shipSelectionUI = _shipSelectionUI;

        Debug.Log("[GameManager] Core initialization complete");

    }

    public T GetDependency<T>() where T : class
    {
         if (typeof(T) == typeof(InputManager)) return _inputManager as T;
         if (typeof(T) == typeof(ShipSelectionUI)) return _shipSelectionUI as T;
        if (typeof(T) == typeof(Player)) return _player as T;
        return null;
    }
}