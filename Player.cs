using UnityEngine;
using System.Collections.Generic;
using CSM.Base;

namespace CorsairGame
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private Pirate activePirate;
        [SerializeField] private List<Pirate> pirates = new List<Pirate>();
        
        public Pirate ActivePirate => activePirate;
        
        private void Start()
        {
            if (pirates.Count > 0 && activePirate == null)
            {
                SetActivePirate(pirates[0]);
            }
        }
        
        public void AddPirate(Pirate pirate)
        {
            if (!pirates.Contains(pirate))
            {
                pirates.Add(pirate);
                
                if (activePirate == null)
                {
                    SetActivePirate(pirate);
                }
            }
        }
        
        public void RemovePirate(Pirate pirate)
        {
            if (pirates.Contains(pirate))
            {
                pirates.Remove(pirate);
                
                if (activePirate == pirate)
                {
                    SetActivePirate(pirates.Count > 0 ? pirates[0] : null);
                }
            }
        }
        
        public void SetActivePirate(Pirate pirate)
        {
            if (pirates.Contains(pirate) || pirate == null)
            {
                activePirate = pirate;
            }
        }
        
        public List<Pirate> GetAllPirates()
        {
            return new List<Pirate>(pirates);
        }
    }
}