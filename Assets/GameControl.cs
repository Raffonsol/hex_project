using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using System;

public static class HexMetrics {

	public const float outerRadius = 0.7f;

	public const float innerRadius = outerRadius * 0.866025404f;

    public static Vector3[] corners = {
		new Vector3(0f, 0f, outerRadius),
		new Vector3(innerRadius, 0f, 0.5f * outerRadius),
		new Vector3(innerRadius, 0f, -0.5f * outerRadius),
		new Vector3(0f, 0f, -outerRadius),
		new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
		new Vector3(-innerRadius, 0f, 0.5f * outerRadius)
	};
}

[Serializable]
public class ITileProperties {
    public string name;
    public int stepCost;
    public Color color;
    public GameObject[] defaultImages;
}

public class IPath {
    public List<GameTile> path;
    public int stepCost;

    public IPath(int cost, List<GameTile> path) {
        this.path = path;
        stepCost = cost;
    }
}

public enum CharClass {
    Pawn,
    Knight,
    Bishop,
    Rook,
    King,
    Queen,
    Object
}
public class GameControl : MonoBehaviour
{
    #region "Singleton"

    private static GameControl _instance;
    public static GameControl Instance {
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

    public SelectionMenu selectionMenu;
    public GameMenu gameMenu;

    public HexGrid hexGrid;

    // game configs
        [SerializeField]
        public List<ITileProperties> tileNatureProperties;

    public Chararcter selected;


    public int startingPlayer = 1; // 0 is independent
    private int controllers;
    private GameTile lastTileClicked;

    private GameTile[] lastDrawnPlot;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (IsPointerOverUIObject()) return;
        if (Input.GetMouseButtonDown(0)) {
           Select();
        }
        if (Input.GetMouseButtonDown(1)) {
           Action();
        }
    }

    private bool IsPointerOverUIObject() {
        // Referencing this code for GraphicRaycaster https://gist.github.com/stramit/ead7ca1f432f3c0f181f
        // the ray cast appears to require only eventData.position.
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
 
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
    
    public void Initialize(int controllers) {
        this.controllers = controllers;
    }

    public int Pass() {
        startingPlayer = startingPlayer == controllers ?  0 : startingPlayer + 1;
        Debug.Log("Player " + (startingPlayer) + " turn");
        
        // find all characters belonging to turn-just-passed owner and reset their actions

        return startingPlayer;
    }

    /* Tile selection */
    void Select(){
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
        
        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
        if (hit.collider != null) {
        
            // clear previous selection
             if (lastTileClicked != null)
            resetTileColor(lastTileClicked);

            lastTileClicked = hit.collider.transform.GetComponent<GameTile>();
            hit.collider.transform.GetComponent<SpriteRenderer>().color = Color.red;
            if (lastTileClicked.occupier >= 0) {
                Chararcter unit = GameObject.Find("unit"+lastTileClicked.occupier).GetComponent<Chararcter>();
                setSelection(unit);
            } else {
                ClearLastPath();
                selected = null;
                selectionMenu.showHide(false);
            }
        }
    }

    public void setSelection(Chararcter select) {
        selected = select;
                if (selected.owner == startingPlayer) {
                    // owned unit selected
                    if (isPlotValid(selected.plottedTilePath)) DrawPath(selected.plottedTilePath);
                    selected.steppingOn.transform.GetComponent<SpriteRenderer>().color = Color.blue;
                } else {
                    // not-owned unit selected
                }
                selected.steppingOn = lastTileClicked;
                selectionMenu.showHide(true);
                selectionMenu.name.text = selected.name + " Lvl: " + selected.level;
                selectionMenu.steps.text = selected.actionsLeft.ToString();
                selectionMenu.stepsPlanned.text = "Planned for " +selected.plottedSteps.ToString()+ (selected.plottedSteps > 1 ? " steps" : " step");
                selectionMenu.healthBar.value = selected.lifePoints / selected.getMaxLife();
                selectionMenu.resetBttn.onClick.AddListener(() => 
                {selected.plottedTilePath = null; selected.plottedSteps = 0; ClearLastPath();});
    }

    public ITileProperties getTilePropertyFromName(string name) {
        return tileNatureProperties[tileNatureProperties.FindIndex((ITileProperties prop) => 
                {return prop.name.Equals(name);})];
    }

    public Color getTileColor(GameTile tile) {
        Color resultant = getTilePropertyFromName(tile.groundNature.ToString()).color;
        for(int i = 0; i < Math.Abs(tile.elevation); i++){
            resultant = Color.Lerp(resultant, (tile.elevation > 0 ? Color.white : Color.black), 0.1f);
        }
        
        return resultant;
    }

    public Transform Spawn(Chararcter character, ICoordenate at = null) {
        GameTile target = at !=null ? hexGrid.GetTileByCoordenates(at) : hexGrid.GetRandomTile(GroundType.Walkable);
        character.Begin();
        target.occupier = character.id;
        character.steppingOn = target;
        return Instantiate(character.transform, target.transform.position, Quaternion.identity);
    }

    private void resetTileColor(GameTile tile) {
        tile.GetComponent<SpriteRenderer>().color =
                getTileColor(tile);
    }
    private void performFriendlyUnitSelect() {
        // show menu with full knowledge
    }
    private void performUnfriendlyUnitSelect() {
        // show menu with limited knowledge
    }

    private int getStepCountFromGroundNature(GroundNature nature, int changingElev = 0) {
        return getTilePropertyFromName(nature.ToString()).stepCost + changingElev;
    }

    void Action() {        
        if (!canAction()) 
        return;

         Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
        
        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
        if (hit.collider != null) {
        
            
            GameTile actioned = hit.collider.transform.GetComponent<GameTile>();
            performAction(actioned);
           
            
            // hexGrid.cells[actioned.id];

        }
    }

    public void DrawPath(GameTile[] path) {
        // politics first
        ClearLastPath();
        lastDrawnPlot = path;

        // then the fun stuff
        for(int i = 0; i <path.Length; i++){ 
            path[i].transform.GetComponent<SpriteRenderer>().color = Color.yellow;
            
        }
    }

    public void ClearLastPath() {
        if (lastDrawnPlot == null) 
            return;
        
        for(int i = 0; i <lastDrawnPlot.Length; i++){
            resetTileColor(lastDrawnPlot[i]);
        }
        
    }

    public void performAction(GameTile actioned) {
         // if they clicked on themselves let's reset their plot
            if (actioned.id == selected.steppingOn.id) {
                selected.plottedTilePath = null;
                selected.plottedSteps = 0;
                ClearLastPath();
                setSelection(selected);
                // TODO: If they clicked somewhere on their plot, reset plot to that point
            } else { // otherwise lets pathfind and suggest a plot
                IPath pathObj = Pathfind(actioned);
                GameTile[] found = pathObj.path.ToArray();

                if (isPlotValid(selected.plottedTilePath)) { // if plot is started, append to existing
                    GameTile[] appended = new GameTile[selected.plottedTilePath.Length + found.Length];

                    Array.Copy(found, appended, found.Length);
                    Array.Copy(selected.plottedTilePath, 0, appended, found.Length, selected.plottedTilePath.Length);

                    selected.plottedTilePath = appended;
                } else {
                    selected.plottedTilePath = found;
                }
                selected.plottedSteps += pathObj.stepCost;
                DrawPath(selected.plottedTilePath);
                setSelection(selected);
            }
    }

    private IPath Pathfind(GameTile target) {        
        
        // determine starting point (will be endpoint here)
        GameTile charTile;
        if (isPlotValid(selected.plottedTilePath) ) {
            charTile = selected.plottedTilePath[0];//Plotting from last point in last plotted 
            List<GameTile> list = selected.plottedTilePath.ToList();
            list.RemoveAt(0);
            selected.plottedTilePath = list.ToArray();
        } else {
            selected.plottedTilePath = null; // this is just tod eal with some weird caching that keeps the path from last test
            charTile = selected.steppingOn;
        }             
        
        int xDiff = charTile.self.x - target.self.x;
        int yDiff = charTile.self.y - target.self.y;
        
        // we are going backwards, starting at target
        GameTile currentTile = target;

        // determine quickest available path. Store determined steps starting by final destination
        List<GameTile> nextStep = new List<GameTile>();
        nextStep.Add(currentTile);
        // list of tiles we already tried and it takes us nowhere. Don't go to noGoes
        List<int> noGoes = new List<int>();
        
        int countSteps = 0;
        // as long as there is still a distance to cover
        // meaning we should recalculate this distance for each step taken
        int runs=0;
        while (xDiff != 0 || yDiff !=0 )
        {// this is where we decide what the next tile is going to be
            // prevent crash
            runs++;
            if (runs>1000){
                for(int i = 0; i <nextStep.Count; i++){ 
                        Debug.Log(nextStep[i].self.x + " | " + nextStep[i].self.y);
            
                 }  Debug.Log("Crashed on first calculation");  
                return null;} // TODO: Change this to error

            // if char is adjacent then it's next to us
            if (yDiff == 0 && Math.Abs(xDiff) == 1 
            || xDiff == 0 && Math.Abs(yDiff) == 1 ) {
                currentTile = charTile;
            } else {
                // of all available next steps
                List<GameTile> availableNeighbors = hexGrid.GetAvailableNeighborTiles(currentTile.id);
                if (availableNeighbors.Count <= 0) return null; // ain't no available path

                for(int i = 0; i < availableNeighbors.Count; i++) {
                    // remove any already thread
                    if (nextStep.Contains(availableNeighbors[i]) || noGoes.Contains(availableNeighbors[i].id)){
                        availableNeighbors.RemoveAt(i);
                        i--;
                    }
                }
                
                // a deadend
                if (availableNeighbors.Count <= 0){
                    if (currentTile.id == target.id) {
                        return null; // ain't no available path
                    } else {
                        // just not this way, mark it as noGo and go back one
                        noGoes.Add(currentTile.id);
                        // go back to second last step, now it shouldn't be able to go here again
                        currentTile = nextStep[nextStep.Count-2];

                        // disconsider last one
                        countSteps -= getStepCountFromGroundNature(nextStep[nextStep.Count - 1].groundNature);
                        nextStep.RemoveAt(nextStep.Count - 1);

                        // the distance also needs to be recalculated
                        xDiff = charTile.self.x - currentTile.self.x;
                        yDiff = charTile.self.y - currentTile.self.y;

                    }
                    // next moves must be inside else so that it restarts loop when a deadend is reached
                } else {                  

                    // lets keep track of the distance of each possible path
                    int[] parallelDistances = new int[availableNeighbors.Count];
                    int lowestValue = -1; // the lowest distance we could find
                    for(int i = 0; i <availableNeighbors.Count; i++){
                        // find the absolute x and y distance for this speculation
                        int xSpec = Math.Abs(charTile.self.x - availableNeighbors[i].self.x);
                        int ySpec = Math.Abs(charTile.self.y - availableNeighbors[i].self.y);

                        // the next step is complicated. This is my current draft
                        // I want to calculate the distance by using the sum of both distances
                        // however, the one that has the longest distance to go is doubled
                        // this is so that the path can worry about crossing the long distance first before having to worry about the short distance
                        int dist = xSpec * (Math.Abs(xDiff) > Math.Abs(yDiff) ? 2 : 1) + ySpec * (Math.Abs(yDiff) > Math.Abs(xDiff) ? 2 : 1);
                        // taking step cost into consideration and settig it on candidates list
                        // TODO: consider changing elevation index here
                        parallelDistances[i] = dist * getStepCountFromGroundNature(availableNeighbors[i].groundNature);
                        // keep track of the lowest dist cost found
                        if (lowestValue == -1 || lowestValue > parallelDistances[i])
                        lowestValue = parallelDistances[i];
                    }
                    // now all the ones that have the lowest distance cost are potential candidates,
                    int removed = 0;
                    for(int i = 0; i <parallelDistances.Length; i++) {                    
                        // remove any that doesn't have lowest value cause it's too long
                        if (parallelDistances[i] != lowestValue) {                        
                            availableNeighbors.RemoveAt(i - removed);
                            removed++;
                        }
                    } // this loop should always end with at least 1 value

                    // for now just make the next step the first index in the remaining available options, it shouldn't matter
                    currentTile = availableNeighbors[0];
                    // if it's accessible from nextTile[count -2], remove nextTile[count -1]
                    // if (nextStep.Count >= 2 && hexGrid.GetAvailableNeighborTiles(nextStep[nextStep.Count - 2].id).Contains(currentTile)) {
                    //     countSteps -= getStepCountFromGroundNature(nextStep[nextStep.Count - 1].groundNature);
                    //     noGoes.Add(nextStep[nextStep.Count - 1].id);// shouldn't add it to no goes because it might be needed again
                        
                    //     nextStep.RemoveAt(nextStep.Count - 1);
                    //     // could this create an error? might it keep going back and removing a tile? should I add a noGoes inside the while loop?
                    //     // TODO: consider the above TODO: Do this after
                    // }
                } // end deadend else
            } // end ifclause deciding next tile

            // re calculating distance
            xDiff = charTile.self.x - currentTile.self.x;
            yDiff = charTile.self.y - currentTile.self.y;
            nextStep.Add(currentTile);
            
            // more steps added for the count
            countSteps+= getStepCountFromGroundNature(currentTile.groundNature);
        } // end while loop

        // now we need to cut the fat but looping through it
        bool thereWereChanges = true;
        // keep running until variable gets to stay false
        runs = 0;
        while (thereWereChanges) { // if we cut some fat last time we might be able to do so again
            
            runs++; if(runs>1000){Debug.Log("Crashed on fat cutting");return null;}
            
            thereWereChanges = false; // cause there weren't any yet ;)
            int cutStart = 0; int cutEnd = 0; // start and end of fat

            for(int i = 0; i < nextStep.Count-1; i++){// for every step
                for(int j = i+2; j < nextStep.Count; j++){ // loop through steps that come after
                // if we can go from tile i to tile j
                    if (hexGrid.GetAvailableNeighborTiles(nextStep[j].id).Contains(nextStep[i])) {
                        // then we can get rid of every tile between i and j cause it fat
                        cutStart = i+1; cutEnd = j;
                        thereWereChanges = true; // told you not to worry ;)
                    } else if(nextStep[j].id == nextStep[i].id) { // or if they are the same TODO: Look into this
                        cutStart = i; cutEnd = j;
                        thereWereChanges = true;
                    }
                }
            }

            // that was just measuring, now its the real cutting
            if (thereWereChanges){
                for(int a = cutStart; a < cutEnd; a++){
                    nextStep.RemoveAt(a);
                    countSteps -= getStepCountFromGroundNature(nextStep[a].groundNature);
                }
            }
        }

        return new IPath(countSteps, nextStep);
    }

    private bool canAction() {
        return selected != null && selected.owner == startingPlayer;
    }

    private bool isPlotValid(GameTile[] plot) {
        return plot != null && plot.Length > 0
        && plot[0] != null;
    }
}
