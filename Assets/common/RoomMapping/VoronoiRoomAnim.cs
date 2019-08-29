using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoronoiRoomAnim : MonoBehaviour
{
    public float speed = 0.2f;
    private Material mat;
    private float distanceOffset = 1.15f;
    private float distance;
    private Util.Timer distanceTimer;

    // Start is called before the first frame update
    void Start()
    {
        MeshRenderer renderer = this.GetComponent<MeshRenderer>();
        this.mat = renderer.material;
        this.distance = this.distanceOffset;
        this.distanceTimer = new Util.Timer(2.5f);

        Vector3 objCenter = renderer.bounds.center;
        objCenter.y -= renderer.bounds.size.y / 2.0f;
        mat.SetVector("_Center", objCenter);
    }

    /*
    void Update()
    {
        if (this.distanceTimer.OnTime()) {
            this.distance = 0;
        }

        //アニメーション中のとき(待ちタイマーが始まってないとき)
        if (! this.distanceTimer.isStarted) {
            if (this.distance-this.distanceOffset < 2 * Mathf.PI) {
                mat.SetFloat("_Distance", this.distance);
                this.distance+=speed;
            }
            else {
                this.distanceTimer.Start();
            }
        }

        this.distanceTimer.Clock();
    }
    */

    void Update()
    {
        mat.SetFloat("_Distance", this.distance);
        this.distance+=speed;
    }
}
