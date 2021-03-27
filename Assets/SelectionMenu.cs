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

    public UISkillContent skillContent;

    public void ShowSelection(Chararcter selected) {
        showHide(true);
        name.text = selected.name + " Lvl: " + selected.level;
        steps.text = selected.actionsLeft.ToString();
        stepsPlanned.text = "Planned for " +selected.plottedSteps.ToString()+ (selected.plottedSteps > 1 ? " steps" : " step");
        healthBar.value = selected.lifePoints / selected.getMaxLife();
        resetBttn.onClick.AddListener(() => 
        {
            Chararcter select = GameObject.Find("unit"+selected.id).GetComponent<Chararcter>();
            select.plottedTilePath = null; select.plottedSteps = 0; GameControl.Instance.ClearLastPath();
        });
        skillContent.ShowSkills(selected.skills.ToArray());
    }

    public void HideSkills() {
        skillContent.HideSkills();
    }

    public void showHide(bool showHide) {
        showCover.SetActive(showHide);
    }
}
