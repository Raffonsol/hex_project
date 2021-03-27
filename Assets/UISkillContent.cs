using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISkillContent : MonoBehaviour
{

    public GameObject skillPrefab;


    [HideInInspector]
    public ISkill[] skillsShowing;
    
    public void ShowSkills(ISkill[] skills) {
        HideSkills();
        
        // populate with given
        this.skillsShowing = skills;
         GetComponent<RectTransform>().sizeDelta = new Vector2((skills.Length-4)*60f, GetComponent<RectTransform>().sizeDelta.y);
        for(int i = 0; i <skills.Length; i++){
            GameObject newObj; // Create GameObject instance
             // Create new instances of our prefab until we've created as many as we specified
			newObj = (GameObject)Instantiate(skillPrefab, transform);
           
            newObj.transform.localPosition = new Vector3(60f+i*60f, newObj.transform.localPosition.y, 0f);
			newObj.GetComponent<Image>().sprite = skills[i].icon;
            // add listener to use skill
            newObj.GetComponent<Button>().onClick.AddListener(() => {
                GameControl.Instance.ProjectSkill(i);
            });
        }
        
    }

    public void HideSkills()
    {
        this.skillsShowing = null;
        foreach (Transform child in transform)
            Destroy(child.gameObject);
    }
}
