using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionMenu : MonoBehaviour
{

    public GameObject showCover; 
    public Slider healthBar;
    new public Text name;
    public Text steps;
    public Text stepsPlanned;
    public Button resetBttn;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    
    }

    public void showHide(bool showHide) {
        showCover.SetActive(showHide);
    }
}
