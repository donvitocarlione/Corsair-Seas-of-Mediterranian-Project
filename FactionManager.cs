// Previous code remains the same until the configuration declaration
    [SerializeField] private FactionConfiguration _configuration;
    public FactionConfiguration configuration => _configuration;
    
    [SerializeField] private List<FactionDefinitionAsset> factionDefinitions;
    
    public FactionEventSystem EventSystem { get; private set; }
    
    private Dictionary<FactionType, FactionDefinition> factions = new();

    // Rest of the code remains the same