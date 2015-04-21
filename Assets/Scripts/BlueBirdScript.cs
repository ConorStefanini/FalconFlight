using UnityEngine;
using System.Collections;

/*
**	BlueBirdScript.cs
**
**	Script that defines the behaviour of 
**		the (small) bluebirds in the scene
**	Includes basic steering as well as flocking
**
**	Conor Stefanini, 21 April 2015
*/

public class BlueBirdScript : MonoBehaviour {

	//	forces and speeds of the bluebird
	public Vector3 velocity;
	public Vector3 acceleration;
	public Vector3 force;
	//	used for force and velocity
	public float mass = 1;
	public float maxSpeed = 10f;
	

	//	flocking variables
	//	neighbour counter
	int nCount = 0;
	//	are that the bird pays attention to
	float softRad = 5;
	//	amount of other birds the bird pays attention to
	int neighbourLimit = 5;
	//	all of the other birds in the scene
	public GameObject[] neighbours;


	//	seeking enabled or disabled
	public bool seekO = false;
	//	seeking weight (how strong the seeking force is)
	public float seekW = 1;
	//	seeking target (seek towards)
	public Vector3 seekTarg = new Vector3(50, 10, 0);

	//	roost attraction variables
	public bool roostO = true;
	public float roostW = 5;
	//	position of the roost
	public Vector3 roostPos = new Vector3(0, 0, 0);
	//	size of the roost
	public Vector3 roostSize = new Vector3(20, 20, 20);

	//	flocking, cohesion, separation, alignment
	public bool cohO = true;
	public bool sepO = true;
	public bool aliO = true;
	public float cohW = 2;
	public float sepW = 30;
	public float aliW = 5;

	//	wander variables
	public bool wanO = true;
	public float wanW = 1;


	int i = 0;
	

	void Start () {

		velocity = Vector3.zero;
		acceleration = Vector3.zero;
		force = Vector3.zero;

	}


	public void updateNeighbours() {
		//	used to find the other bluebirds in the scene
		neighbours = GameObject.FindGameObjectsWithTag("Player");
	}
	

	Vector3 seek(Vector3 target) {
		//	generate a force in the direction of "target"
		Vector3 desiredVelocity = (target - transform.position).normalized;
		desiredVelocity *= maxSpeed;
		return desiredVelocity - velocity;
	}
	

	
	Vector3 roostVertical() {
		/*
		**	the vertical attraction to the roost
		**	the roost is a given area in space
		**		described by roostPos which is it's position in space
		**		and roostSize which is its size, 
		**		roostSize can describe a sphere: (2, 2, 2), or an elipse: (2, 1, 2)
		*/
		Vector3 force = Vector3.zero;
		force += new Vector3(0, ((roostPos.y - transform.position.y) / (roostSize.y / 2)), 0);
		return force;
	}
	
	Vector3 roostHorizontal() {
		Vector3 force = Vector3.zero;
		force += new Vector3((roostPos.x - transform.position.x) / (roostSize.x / 2), 0, 0);
		force += new Vector3(0, 0, (roostPos.z - transform.position.z) / (roostSize.z / 2));
		return force;
	}
	
	Vector3 roostAttraction() {
		Vector3 force = Vector3.zero;
		force += roostHorizontal();
		force += roostVertical();
		return force;
	}

	public Vector3 wander() {
		//	just a random force to provide some movement
		//		when no birds are in range
		//	works with other flocking forces
		Vector3 wanderTarg = Vector3.zero;
		float wandRad = 2;
		float wandDist = 3;
		float wandJitt = 0.1f;
		//	add small random displacement to the target
		wanderTarg += new Vector3(
			//wanderTarg = new Vector3(
			((Random.value * 2) - 1) * wandJitt,
			//((Random.value * 2) - 1) * wanderJitt,
			0, 
			((Random.value * 2) - 1) * wandJitt
			);
		//	project the wander target on to the wander circle
		wanderTarg = wanderTarg.normalized * wandRad;

		Vector3 tempTarg = wanderTarg + transform.position;
		tempTarg += (transform.forward.normalized * wandDist);
		return (tempTarg - transform.position);
	}
	
	public Vector3 separate() {
		//	separation
		//		maintain a certain distance from flock mates
		nCount = 0;
		Vector3 force = Vector3.zero;
		for (i = 0; i < neighbours.Length; ++i) {
			//	examined neighbour isn't self
			if ((neighbours[i] != gameObject)
			    //	neighbour is within radius
			    && ((neighbours[i].transform.position - transform.position).magnitude < softRad)){
				//	setting a limit to amount of neighbours as described in STARDisplay paper
				if (nCount < neighbourLimit) {
					nCount++;
					//	based on buckland's programming game AI
					Vector3 toBoid = transform.position - neighbours[i].transform.position;
					//	the (normal / magnitude) thing is to seperate more when the neighbour is closer 
					force += toBoid.normalized / toBoid.magnitude;
					
				}
			}
		}
		return force;
	}
	
	public Vector3 align() {
		//	alignment
		//		travel in the same direction as flock mates

		nCount = 0;
		
		Vector3 force = Vector3.zero;
		
		for (i = 0; i < neighbours.Length; ++i) {
			if ((neighbours[i] != gameObject) && ((neighbours[i].transform.position - transform.position).magnitude < softRad)){
				
				if (nCount < neighbourLimit) {
					
					force += neighbours[i].transform.forward;
					nCount++;
					
				}
				
			}
		}
		if (nCount > 0) {
			force /= nCount;
			force -= transform.forward;
		}
		return force;
	}
	
	public Vector3 cohere() {
		//	cohesion
		//		maintain a certain closeness to flock mates
		nCount = 0;
		Vector3 centreOfMass = Vector3.zero;
		Vector3 force = Vector3.zero;
		for (i = 0; i < neighbours.Length; ++i) {
			if ((neighbours[i] != gameObject) && ((neighbours[i].transform.position - transform.position).magnitude < softRad)){
				
				if (nCount < neighbourLimit) {
					centreOfMass += neighbours[i].transform.position;
					nCount++;
				}
				
			}
		}
		if (nCount > 0) {
			centreOfMass /= nCount;
			force = seek(centreOfMass);
		}

		return force;
	}


	
	void Update () {

		force = Vector3.zero;

		//	calculate forces based on weights
		if (seekO) {
			force += seek(seekTarg) * seekW;
		}

		if (roostO) {
			force += roostAttraction() * roostW;
		}
		if (wanO) {
			force += wander() * wanW;
		}


		if (sepO) {
			force += separate() * sepW;
		}
		if (cohO) {
			force += cohere() * cohW;
		}
		if (aliO) {
			force += align() * aliW;
		}

		//	F = ma, a = F/m
		acceleration = force / mass;
		//	v = u + at, velocity increases by acceleration multiplyed by time
		velocity += acceleration * Time.deltaTime;
		//	if velocity is bigger than max speed, clamp it
		if (velocity.magnitude > maxSpeed) {
			velocity = velocity.normalized * maxSpeed;
		}
		//	s = ut + 1/2 a t^2, position increases by velocity multiplied by time
		transform.position += velocity * Time.deltaTime;

		//	trying to face the bird models in the right direction
		//		the models came rotated oddly
		transform.rotation = Quaternion.AngleAxis(90, Vector3.right);

	}

}
