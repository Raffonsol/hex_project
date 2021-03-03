using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chararcter : MonoBehaviour
{
    new public string name;
    public int owner; // 0 is independent

    public int startingMaxActions;
    public int upgradeMaxActions; // how many points it increases per level

    public int startingMaxLife;
    public int upgradeMaxLife; // how many points it increases per level

    public CharClass charClass = CharClass.Pawn;

    public int level = 0;
    public GameTile steppingOn;
    
    [HideInInspector]
    public int id;
    [HideInInspector]
    public int actionsLeft;
    [HideInInspector]
    public int lifePoints;
    [HideInInspector]
    public GameTile[] plottedTilePath;
    [HideInInspector]
    public int plottedSteps;
    [HideInInspector]
    public float speed = 1.3f;

    private bool moving = false;
    private Vector2 targetPosition;
    
    

    // Start is called before the first frame update
    public void Begin()
    {
        actionsLeft = startingMaxActions + upgradeMaxActions*level;
        lifePoints = startingMaxLife + upgradeMaxLife*level;
        plottedTilePath = null;   
        plottedSteps = 0;     
        targetPosition = transform.position;
    }

    // Update is called once per frame
    private void Update()
    {
        // all movement going forward
        if (!moving) return;
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPosition) < 0.1f) {
            // last step for style
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, 5f);
            moving = false;
            GetComponent<Animator>().SetInteger("moveAnimation", 0);
            speed = 1;
        }
    }
    public int getMaxLife() {
        return startingMaxLife + upgradeMaxLife*level;
    }

    // just move to next tile on the plot. Mostly politics and animation
    public void Move(int speedUp = 1) {
        if (plottedTilePath == null || plottedTilePath.Length == 0)Debug.LogError("A movement was attempted without a plotted path");
        // start aniamtion showing little movement
        GetComponent<Animator>().SetInteger("moveAnimation", 1);
        speed = speedUp;
for(int i = 0; i <plottedTilePath.Length; i++){Debug.Log(plottedTilePath[i].self.x + " - "+plottedTilePath[i].self.y);

}

        steppingOn.occupier = -1; // TODO: if that becomes array, just remove this guy
        // remove next step
        List<GameTile> list = new List<GameTile>(plottedTilePath);
        list.RemoveAt(list.Count-1);
        GameControl.Instance.ClearLastPath();
        plottedTilePath = list.ToArray();
        steppingOn = plottedTilePath[list.Count-1];
        steppingOn.occupier = id; // TODO: same as above
        if (plottedTilePath.Length == 1) {plottedTilePath = null;}

        // this makes it move
        targetPosition = steppingOn.transform.position;
        moving = true;
        Debug.Log("target now "+ targetPosition);
        
    }
}
