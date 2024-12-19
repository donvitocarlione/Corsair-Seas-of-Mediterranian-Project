using System.Collections.Generic;
using CorsairGame;

namespace CorsairGame
{
    public interface IShipOwner
    {
        void AddShip(Ship ship);
        void RemoveShip(Ship ship);
        void SelectShip(Ship ship);
        List<Ship> GetOwnedShips();
    }
}