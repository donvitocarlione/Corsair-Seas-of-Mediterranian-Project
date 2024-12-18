using UnityEngine;

public static class FactionConstants
{
    // Relationship thresholds
    public const float MAX_RELATIONSHIP = 100f;
    public const float MIN_RELATIONSHIP = -100f;
    public const float NEUTRAL_RELATIONSHIP = 0f;
    
    // Status thresholds
    public const float FRIENDLY_THRESHOLD = 25f;
    public const float HOSTILE_THRESHOLD = -25f;
    
    // Default values
    public const float DEFAULT_INFLUENCE = 100f;
    public const float DEFAULT_STARTING_RELATIONSHIP = 0f;
    
    // System constraints
    public const int MAX_FACTION_NAME_LENGTH = 32;
    public const int MIN_FACTION_NAME_LENGTH = 3;
    
    // Error messages
    public const string ERROR_INVALID_FACTION = "Invalid faction type provided";
    public const string ERROR_INVALID_RELATIONSHIP = "Relationship value must be between {0} and {1}";
    public const string ERROR_SAME_FACTION = "Cannot set relationship with same faction";
    public const string ERROR_INVALID_NAME_LENGTH = "Faction name must be between {0} and {1} characters";
}