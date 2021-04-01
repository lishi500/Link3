using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonUtil : Singleton<CommonUtil>
{

      // ease in and out
    public float Smoothstep(float t) {
        return t * t * (3 - 2 * t);
    }

    public GameObject GetPrefabByName(string name) {
        return (GameObject) Resources.Load("Prefab/" + name);
        
    }
}
