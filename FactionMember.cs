using UnityEngine;
using CSM.Base;

namespace CorsairGame
{
    [RequireComponent(typeof(Ship))]
    public class FactionMember : MonoBehaviour
    {
        [SerializeField] private FactionType faction;
        public FactionType Faction => faction;

        private Ship ship;

        private void Awake()
        {
            ship = GetComponent<Ship>();
        }

        private void Start()
        {
            if (FactionManager.Instance != null)
            {
                FactionManager.Instance.OnShipRegistered(ship, faction);
            }
        }

        public void SetFaction(FactionType newFaction)
        {
            if (newFaction == faction) return;

            var oldFaction = faction;
            faction = newFaction;

            if (FactionManager.Instance != null)
            {
                if (oldFaction != FactionType.None)
                {
                    FactionManager.Instance.OnShipDestroyed(ship, oldFaction);
                }

                if (newFaction != FactionType.None)
                {
                    FactionManager.Instance.OnShipRegistered(ship, newFaction);
                }
            }
        }

        public bool IsFriendly(FactionMember other)
        {
            if (other == null)
                return false;

            if (FactionManager.Instance == null) return false;

            return FactionManager.Instance.AreFactionsAllied(this.faction, other.faction);
        }

        public bool IsHostile(FactionMember other)
        {
            if (other == null)
                return false;

            if (FactionManager.Instance == null) return false;

            return FactionManager.Instance.AreFactionsAtWar(this.faction, other.faction);
        }

        private void OnDestroy()
        {
            if (FactionManager.Instance != null)
            {
                FactionManager.Instance.OnShipDestroyed(ship, faction);
            }
        }
    }
}