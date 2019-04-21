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
    public float _stepFrequency = 2.0f;

    Animator anim;
    NoiseGenerator _noise;
    XXHash _hash;

    // Start is called before the first frame update
    void Start()
    {
        this.anim = this.GetComponent<Animator>();
        this.anim.speed = 0.8f;
        this.anim.Play("Take 001");
        this._hash = new XXHash(_randomSeed);
        this._noise = new NoiseGenerator(_randomSeed, _noiseFrequency);
    }

    // Update is called once per frame
    void Update()
    {
        this._noise.Frequency = _noiseFrequency;
        this._noise.Step();
        var delta = this._stepFrequency * Time.deltaTime;

        RaycastHit hit;
        Vector3 obstacle_dir = Vector3.zero;
        if (Physics.Raycast(this.transform.position, this.transform.forward, out hit, 3) ) {
            if (hit.transform != transform) {
                Debug.DrawLine(this.transform.position, hit.point, Color.red);
                obstacle_dir = hit.normal * 5;
            }

        }

        //Debug.Log("new pos: " + newPos);
        this.transform.rotation = this._noise.Rotation(0, 90f, 360f, 90f) * Quaternion.LookRotation(obstacle_dir);
        this.transform.position += 2 * this.transform.forward* delta;

        //Debug.Log("rot: " + this.transform.rotation.eulerAngles);
    }

}
