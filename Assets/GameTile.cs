using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GroundType{
    Walkable,
    Occupied,
    Blocked,
    None, // not to be used
}
public enum GroundNature{
    Water,
    Dirt,
    Grass,
    Tree,
    Rock,
    Road,
}

public class ICoordenate {
    public int x;
    public int y;

    public ICoordenate(int xC, int yC) {
        x = xC;
        y = yC;
    }
}

public class GameTile : MonoBehaviour
{

    public GroundType groundType;
    public GroundNature groundNature;
    public int elevation = 0;
    public int owner;
    public bool commonWealth = true;
    public int occupier = -1;

    [HideInInspector]
    public int id;

    [HideInInspector]
    public ICoordenate self;

    [HideInInspector]
    public ICoordenate north;
    [HideInInspector]
    public ICoordenate northWest;
    [HideInInspector]
    public ICoordenate northEast;
    [HideInInspector]
    public ICoordenate south;
    [HideInInspector]
    public ICoordenate southWest;
    [HideInInspector]
    public ICoordenate southEast;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    
    }
}
