using UnityEngine;
using System.Collections;

public class FalconScript : MonoBehaviour {

	
	public Vector3 velocity;
	public Vector3 acceleration;
	public Vector3 force;
	
	public float mass = 1;
	public float maxSpeed = 100f;

	
	public bool pathO = true;
	public float pathW = 0;
	public bool pathLoop = false;
	public bool pathRand = false;
	public Vector3 pathRandDimensions = new Vector3(10, 10, 10);
	int pathIndex = 0;
	public float pathProx = 5;
	public Vector3[] path;
	
	public bool pathDemo = true;


	
	
	void Start () {
		
		//gameObject.tag = "Falcon";
		gameObject.AddComponent<Rigidbody>();
		gameObject.rigidbody.mass = mass;
		gameObject.rigidbody.useGravity = false;
		
		velocity = Vector3.zero;
		acceleration = Vector3.zero;
		force = Vector3.zero;
		
		pathO = true;
		
		if (pathDemo) {
			path = new Vector3[2];
			path[0] = new Vector3(50, 10, 0);
			path[1] = new Vector3(50, 50, 50);
		}
		
		transform.position = new Vector3(50, 50, -100);
		transform.rotation = Quaternion.AngleAxis(90, Vector3.right);
		
	}
	
	
	Vector3 seek(Vector3 target) {
		Vector3 desiredVelocity = (target - transform.position).normalized;
		desiredVelocity *= maxSpeed;
		return desiredVelocity - velocity;
	}
	
	
	Vector3 pathFollow() {
		//	check if at path point
		if ((path[pathIndex] - transform.position).magnitude < pathProx) { 
			if (pathRand) {
				//	random path
				path[pathIndex] = new Vector3(
					(Random.value * pathRandDimensions.x) - (pathRandDimensions.x / 2), 
					(Random.value * pathRandDimensions.y), 
					(Random.value * pathRandDimensions.z) - (pathRandDimensions.z / 2)
				);
			} else if (pathIndex == path.Length - 1) {
				//	if it's the last point
				if (pathLoop) {
					pathIndex = 0;
				} else {
					//return Vector3.zero;
				}
			} else {
				pathIndex++;
			}
		}

		return seek(path[pathIndex]);
	}
	
	
	
	
	void Update () {
	
		force = Vector3.zero;
		
		if (pathO) {
			force += pathFollow();
		}
		
		acceleration = force / mass;
		velocity += acceleration * Time.deltaTime;
		
		if (velocity.magnitude > maxSpeed) {
			velocity = velocity.normalized * maxSpeed;
		}

		transform.position += velocity * Time.deltaTime;
		
		//	point in direction of travel
		if (velocity.magnitude > float.Epsilon) {
			//transform.forward = velocity.normalized;
		}
	
	}
	
	
}
