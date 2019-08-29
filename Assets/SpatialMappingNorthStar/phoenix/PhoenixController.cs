using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Klak.Math;
using Leap;
using Leap.Unity;

enum State {
	START_FLYING, EXPLORING, END_FLYING,
};

public class PhoenixController : MonoBehaviour
{
    public float _noiseFrequency = 0.3f;
    public int _randomSeed = 123;
    public float _stepFrequency = 3.0f;
    public float _obstacle_detecting_distance = 0.3f;
    public Transform player;
    public GameObject leapProviderObj;

    private bool exploring_on;
	private float exploring_start_time;
	private float exploring_ending_time;
	private float FLYOUT_END_TIME = 0.4f; //seconds
	private float FLYBACK_END_TIME = 0.4f; //seconds
	private Vector3 tmpScale;
	private Quaternion tmpRotation;
    private LeapServiceProvider m_Provider;
    private HandUtil handUtil;

    Animator anim;
    NoiseGenerator _noise;
    XXHash _hash;
	GameObject bodyObj;
    float turning_rate;

	public class ObstacleAvoidInfo {
		public Vector3 dir;
		public float turning_rate;
		public bool is_hit;
		public ObstacleAvoidInfo() {
			this.dir = Vector3.zero;
			this.turning_rate = 0.0f;
			this.is_hit = false;
		}
	};

    // Start is called before the first frame update
    void Start()
    {
        this.anim = this.GetComponent<Animator>();
		this.anim.enabled = false;
        this._hash = new XXHash(_randomSeed);
        this._noise = new NoiseGenerator(_randomSeed, _noiseFrequency);

		this.tmpScale = this.transform.localScale;
		this.tmpRotation = this.transform.rotation;
		this.transform.localScale = Vector3.zero;
		this.bodyObj = GameObject.Find(this.name + "/phoenix");
        this.turning_rate = 0.0f;
		this.exploring_start_time = 0.0f;
		this.exploring_ending_time = 0.0f;
		this.exploring_on = false;
		this.FLYOUT_END_TIME = 0.5f;

        this.m_Provider = this.leapProviderObj.GetComponent<LeapServiceProvider>();
        this.handUtil = new HandUtil(player);

        //this.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        Debug.Log(this.transform.localScale);
    }

    // Update is called once per frame
    void Update()
    {
		this.Fly();
    }

	public void Fly()
	{
        Frame frame = this.m_Provider.CurrentFrame;
        Hand[] hands = HandUtil.GetCorrectHands(frame); //0=LEFT, 1=RIGHT

        Hand leftHand = hands[HandUtil.LEFT];
        Hand rightHand = hands[HandUtil.RIGHT];
        bool isJustOpenedLeftHand = this.handUtil.JustOpenedHandOn(hands, HandUtil.LEFT);
        bool isJustOpenedRightHand = this.handUtil.JustOpenedHandOn(hands, HandUtil.RIGHT);

        // Switch for flying
        if (isJustOpenedLeftHand && !exploring_on) {
            //Start action
            this.exploring_on = true;
            this.anim.enabled = true;
            this.exploring_start_time = Time.time;
            this.transform.position = HandUtil.ToVector3(leftHand.PalmPosition);
            //HandUtil.ToVector3(leftHand.PalmPosition);
        }
        else if (isJustOpenedRightHand && exploring_on) {
            //End action
            this.exploring_on = false;
            this.anim.enabled = false;
            this.exploring_ending_time = Time.time;
        }

        if (this.exploring_on) {
            if ((Time.time - this.exploring_start_time) < this.FLYOUT_END_TIME) {
                this.StartFlying(leftHand);
            }
			else {
				this.Explore(); 
			}
		}
		else {
			this.EndFlying();
		}
	}


	public void StartFlying(Hand hand)
    {
        this.anim.speed = 0.5f;
        this.anim.Play("Explore");

		float delta = this._stepFrequency * Time.deltaTime;
		this.transform.position += 1.2f * HandUtil.ToVector3(hand.PalmNormal) * delta; //bit faster
		float flyout_time = (Time.time - this.exploring_start_time);
        float remapped_flyout_time = Util.EaseIn(Util.Remap(flyout_time, 0, this.FLYOUT_END_TIME, 0, 1));

		//
        // Scale small to big.
        //
		this.transform.localScale = Vector3.Slerp( this.tmpScale * 0.1f, this.tmpScale, remapped_flyout_time);

        //
        // Rotate 360 degree.
		// Quaternion.Slerp works only until 180 degrees. So it is separated.
        //
		if (remapped_flyout_time < 0.5f) {
            this.transform.rotation = Quaternion.Slerp(
				this.tmpRotation,
				this.tmpRotation * Quaternion.Euler(0, 0, 180),
				remapped_flyout_time);
		}
		else {
			this.transform.rotation = Quaternion.Slerp(
				this.tmpRotation * Quaternion.Euler(0, 0, 180),
                this.tmpRotation * Quaternion.Euler(0, 0, 360),
                remapped_flyout_time);
		}
    }

	public void EndFlying()
	{
	}

	public void Explore()
	{
		this.anim.speed = 0.8f;
		this.anim.Play("Explore");
		this._noise.Frequency = _noiseFrequency;
        this._noise.Step();
        Quaternion current_rot = this.transform.rotation;

        ObstacleAvoidInfo obs_info = this.GetObstacleAvoidInfo();
        //Vector3 ex_pos = this.transform.position;

        float delta = this._stepFrequency * Time.deltaTime;
        Quaternion target_rot;
        if (obs_info.is_hit)
        {
            target_rot = Quaternion.LookRotation(obs_info.dir);
            this.transform.rotation = Quaternion.Slerp(current_rot, target_rot, obs_info.turning_rate);
        }
        else
        {
            target_rot = this._noise.Rotation(0, 90f, 360f, 90f);
            this.transform.rotation = Quaternion.Slerp(current_rot, target_rot, delta);
        }
        this.transform.position += 0.8f * this.transform.forward * delta;

        //Debug.DrawLine(ex_pos, this.transform.position, Color.blue);
        //Debug.Log("rot: " + this.transform.rotation.eulerAngles);
	}

	private ObstacleAvoidInfo GetObstacleAvoidInfo()
	{
		ObstacleAvoidInfo obs_info = new ObstacleAvoidInfo();
		obs_info.dir = Vector3.zero;
		RaycastHit hit;
        float hit_distance = float.MaxValue;

        if (Physics.Raycast(this.transform.position, this.transform.forward,
                    out hit, this._obstacle_detecting_distance)) {
            if (hit.transform != transform) {
                Debug.DrawLine(this.transform.position, hit.point, Color.red);
				obs_info.is_hit = true;
				obs_info.dir = hit.normal;
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
				obs_info.is_hit = true;
				obs_info.dir = hit.normal;
                hit_distance = Mathf.Min(hit_distance, Vector3.Distance(this.transform.position, hit.point));
            }
        }
        //Right
        if (Physics.Raycast(rightR, this.transform.forward, out hit,
                    this._obstacle_detecting_distance)) {
            if (hit.transform != transform) {
                Debug.DrawLine(rightR, hit.point, Color.red);
				obs_info.is_hit = true;
				obs_info.dir = hit.normal;
                hit_distance = Mathf.Min(hit_distance, Vector3.Distance(this.transform.position, hit.point));
            }
        }
        if (0 < hit_distance && hit_distance < this._obstacle_detecting_distance) {
            //Has obstacle objects in the distance
			obs_info.turning_rate = Util.Remap(hit_distance,
                       0, this._obstacle_detecting_distance, 1, 0); //0~obs -> 1~0
			//Debug.Log("d: " + hit_distance + ", obstacle_dist: " + this._obstacle_detecting_distance + ", turning_rate: " + turning_rate);
        }
        else {
            //No obstacle objects
			obs_info.turning_rate = Time.deltaTime;
        }

		return obs_info;
	}

}
