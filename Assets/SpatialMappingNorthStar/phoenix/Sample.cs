using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sample : MonoBehaviour
{
	public GameObject target;

	// Update is called once per frame
	void Update()
	{
		Vector3 dir = Vector3.Normalize(this.target.transform.position - this.transform.position);
		RaycastHit hit;
		float distance = 10.0f;
		if (Physics.Raycast(this.transform.position, this.transform.forward, out hit, distance)) {
            if (hit.transform != transform) {
                Debug.DrawLine(this.transform.position, hit.point, Color.red);
                dir += hit.normal * 50;
            }
    	}
		Vector3 leftR = this.transform.position;
		Vector3 rightR = this.transform.position;
		leftR.x -= 1;
		rightR.x += 1;
        //Left
		if (Physics.Raycast(leftR, this.transform.forward, out hit, distance)) {
            if (hit.transform != transform) {
				Debug.DrawLine(leftR, hit.point, Color.red);
                dir += hit.normal * 50;
            }
        }
        //Right
		if (Physics.Raycast(rightR, this.transform.forward, out hit, distance)) {
            if (hit.transform != transform) {
                Debug.DrawLine(rightR, hit.point, Color.red);
                dir += hit.normal * 50;
            }
        }

		Quaternion rot = Quaternion.LookRotation(dir);
		this.transform.rotation = Quaternion.Slerp(this.transform.rotation, rot, Time.deltaTime);
		this.transform.position += this.transform.forward * 20 * Time.deltaTime;
    }
}
