using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Klak.Math;

public class PhoenixController : MonoBehaviour
{
    public float timeScale = 0.1f;
    public float positionScale = 1.0f;
    public float _noiseFrequency = 0.3f;
    public int _randomSeed = 123;
    public float _stepFrequency = 3.0f;
    public float _obstacle_detecting_distance = 0.5f;

    Animator anim;
    NoiseGenerator _noise;
    XXHash _hash;
	GameObject bodyObj;
    float turning_rate;

    // Start is called before the first frame update
    void Start()
    {
        this.anim = this.GetComponent<Animator>();
        this.anim.speed = 0.8f;
        this.anim.Play("Take 001");
        this._hash = new XXHash(_randomSeed);
        this._noise = new NoiseGenerator(_randomSeed, _noiseFrequency);

		this.bodyObj = GameObject.Find(this.name + "/phoenix");
        this.turning_rate = 0.0f;

		//Debug.Log("remap: 1.2f -> " + Util.Remap(1.2f, 0, 3, 1, 0));
		//Debug.Log("remap: 0.2f -> " + Util.Remap(0.2f, 0, 3, 1, 0));
		//Debug.Log("remap: 2.9f -> " + Util.Remap(2.9f, 0, 3, 1, 0));
		//Debug.Log("remap: 3.5f -> " + Util.Remap(3.5f, 0, 3, 1, 0));

    }

    // Update is called once per frame
    void Update()
    {
        this._noise.Frequency = _noiseFrequency;
        this._noise.Step();
		Quaternion current_rot = this.transform.rotation;

		Vector3 obstacle_dir = this.GetObstacleAvoidedDir();

		//Debug.Log("new pos: " + newPos);
		Quaternion target_rot = this._noise.Rotation(0, 90f, 360f, 90f) * Quaternion.LookRotation(obstacle_dir);
        float delta = this._stepFrequency * Time.deltaTime;
		this.transform.rotation = Quaternion.Slerp(current_rot, target_rot, this.turning_rate);
        this.transform.position += this.transform.forward * delta;

        //Debug.Log("rot: " + this.transform.rotation.eulerAngles);
    }

	private Vector3 GetObstacleAvoidedDir() {
		Vector3 obstacle_dir = Vector3.zero;
		RaycastHit hit;
        float hit_distance = float.MaxValue;

        if (Physics.Raycast(this.transform.position, this.transform.forward,
                    out hit, this._obstacle_detecting_distance)) {
            if (hit.transform != transform) {
                Debug.DrawLine(this.transform.position, hit.point, Color.red);
                obstacle_dir = hit.normal;
                hit_distance = Mathf.Min(hit_distance, Vector3.Distance(this.transform.position, hit.point));
            }
        }

		//Debug.Log("extends: " + this.bodyObj.GetComponent<Renderer>().bounds.extents);
		Vector3 leftR = this.transform.localPosition;
        Vector3 rightR = this.transform.localPosition;
		float wingDistance = this.bodyObj.GetComponent<Renderer>().bounds.extents.x;
		leftR.x -= wingDistance;
		rightR.x += wingDistance;
        //Left
        if (Physics.Raycast(leftR, this.transform.forward, out hit,
                    this._obstacle_detecting_distance)) {
            if (hit.transform != transform) {
                Debug.DrawLine(leftR, hit.point, Color.red);
				obstacle_dir += hit.normal;
                hit_distance = Mathf.Min(hit_distance, Vector3.Distance(this.transform.position, hit.point));
            }
        }
        //Right
        if (Physics.Raycast(rightR, this.transform.forward, out hit,
                    this._obstacle_detecting_distance)) {
            if (hit.transform != transform) {
                Debug.DrawLine(rightR, hit.point, Color.red);
				obstacle_dir += hit.normal;
                hit_distance = Mathf.Min(hit_distance, Vector3.Distance(this.transform.position, hit.point));
            }
        }
        if (0 < hit_distance && hit_distance < this._obstacle_detecting_distance) {
            //Has obstacle objects in the distance
			this.turning_rate = Util.Remap(hit_distance,
                       0, this._obstacle_detecting_distance, 1, 0);
			Debug.Log("d: " + hit_distance + ", obstacle_dist: " + this._obstacle_detecting_distance + ", turning_rate: " + turning_rate);
        }
        else {
            //No obstacle objects
            this.turning_rate = Time.deltaTime;
        }

		return obstacle_dir;
	}

}
