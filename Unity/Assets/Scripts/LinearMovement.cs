using UnityEngine;
using System.Collections;

public class LinearMovement : MonoBehaviour {
	float vel = 0.0f;
	Vector3 pos;
	// Use this for initialization
	void Start () {
		pos = new Vector3();
	}
	
	// Update is called once per frame
	void Update () {
		pos = transform.position;
		pos.x += vel * Time.deltaTime;
		transform.position = pos;
		vel *= 0.95f;

		if(Mathf.Abs(vel) < 0.01f) vel = 0.0f;
	}

	public void AddImpulse(float impulse) {
		vel += impulse;
	}
}
