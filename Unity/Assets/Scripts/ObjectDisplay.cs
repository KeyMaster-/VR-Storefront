using UnityEngine;
using System.Collections;
using SimpleJSON;
using UnityEngine.UI;
public class ObjectDisplay : MonoBehaviour {
	public TextAsset object_info_json;
	JSONArray object_info;
	public float rotationSpeedScaling = 0.01f;
	public float scrollingSpeedScaling = 0.05f;
	public float middleElevation = 0.3f;
	public float spacing = 1.8f;

	public GameObject labelPrefab;
	public GameObject confirmDialog;
	public GameObject purchaseCompletedDialog;

	JSONNode parsed;
	GameObject[] objects;
	GameObject[] labels;

	Rigidbody frontBody;

	bool inBuyDialog = false;

	float firstCircleDuration = 0.0f;

	int middleObjectIndex = 0;

	// Use this for initialization
	void Start () {
		parsed = JSON.Parse(object_info_json.text);
		object_info = parsed["objects"] as JSONArray;
		objects = new GameObject[object_info.Count];
		labels = new GameObject[object_info.Count];

		
		float offset = -((object_info.Count - 1) / 2.0f) * spacing;

		if(object_info.Count % 2 == 0) offset -= spacing / 2.0f;
		middleObjectIndex = object_info.Count / 2;

		Vector3 inst_pos = new Vector3(0,0,0);
		for(int i=0; i<object_info.Count; i++) {
			inst_pos.x = offset + spacing * i;
			inst_pos.y = 0.0f;
			if(inst_pos.x == 0.0f) {
				inst_pos.y = middleElevation;
			}

			labels[i] = Instantiate(labelPrefab, inst_pos, Quaternion.identity) as GameObject;
			labels[i].transform.parent = transform;
			labels[i].transform.localPosition = inst_pos;
			GameObject textObj = labels[i].transform.Find("Canvas/Text").gameObject;
			Text text = textObj.GetComponent<Text>();
			text.text = text.text.Replace("_x", object_info[i]["price"]);

			objects[i] = Instantiate(Resources.Load("models/" + object_info[i]["model_name"]) as GameObject, inst_pos, Quaternion.identity) as GameObject;
			objects[i].transform.parent = transform;
			objects[i].transform.localPosition = inst_pos;
			Rigidbody rb = objects[i].AddComponent<Rigidbody>();
			rb.useGravity = false;
			rb.angularDrag = 0.2f;
		}
	}
	
	void moveObjectsLeft() {
		if(middleObjectIndex == object_info.Count - 1) return;
		Vector3 pos;
		for(int i = 0; i < objects.Length; i++) {
			pos = objects[i].transform.localPosition;
			pos.x -= spacing;
			pos.y = 0.0f;
			if(Mathf.Abs(pos.x) <= 0.01f) {
				pos.y = middleElevation;
			}
			StartCoroutine(MoveToPosition(objects[i], pos, 0.5f));
			StartCoroutine(MoveToPosition(labels[i], pos, 0.5f));
		}
		middleObjectIndex++;
	}

	void moveObjectsRight() {
		if(middleObjectIndex == 0) return;
		// LinearMovement mov;
		Vector3 pos;
		for(int i = 0; i < objects.Length; i++) {
			pos = objects[i].transform.localPosition;
			pos.x += spacing;
			pos.y = 0.0f;
			if(Mathf.Abs(pos.x) <= 0.01f) {
				pos.y = middleElevation;
			}
			StartCoroutine(MoveToPosition(objects[i], pos, 0.5f));
			StartCoroutine(MoveToPosition(labels[i], pos, 0.5f));
		}
		middleObjectIndex--;
	}

	IEnumerator MoveToPosition(GameObject obj, Vector3 newPosition, float time) {
		float elapsedTime = 0;
		Vector3 startingPos = obj.transform.localPosition;
		while (elapsedTime < time) {
			float t = elapsedTime / time;
			t = t*t*t * (t * (6f*t - 15f) + 10f);
			obj.transform.localPosition = Vector3.Lerp(startingPos, newPosition, t);
 			elapsedTime += Time.deltaTime;
			yield return null;
		}
	}

	GameObject curDialog;
	public void circle(float duration) {
		if(frontBody == null) return;
		if(!inBuyDialog) {
			curDialog = Instantiate(confirmDialog);
			firstCircleDuration = duration;
			inBuyDialog = true;
		}
		else if(duration >= 1.0f + firstCircleDuration) {
			ServerAPI.makePaymentWithPrice(object_info[middleObjectIndex]["price"], object_info[middleObjectIndex]["description"]);
			inBuyDialog = false;
			firstCircleDuration = 0.0f;
			frontBody.gameObject.transform.rotation = Quaternion.identity;
			StartCoroutine(MoveToPosition(frontBody.gameObject, new Vector3(0.0f, middleElevation, 0.0f), 0.5f));
			frontBody = null;
			Destroy(curDialog);
			curDialog = Instantiate(purchaseCompletedDialog);
			Destroy(curDialog, 2.0f);
		}
	}

	public void swipe(Direction dir, float speed) {
		if(frontBody != null) {
			Vector3 angVel = frontBody.angularVelocity;
			angVel.x = 0;
			angVel.y = 0;
			frontBody.angularVelocity = angVel;
			switch(dir) {
				case Direction.Left:
					frontBody.AddTorque(Vector3.up * speed * rotationSpeedScaling, ForceMode.Impulse);
					break;

				case Direction.Right:
					frontBody.AddTorque(Vector3.down * speed * rotationSpeedScaling, ForceMode.Impulse);
					break;

				case Direction.Up:
					frontBody.AddTorque(Vector3.right * speed * rotationSpeedScaling, ForceMode.Impulse);
					break;

				case Direction.Down:
					frontBody.AddTorque(Vector3.left * speed * 4 * rotationSpeedScaling, ForceMode.Impulse);
					break;

				case Direction.Forward:
					frontBody.gameObject.transform.rotation = Quaternion.identity;
					StartCoroutine(MoveToPosition(frontBody.gameObject, new Vector3(0.0f, middleElevation, 0.0f), 0.5f));
					frontBody = null;
					if(inBuyDialog) {
						inBuyDialog = false;
						firstCircleDuration = 0.0f;
						Destroy(curDialog);
					}
					break;

				case Direction.Backward:
					frontBody.angularVelocity = Vector3.zero;
					break;
			}
		}
		else {
			switch(dir) {
				case Direction.Left:
						moveObjectsLeft();
					break;

				case Direction.Right:
						moveObjectsRight();
					break;
				case Direction.Backward:
					foreach(GameObject obj in objects) {
						if(Mathf.Abs(obj.transform.localPosition.x) < 0.01f) {
							StartCoroutine(MoveToPosition(obj, new Vector3(0.0f, 0.0f, -2.0f), 0.5f));
							frontBody = obj.GetComponent<Rigidbody>();
							break;
						}
					}
					break;
			}
		}
		
	}
}
