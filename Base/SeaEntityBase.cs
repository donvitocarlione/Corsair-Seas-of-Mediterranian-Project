using UnityEngine;

namespace CorsairGame
{
    public class SeaEntityBase : MonoBehaviour
    {
        [SerializeField] protected FactionType factionType;
        
        public FactionType FactionType => factionType;
        
        protected virtual void Start()
        {
            // Base initialization
        }
        
        protected virtual void OnDestroy()
        {
            // Base cleanup
        }
    }
}