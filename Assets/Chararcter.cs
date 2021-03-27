using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum SkillTargetType {
    Self,
    Forward,
    Around,
    Backward,
    Throw
}
public enum SkillFactionsAllowed {
    Friendly,
    Hostile,
    Both,
    Neither,
}
public enum SkillEffectType {
    None,
    Damage,
    Heal,
    Build,
    Conjure,
    Buff,
    Debuff,
}
public enum SkillSpecialEffectType {
    None,
    RelocateSelf,
    PushHit,
    PullClose,
}

[Serializable]
public class ISkill {
    
    [HideInInspector]
    public int id; // generated
    public int acquiredAtLvl;
    public string name;
    
    public Sprite icon;
    public SkillTargetType targetType;
    public SkillEffectType effectTypeOnEnemies;
    public SkillEffectType effectTypeOnFriends;
    public SkillFactionsAllowed factionsAllowed;
    public SkillSpecialEffectType specialEffectType;
    // TODO: maybe add a lil something for particle effect

    public int actionCost;
    public int actionCostPerLevel;
    public float range;
    public float rangePerLevel;
    public float dice;
    public float dicePerLevel;

    // TODO: Make interface for terrain change effects. To include prefab for overlay and terrainNature/type 
    
}

public class Chararcter : MonoBehaviour
{
    new public string name;
    public int owner; // 0 is independent

    public int startingMaxActions;
    public int upgradeMaxActions; // how many points it increases per level

    public int startingMaxLife;
    public int upgradeMaxLife; // how many points it increases per level

    public CharClass charClass = CharClass.Pawn;

    [SerializeField]
    public List<ISkill> skills;

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
    private int lastDir = 2; // to determine default direction
    

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
       
        steppingOn.occupier = -1; // TODO: if that becomes array, just remove this guy
        // remove next step
        
        List<GameTile> list = new List<GameTile>(plottedTilePath);
        int dir;
        if (steppingOn.self.x > list[list.Count-2].self.x) dir = 1; else dir = 2;
        if (dir != lastDir) transform.Rotate(0, 180, 0); 
        lastDir = dir;

        list.RemoveAt(list.Count-1);
        GameControl.Instance.ClearLastPath();
        plottedTilePath = list.ToArray();
        steppingOn = plottedTilePath[list.Count-1];
        steppingOn.occupier = id; // TODO: same as above
        if (plottedTilePath.Length == 1) {plottedTilePath = null;}

         // start  showing little movement anim
        GetComponent<Animator>().SetInteger("moveAnimation", lastDir);
        speed = speedUp;

        // this makes it move
        targetPosition = steppingOn.transform.position;
        moving = true;
        
    }
}
