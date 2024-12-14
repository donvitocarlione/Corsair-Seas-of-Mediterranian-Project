using System.Collections.Generic;

namespace CSM.Base {
public interface IShipOwner {
  void AddShip(Ship ship);
  void RemoveShip(Ship ship);
  void SelectShip(Ship ship);
  List<Ship> GetOwnedShips();
}
}