using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{

    public GameObject showCover; 
    public Text playerIndex;
    public Button endTurnBttn;
    public Button stepButtn;
    public Button stepButtn2;
    public Button stepButtn5;
    public Button stepButtn10;
    // public Slider healthBar;
    // public Text steps;

    // Start is called before the first frame update
    void Start()
    {
        endTurnBttn.onClick.AddListener(FactionControl.Instance.EndTurn);

        stepButtn.onClick.AddListener(() => FactionControl.Instance.Step(1));
        stepButtn2.onClick.AddListener(() => FactionControl.Instance.Step(2));
        stepButtn5.onClick.AddListener(() => FactionControl.Instance.Step(5));
        stepButtn10.onClick.AddListener(() => FactionControl.Instance.Step(10));
    }

    // Update is called once per frame
    void Update()
    {
    
    }

    // Update is called once per frame
    public void AllowSteps(int steps)
    {
        if (steps > 1)  stepButtn.enabled = true;
        else stepButtn.enabled = false;

        if (steps > 2)  stepButtn2.enabled = true;
        else stepButtn2.enabled = false;

        if (steps > 5)  stepButtn5.enabled = true;
        else stepButtn5.enabled = false;

        if (steps > 10)  stepButtn10.enabled = true;
        else stepButtn10.enabled = false;
        
    }

    public void showHide(bool showHide) {
        showCover.SetActive(showHide);
    }
}
