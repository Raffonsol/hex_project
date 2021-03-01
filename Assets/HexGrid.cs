using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class IGrowth {
    public string name;
	public GroundNature growthNature;
    public int height;
	public int width;
	public int skipChance;
}

public class HexGrid : MonoBehaviour {

	public int height = 6;
	public int width = 6;

	public GameTile cellPrefab;

	[SerializeField]
		public List<IGrowth> growths;

    GameTile[] cells;
	// yes this is an arra of lists
	List<GameTile>[] growthsTileCollection;
	System.Random rnd = new System.Random();

	public List<GameTile> GetAvailableNeighborTiles (int tile) {
		List<GameTile> free = new List<GameTile>();

		GameTile north = GetNeighborTile("north", tile);
		if (north != null && north.groundType == GroundType.Walkable) free.Add(north);

		GameTile south = GetNeighborTile("south", tile);
		if (south != null && south.groundType == GroundType.Walkable) free.Add(south);

		GameTile southWest = GetNeighborTile("southWest", tile);
		if (southWest != null && southWest.groundType == GroundType.Walkable) free.Add(southWest);

		GameTile southEast = GetNeighborTile("southEast", tile);
		if (southEast != null && southEast.groundType == GroundType.Walkable) free.Add(southEast);

		GameTile northWest = GetNeighborTile("northWest", tile);
		if (northWest != null && northWest.groundType == GroundType.Walkable) free.Add(northWest);

		GameTile northEast = GetNeighborTile("northEast", tile);
		if (northEast != null && northEast.groundType == GroundType.Walkable) free.Add(northEast);

		return free;
	}

	public GameTile GetNeighborTile (string direction, int target) {
		switch (direction)
		{
			case ("north"):
			return GetTileByCoordenates(cells[target].north);
			case ("south"):
			return GetTileByCoordenates(cells[target].south);
			case ("southWest"):
			return GetTileByCoordenates(cells[target].southWest);
			case ("southEast"):
			return GetTileByCoordenates(cells[target].southEast);
			case ("northWest"):
			return GetTileByCoordenates(cells[target].northWest);
			case ("northEast"):
			return GetTileByCoordenates(cells[target].northEast);
			default:
			Debug.LogError("GetNeighborTile cannot read directon " 
			+ direction + ".\n Direction parameter should be one of the following: "
			+ "\n- \"north\" \n- \"south\" \n- \"southWest\" \n- \"southEast\" \n- \"northWest\" \n- \"northEast\"");
			return null;
		}
		// TODO: figure out how the line below can work eventually
		// return GetTileByCoordenates(cells[target].GetType().GetProperty(direction, typeof(Vector2)).GetValue(null));
		
	}

	public GameTile GetRandomNeighborTile (int target) {
		
		int dir = rnd.Next(0, 6);
		switch (dir)
		{
			case (0):
				return GetNeighborTile("north", target);
			case (1):
				return GetNeighborTile("northEast", target);
			case (2):
				return GetNeighborTile("northWest", target);
			case (3):
				return GetNeighborTile("south", target);
			case (4):
				return GetNeighborTile("southEast", target);
			case (5):
				return GetNeighborTile("southWest", target);
		}
		return null;
	}

	/* gets a random tile matching a ground type or not */
	public GameTile GetRandomTile (GroundType type = GroundType.None) {
		GameTile tile;

		int index = rnd.Next(0, cells.Length);
		tile = cells[index];
		if (type == GroundType.None) return tile;

		while (tile == null || tile.groundType != type) {
			index++; tile = cells[index];
		}

		return tile;

	}

	public GameTile GetTileByCoordenates(ICoordenate coordenates) {		
		if (coordenates.x < 0 || coordenates.y < 0
		 || coordenates.x >= width || coordenates.y >= height) 
		 {
			// Debug.Log("Skipping edge " + coordenates.x + "," + coordenates.y);
			return null;
		 }
		return System.Array.Find(cells, (cell) => {
			return cell.self.x == coordenates.x && cell.self.y == coordenates.y;
		});
	}

	public void Terraform(int tileId, GroundNature transformTarget) {
		cells[tileId].groundNature = transformTarget;
		int elev = rnd.Next(-1, 2);
		cells[tileId].elevation = elev;
		cells[tileId].groundType = GroundType.Walkable;
		cells[tileId].transform.GetComponent<SpriteRenderer>().color = GameControl.Instance.getTileColor(cells[tileId]);
	}

    void Awake () {
		cells = new GameTile[width * height];
		growthsTileCollection = new List<GameTile>[growths.Count];

		// time to make a world
		for (int x = 0, i = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				CreateCell(y, x, i++);
			}
		}

		for (int i = 0; i < growths.Count; i++){
			CreateGrowth(i);
		}
		
	}
	
	void CreateCell (int y, int x, int i) {
		Vector3 position;
		position.y = (y + x * 0.5f - x / 2) * (HexMetrics.innerRadius * 2f);
		position.x = x * (HexMetrics.outerRadius * 1.5f);
		position.z = 0f; // z * (HexMetrics.outerRadius * 1.5f);

		GameTile cell = cells[i] = Instantiate<GameTile>(cellPrefab);
		cell.transform.SetParent(transform, false);
		cell.transform.localPosition = position;
        
		cell.id = i;
		// some triangulation
		cell.self = new ICoordenate(x, y);

        cell.north = new ICoordenate(x, y + 1);

        cell.south = new ICoordenate(x, y - 1);

        cell.southWest = new ICoordenate( x - 1, x % 2 == 0 ? y - 1 : y);

        cell.northWest = new ICoordenate(x - 1, x % 2 == 0 ? y : y + 1);

        cell.southEast = new ICoordenate(x + 1, x % 2 == 0 ? y - 1 : y);

        cell.northEast = new ICoordenate(x + 1, x % 2 == 0 ? y : y + 1);
	}

	void CreateGrowth(int index) {
		IGrowth growthAtHand = growths[index];
		if (growthAtHand.height > height || growthAtHand.width > width) {
			Debug.LogError("Growth at index " + index + "Has dimensions that are larger than the map's");
			return;
		}
		
		// var setting
		// random first style far enough in the south to be able to grow
		int startY = rnd.Next(0, height - growthAtHand.height);
		int startX = rnd.Next(0, width - growthAtHand.width);

		ICoordenate coor = new ICoordenate(startX, startY);
		List<GameTile> goneTiles = new List<GameTile>();
		GameTile goingTile = GetTileByCoordenates(coor);
		goneTiles.Add(goingTile);

		// random first style can be set
		Terraform(goingTile.id, growthAtHand.growthNature);

		// then go randomly up based on height
		for(int i = 0; i < growthAtHand.height; i++){
			int upper = rnd.Next(1, 4);
			switch (upper)
			{
				case (1):
					goingTile = GetNeighborTile("north", goingTile.id);
					break;
				case (2):
					goingTile = GetNeighborTile("northEast", goingTile.id);
					break;
				case (3):
					goingTile = GetNeighborTile("northWest", goingTile.id);
					break;
			}
			if (goingTile == null) break;
			// transform all of them
			Terraform(goingTile.id, growthAtHand.growthNature);
			// and add each tile in this line to the list of gone
			goneTiles.Add(goingTile);
		}
	
		// for every width, have one round of growth
		for(int i = 0; i <growthAtHand.width; i++){
			// list to iterate through but not keep growing
			List<GameTile> iterationTiles = goneTiles;
			for(int j = 0; j <iterationTiles.Count; j++){
				// random dir now
				goingTile = GetRandomNeighborTile(iterationTiles[j].id);
				// if we couldn't get a tile, oh well
				if (goingTile == null) break;
				
				// otherwise add it to the count
				goneTiles.Add(goingTile);

				// random skip chance applies here
				int skipChance = rnd.Next(1, 11);
				if (growthAtHand.skipChance < skipChance)
				// only terraform if not skipped 
				Terraform(goingTile.id, growthAtHand.growthNature);
				
			}
		}

		growthsTileCollection[index] = goneTiles;
		
		

	}

	// private Vector2 cObjectToVector2(object vect) {
	// 	int x = vect.GetType().GetProperty("x", typeof(int)).GetValue(x) as int;
	// 	int y = vect.GetType().GetProperty("y", typeof(int)).GetValue(y) as int;
	// 	return new Vector2(x, y);
	// }

}
