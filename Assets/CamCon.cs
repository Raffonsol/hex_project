using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamCon : MonoBehaviour
{

    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 direction = Vector3.zero;
         if (Input.GetKey("w")) {
             direction.y = 10;
         }
         if (Input.GetKey("s")) {
             direction.y = -10;
         }
         if (Input.GetKey("d")) {
             direction.x = 10;
         }
         if (Input.GetKey("a")) {
             direction.x = -10;
         }


        // https://docs.unity3d.com/2020.1/Documentation/ScriptReference/Time-deltaTime.html
        float timeSinceLastFrame = Time.deltaTime;

        Vector3 translation = direction * timeSinceLastFrame;

        transform.Translate(
          translation
        );
    }
}
