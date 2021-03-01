using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMenu : MonoBehaviour
{
    public GameObject showCover;

    public GameObject KingsQueens;
    public GameObject knightsBishops;
    public GameObject pawnsRooks;
    

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
