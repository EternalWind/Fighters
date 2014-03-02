using UnityEngine;
using System.Collections;

public class Explosion2 : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (particleSystem.isStopped)
            Destroy(gameObject);
	}
}
