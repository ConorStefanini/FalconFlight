using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour {

	public GameObject falcon;
	public Vector3 offset = new Vector3(0, 0, 1F);
	public Vector3 direction;
	
	void Start () {
		
		offset = new Vector3(0, 0, 2F);
		
	}
	
	void Update () {
		
		//falcon = GameObject.FindGameObjectWithTag("Falcon");
		
		Vector3 calcPos = falcon.transform.position + offset;
		transform.position = calcPos;
		
		direction = falcon.transform.position - transform.position;
		transform.forward = direction;
	}
}
