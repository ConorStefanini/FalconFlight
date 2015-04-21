using UnityEngine;
using System.Collections;

/*
**	CameraScript.cs
**
**	This script provides the main control over the scene, 
**		the camera, and the birds in the scene
**
**	Conor Stefanini, 21 April 2015
*/

public class CameraScript : MonoBehaviour {

	public GameObject falcon;
	//	offset of the camera itself
	public Vector3 offset = new Vector3(0, 0, 1F);
	//	direction the camera points in
	public Vector3 direction;
	
	//	an array for the bluebirds in the scene
	public GameObject[] blues;
	//	the amount of bluebirds to be spawned
	public int bluesNum;

	//	roost variables
	//		the roost is an area that the flock desires to stay in
	//	roost position, roost is used by the bluebirds to flock
	public Vector3 roostP = new Vector3(0, 30, 0);
	//	roost size
	public Vector3 roostS = new Vector3(20, 20, 20);

	//	"mode" defines how the camera behaves
	//		different modes point at the falcon or the flock
	//		they will be described in more detail later
	public int mode = 4;

	//	camera rotation
	//		used when the camera is focused on the flock or the bluebird
	//	the current rotation of the camera around the flock
	public float rotaty = 0F;
	//	the amount to increment the rotation at each step
	public float rotAm = 5F;

	//	original direction the camera points in
	//		used for some quaternion maths when the camera 
	//		points at the flock
	public Vector3 targLookOrig = new Vector3(0, 0, 1F);

	//	scene switching
	//		not unity scenes, just camera modes and 
	//		bird behaviours
	//	used to count time
	public float timer;
	//	used to dictate how the scene/camera is set up
	public float sceneCount;

	//	used as a flag to ensure flock only 
	//		has values modified once in loop
	int miscCount = 1;

	//	used for when the falcon catches the bluebird
	bool blueCaught = false;


	
	void Start () {

		//	camera offset, starts on falcon
		offset = new Vector3(0, 0, 2F);

		//	roost position and size
		roostP = new Vector3(0, 20, 0);
		roostS = new Vector3(10, 10, 10);

		//	twenty bluebirds in the scene
		bluesNum = 20;
		blues = new GameObject[bluesNum];
		
		for (int i = 0; i < bluesNum; ++i) {

			//	initialise bluebirds
			//	initialise model
			blues[i] = (GameObject)Instantiate(Resources.Load("BlueBird"));
			//	add bluebird script
			blues[i].AddComponent<BlueBirdScript>();
			//	set bluebirds to start at a random point in the roost
			Vector3 pos = new Vector3(
				((Random.value * roostS.x) - (roostS.x / 2) + roostP.x), 
				((Random.value * roostS.y) - (roostS.y / 2) + roostP.y), 
				((Random.value * roostS.z) - (roostS.z / 2) + roostP.z));
			blues[i].GetComponent<BlueBirdScript>().transform.position = pos;

			//	set the behaviour and weights for the birds
			//	this is default set up for flocking behaviour
			blues[i].GetComponent<BlueBirdScript>().seekO = false;
			blues[i].GetComponent<BlueBirdScript>().roostO = true;
			blues[i].GetComponent<BlueBirdScript>().roostW = 5;

			blues[i].GetComponent<BlueBirdScript>().roostPos = roostP;
			blues[i].GetComponent<BlueBirdScript>().roostSize = roostS;

			blues[i].GetComponent<BlueBirdScript>().sepO = true;
			blues[i].GetComponent<BlueBirdScript>().cohO = true;
			blues[i].GetComponent<BlueBirdScript>().aliO = true;

			blues[i].GetComponent<BlueBirdScript>().sepW = 40;
			blues[i].GetComponent<BlueBirdScript>().cohW = 2;
			blues[i].GetComponent<BlueBirdScript>().aliW = 5;

			blues[i].GetComponent<BlueBirdScript>().wanO = true;
			blues[i].GetComponent<BlueBirdScript>().wanW = 1;

		}

		
		for (int i = 0; i < bluesNum; ++i) {
			//	set each bird in the flock to 'see' its neighbours
			blues[i].GetComponent<BlueBirdScript>().updateNeighbours();
		}

		//	camera mode, this is centred on the flock
		mode = 5;

		//	initialise rotation variables (explained above)
		rotaty = 0F;
		rotAm = 0.5F;

		//	initialise the timer and scene counter
		timer = 0;
		sceneCount = 0;

		
	}
	
	void Update () {


		//	Camera Modes

		//	mode 0
		//		head on view of falcon
		if (mode == 0) {
			//	calculate position based on falcon and offset
			offset = new Vector3(0, 0, 1.5F);
			Vector3 calcPos = falcon.transform.position + offset;
			transform.position = calcPos;
			//	calculate direction for camera to point in
			direction = falcon.transform.position - transform.position;
			transform.forward = direction;

		} else if (mode == 1) {
			//	mode 1
			//		side view of falcon

			offset = new Vector3(1.5F, 0, 0);
			Vector3 calcPos = falcon.transform.position + offset;
			transform.position = calcPos;

			direction = falcon.transform.position - transform.position;
			transform.forward = direction;

		} else if (mode == 2) {
			//	mode 2
			//		top view of falcon
			offset = new Vector3(0, 1F, 0);
			Vector3 calcPos = falcon.transform.position + offset;
			transform.position = calcPos;
			
			direction = falcon.transform.position - transform.position;
			transform.forward = direction;


		} else if (mode == 3) {
			//	mode 3
			//		back view of falcon
			//		used to view the caught bluebird
			offset = new Vector3(0, -0.5F, -1F);
			Vector3 calcPos = falcon.transform.position + offset;
			transform.position = calcPos;

			direction = new Vector3(0, 0, 1F);
			transform.forward = direction;


		} else if (mode == 4) {
			//	mode 4
			//		circle view of flock
			//		camera points at centre of flock and slowly circles it
			Vector3 flockCentre = Vector3.zero;
			for (int i = 0; i < bluesNum; ++i) {
				flockCentre += blues[i].transform.position;
			}
			flockCentre /= bluesNum;
			//	optDist is the distance from the camera to
			//		the centre of the flock
			float optDist = 7;
			//	this is the camera's position's rotation around the 
			//		flock centre, the variable "rotaty" increments on
			//		each update
			Quaternion q = Quaternion.AngleAxis(rotaty, Vector3.up);
			//	targLook is the target direction to look in
			//		targLookOrig is the initial direction the camera was facing
			//		the cumulitive rotation is applied to the original direction
			Vector3 targLook = q * targLookOrig;
			targLook.Normalize();
			if (rotaty >= 360F) {
				rotaty = 0F;
			}
			rotaty += rotAm;
			transform.position = flockCentre + (targLook * optDist);
			transform.forward = (flockCentre - transform.position).normalized;
		} else if (mode == 5) {
			//	mode 5
			//		circle view of single bird in flock
			//	same principles of mode 4
			Vector3 flockCentre = Vector3.zero;
			flockCentre += blues[0].transform.position;
			float optDist = 7;
			Quaternion q = Quaternion.AngleAxis(rotaty, Vector3.up);
			Vector3 targLook = q * targLookOrig;
			targLook.Normalize();
			if (rotaty >= 360F) {
				rotaty = 0F;
			}
			rotaty += rotAm;
			transform.position = flockCentre + (targLook * optDist);
			if (flockCentre.y <= 5) {
				transform.position = (flockCentre + (targLook * optDist)) + new Vector3(0, 20, 0);
			}
			transform.forward = (flockCentre - transform.position).normalized;
		}


		//	increment timer
		timer += Time.deltaTime;

		if (timer >= 5) {
			//	change scene approximately every 5 seconds
			timer = 0;	//	reset timer so variable doesn't go out of bounds
			sceneCount++;
		} else if ((sceneCount == 4) && (timer >= 3)) {
			timer = 0;
			sceneCount++;
		} else if ((sceneCount == 7) && (timer >= 2)) {
			timer = 0;
			sceneCount++;
		}



		//	Scenes
		//		this area describes the scenes in terms of
		//			camera mode/view and the bird's behaviours

		if (sceneCount == 0) {
			//	First scene open on the flock
			//	stop the falcon from moving
			falcon.GetComponent<FalconScript>().go = false;
			//	point camera at flock
			mode = 4;
		} else if (sceneCount == 1) {
			//	Second scene
			//		View of the falcon head on
			//	set the falcon to move and to follow the path
			falcon.GetComponent<FalconScript>().go = true;
			falcon.GetComponent<FalconScript>().pathO = true;
			falcon.GetComponent<FalconScript>().seekO = false;
			//	camera pointed head on at falcon
			mode = 0;
		} else if (sceneCount == 2) {
			//	Third scene
			//		View of the flock
			//	freeze the falcon
			falcon.GetComponent<FalconScript>().go = false;
			//	focus on the flock
			mode = 4;
		} else if (sceneCount == 3) {
			//	Fourth scene
			//		Side view of falcon
			//	set the falcon to move
			falcon.GetComponent<FalconScript>().go = true;
			//	point camera side long at falcon
			mode = 1;
		} else if (sceneCount == 4) {
			//	Fifth scene
			//		one bluebird splits off from group
			//		camera focuses on it
			//	freeze falcon
			falcon.GetComponent<FalconScript>().go = false;
			//	point camera at single bird flying outside of flock
			mode = 5;
			//	set the single bluebird to seek to point set below
			blues[0].GetComponent<BlueBirdScript>().seekO = true;
			blues[0].GetComponent<BlueBirdScript>().seekW = 1;
			blues[0].GetComponent<BlueBirdScript>().seekTarg = new Vector3(50, 20, 0);
			//	turn off other forces on the bird
			blues[0].GetComponent<BlueBirdScript>().roostO = false;
			blues[0].GetComponent<BlueBirdScript>().wanO = false;
			blues[0].GetComponent<BlueBirdScript>().sepO = false;
			blues[0].GetComponent<BlueBirdScript>().aliO = false;
			blues[0].GetComponent<BlueBirdScript>().cohO = false;

			//	this section was an attempt to prevent that second bird from
			//		splitting off from the group with blues[0]
			if (miscCount == 1) {
				miscCount = 0;
				for (int i = 1; i < bluesNum; ++i) {
					blues[i].GetComponent<BlueBirdScript>().seekO = false;
					blues[i].GetComponent<BlueBirdScript>().roostO = true;
					blues[i].GetComponent<BlueBirdScript>().sepO = true;
					blues[i].GetComponent<BlueBirdScript>().cohO = true;
					blues[i].GetComponent<BlueBirdScript>().aliO = true;
					blues[i].GetComponent<BlueBirdScript>().wanO = true;
				}
			}

		} else if (sceneCount == 5) {
			//	Sixth scene
			//		View of single bird split off
			falcon.GetComponent<FalconScript>().go = false;
			//	point at small bird
			mode = 5;
		} else if (sceneCount == 6) {
			//	Seventh scene
			//		View through falcon legs of single bird and
			//		falcon catching the single bird
			falcon.GetComponent<FalconScript>().go = true;
			//	follow line of sight of falcon from behind
			mode = 3;
			//	set the falcon to seek the bluebird
			falcon.GetComponent<FalconScript>().seekTarg = blues[0].transform.position;

		} else if (sceneCount == 7) {
			//	Eighth scene
			//		Side view of falcon with little bird in talons
			falcon.GetComponent<FalconScript>().go = true;
			mode = 1;

		} else if (sceneCount >= 8) {
			//	Ninth scene
			//		View of whole flock of bluebirds
			falcon.GetComponent<FalconScript>().go = true;
			mode = 4;
			//	set bluebirds to seek falcon as well as flock
			for (int i = 1; i < bluesNum; ++i) {
				blues[i].GetComponent<BlueBirdScript>().seekO = true;
				blues[i].GetComponent<BlueBirdScript>().seekW = 50;
				blues[i].GetComponent<BlueBirdScript>().seekTarg = falcon.transform.position;
				blues[i].GetComponent<BlueBirdScript>().roostO = true;
				blues[i].GetComponent<BlueBirdScript>().sepO = true;
				blues[i].GetComponent<BlueBirdScript>().cohO = true;
				blues[i].GetComponent<BlueBirdScript>().aliO = true;
				blues[i].GetComponent<BlueBirdScript>().wanO = true;
			}
		}


		//	covers time when single bird splits off and falcon catches it
		if ((sceneCount >= 4) && (sceneCount <= 7)) {
			if ((falcon.GetComponent<FalconScript>().transform.position
			     - blues[0].GetComponent<BlueBirdScript>().transform.position).magnitude < 3) {
				//	when falcon gets 3 units close to little bird, little bird is caught
				blueCaught = true;
				//	falcon then seeks to new point
				falcon.GetComponent<FalconScript>().pathO = false;
				falcon.GetComponent<FalconScript>().seekO = true;
				falcon.GetComponent<FalconScript>().seekTarg = new Vector3(50, 20, -10);
			}


			if (blueCaught) {
				//	set blubird to stay in falcon talons
				blues[0].GetComponent<BlueBirdScript>().transform.position = falcon.GetComponent<FalconScript>().transform.position - new Vector3(0, 0.4F, 0.01F);
				//	set falcon to seek to new position
				falcon.GetComponent<FalconScript>().pathO = false;
				falcon.GetComponent<FalconScript>().seekO = true;
				falcon.GetComponent<FalconScript>().seekTarg = new Vector3(50, 20, 50);
			}
		}

		//	this is when and after the flock attacks the falcon causing
		//		it to fall to the ground
		if (sceneCount >= 8) {
			//	find centre of the flock
			Vector3 flockCentre = Vector3.zero;
			for (int i = 0; i < bluesNum; ++i) {
				flockCentre += blues[i].transform.position;
			}
			flockCentre /= bluesNum;
			//	when the flock centre is closer than 3 units to the falcon, 
			//		the falcon seeks towards the ground
			if ((falcon.GetComponent<FalconScript>().transform.position - flockCentre).magnitude < 3) {

				//	the plane is set around 6 units up
				falcon.GetComponent<FalconScript>().seekTarg = new Vector3(50, 5.1F, 50);
				//	higher the max speed to simulate falling and the seek weigthing
				falcon.GetComponent<FalconScript>().maxSpeed = 20;
				falcon.GetComponent<FalconScript>().seekW = 20;
				//	point the falcon at the ground
				falcon.GetComponent<FalconScript>().transform.forward = new Vector3(0, -1, 0);
			}
		}


	}
}
