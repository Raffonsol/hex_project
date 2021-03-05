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
                Debug.LogError("You have not instantiated the FactionControl in the scene, but you are trying to access it");
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

    private bool movementHappening = false;

    // all below are parallel with factions
    private static int[] playerTurns; // counts number of turns
    private static List<int>[] playerChars; // array of character list

    private IEnumerator coroutine;

    // Start is called before the first frame update
    void Start()
    {
        // start at whatever game control says we should start
        FactionControl.Instance.turn = GameControl.Instance.startingPlayer;
        FactionControl.Instance.players = factions.Count;

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

    public void UpdateStepButtons()
    {
        List<int> units = playerChars[turn];
        
        int totalStepsPlotted = getStepButtons(units);    
        GameControl.Instance.gameMenu.AllowSteps(totalStepsPlotted);
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
            UpdateStepButtons();
        }
    }

    IEnumerator TimeStep(string unitName, int repeats) {
        Chararcter unit = GameObject.Find(unitName).GetComponent<Chararcter>();
        unit.speed = 1 + .2f*repeats;
       for(int j = 0; j < repeats; j++){ // for every move to be made
            if (unit.plottedTilePath != null && unit.plottedTilePath.Length >= 1) {
                movementHappening = true;
                unit.Move();
                
                yield return new WaitForSeconds(.3f);
            }
            movementHappening = false;
        }
        UpdateStepButtons();
    }

    /* return the shortest amount of plottedsteps from given list of unit ids */
    private int getStepButtons(List<int> units) {
        if (units == null || units.Count <= 0 || movementHappening) return 0;
        int shortestLength = -1;
        for(int i = 0; i < units.Count; i++){
            Chararcter unit = GameObject.Find("unit"+units[i]).GetComponent<Chararcter>();
            if (unit.plottedTilePath == null || unit.plottedTilePath.Length < 1) {
                return 0;
            } else if (shortestLength == -1 || unit.plottedTilePath.Length < shortestLength) {
                shortestLength = unit.plottedTilePath.Length;
            }
        }
        return shortestLength;
    }

    private void AddChararcter(int ownerIndex, Chararcter recruit) {
        // adding to the list of existing characters
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
            
        }
        // drumroll... UI stuff! <= TODO 
        UpdateStepButtons();
    }
}
