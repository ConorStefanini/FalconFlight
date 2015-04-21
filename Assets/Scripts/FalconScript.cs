using UnityEngine;
using System.Collections;

/*
**	FalconScript.cs
**
**	Script to control the falcon in the scene
**	Includes basic steering behaviour (seek and path)
**
**	Conor Stefanini, 21 April 2015
*/

public class FalconScript : MonoBehaviour {

	//	variables used to calculate position
	public Vector3 velocity;
	public Vector3 acceleration;
	public Vector3 force;

	//	falcon's properties
	public float mass = 1;
	public float maxSpeed = 100f;

	//	path following variables
	//	path following enabled/disables
	public bool pathO = true;
	//	path following weighting (strength of path following force)
	public float pathW = 0;
	//	when reaching the end of the path, start again
	public bool pathLoop = false;
	//	set the path to random points within pathRandDimensions
	public bool pathRand = false;
	public Vector3 pathRandDimensions = new Vector3(10, 10, 10);
	//	counter for the path
	int pathIndex = 0;
	//	proximity to target when goal is reached
	public float pathProx = 5;
	//	path points
	public Vector3[] path;
	//	set random demonstration points
	public bool pathDemo = true;

	//	whether falcon is moving or not
	public bool go;

	//	seek variables
	public bool seekO = false;
	public Vector3 seekTarg;
	public float seekW = 1;


	void Start () {

		velocity = Vector3.zero;
		acceleration = Vector3.zero;
		force = Vector3.zero;

		//	start off following a path
		pathO = true;
		//	path points to follow
		if (pathDemo) {
			path = new Vector3[2];
			path[0] = new Vector3(50, 20, 0);
			path[1] = new Vector3(50, 50, 50);
		}

		//	start point
		transform.position = new Vector3(50, 50, -100);
		//	the falcon model points upwards
		//		this line tries to point the falcon forwards
		transform.rotation = Quaternion.AngleAxis(90, Vector3.right);

		//	the falcon starts off able to move
		go = true;

		//	the falcon isn't seeking anything at the start
		seekO = false;
		seekTarg = new Vector3(50, 20, 0);

	}
	

	Vector3 seek(Vector3 target) {
		//	seek
		//		generate force to travel towards "target"
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
	
		//	if the falcon can move
		//		this is changed by the camerascript
		//		to stop the falcon from moving when it's offscreen
		if (go) {
			
			force = Vector3.zero;
			//	calculate forces
			if (pathO) {
				force += pathFollow();
			}
			if (seekO) {
				force += seek(seekTarg) * seekW;
			}
			//	F = ma, a = F/m
			acceleration = force / mass;
			//	v = u + at, velocity is increased by acceleration multiplyed by time
			velocity += acceleration * Time.deltaTime;
			//	clamp the velocity if it's over the maximum speed
			if (velocity.magnitude > maxSpeed) {
				velocity = velocity.normalized * maxSpeed;
			}
			//	position is increased by velocity at each step in time
			transform.position += velocity * Time.deltaTime;
			
		}
	
	}
	
	
}
