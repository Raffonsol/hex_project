using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum FactionType {
    Human,
    Ai,
    Independents,
}

[Serializable]
public class IFaction {
    public string playerName;
    public int playerIndex;
    public Color color;
    public FactionType factionType;
    public Chararcter leader;
}

public class FactionControl : MonoBehaviour
{

    #region "Singleton"

    private static FactionControl _instance;
    public static FactionControl Instance {
        get {
            if (_instance == null) {
                Debug.LogError("You have not instantiated the GameConstants in the scene, but you are trying to access it");
                return null;
            }
            return _instance;
        }
    }
   
   private void Awake() {
       _instance = this;
   }
   #endregion
    
    public PlayerMenu playerMenu;
    [SerializeField]
    public List<IFaction> factions;

    private int turn;
    private int players;

    // all below are parallel with factions
    private static int[] playerTurns; // counts number of turns
    private static List<int>[] playerChars; // array of character list

    private IEnumerator coroutine;

    // Start is called before the first frame update
    void Start()
    {
        // start at whatever game control says we should start
        int turn = GameControl.Instance.startingPlayer;
        int players = factions.Count;

        GameControl.Instance.Initialize(players - 1);

        playerTurns = new int[players];
        playerChars = new List<int>[players];

        // give everyone their elader
        for(int i = 0; i <playerChars.Length; i++) {
            if (factions[i].leader != null) {

                Transform instance = GameControl.Instance.Spawn(factions[i].leader);
                Chararcter leader = instance.GetComponent<Chararcter>();
                instance.name = "unit"+leader.id;
                
                leader.owner = i;
                // make sure the leader now points to instance not prefab
                factions[i].leader = leader;

                AddChararcter(i, leader);
                
            }
        }

        startPlayerTurn(turn);        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EndTurn() {
        resetPlayer(turn);
        turn = GameControl.Instance.Pass();
        GameControl.Instance.gameMenu.playerIndex.text = turn.ToString();
        startPlayerTurn(turn);
    }

    public void Step(int moves) {
        turn = GameControl.Instance.startingPlayer;
        
        
        List<int> units = playerChars[turn];
        
        for(int i = 0; i < units.Count; i++){
            coroutine = TimeStep("unit"+units[i], moves);
            StartCoroutine(coroutine);
            // TimeStep("unit"+units[i], moves);
        }
    }

    IEnumerator TimeStep(string unitName, int repeats) {
        Chararcter unit = GameObject.Find(unitName).GetComponent<Chararcter>();
        unit.speed = 1 + .2f*repeats;
       for(int j = 0; j < repeats; j++){ // for every move to be made
            if (unit.plottedTilePath != null && unit.plottedTilePath.Length >= 1) {
                unit.Move();
                
                yield return new WaitForSeconds(.3f);
            }
        }
    }

    private void AddChararcter(int ownerIndex, Chararcter recruit) {
        // list part
        if (playerChars[ownerIndex] == null) playerChars[ownerIndex] = new List<int>();
        playerChars[ownerIndex].Add(recruit.id);
    }

    /**
    * Should be to reset everything about a player as soon as they pass turn 
    */
    private void resetPlayer(int player) {
        playerTurns[player]++;

        // reset all character steps <= TODO
    }

    /**
    * Should be for anythign that has to happen when a player's turn is started
    */
    private void startPlayerTurn(int player) {
        // if they have a leader, move camera to em
        if (factions[player].leader != null) {
            Camera.main.transform.position = new Vector3(factions[player].leader.transform.position.x, factions[player].leader.transform.position.y, -10f) ;
            Debug.Log(factions[player].leader.transform.position);
            
        }
        // drumroll... UI stuff! <= TODO 
    }
}
