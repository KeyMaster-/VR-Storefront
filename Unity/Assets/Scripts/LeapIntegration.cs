using UnityEngine;
using System.Collections;
using Leap;

public class LeapIntegration : MonoBehaviour {
	Controller cont;
	float gestureTimeout = 0.0f;
	public GameObject obj;
 	Rigidbody rb;
 	

 	public ObjectDisplay disp;
	// Use this for initialization
	void Start () {
		cont = new Controller();
		cont.EnableGesture(Gesture.GestureType.TYPE_SWIPE, true);
		cont.EnableGesture(Gesture.GestureType.TYPE_CIRCLE, true);
	}
	
	// Update is called once per frame
	void Update () {
		gestureTimeout -= Time.deltaTime;
		if(gestureTimeout > 0.0f) return;
		Frame frame = cont.Frame();
		GestureList gestures = frame.Gestures();
		foreach(Gesture gesture in gestures) {
			if(gesture.Type == Gesture.GestureType.TYPE_CIRCLE) {
				CircleGesture circle = new CircleGesture(gesture);
				if(testCircle(circle)) {
					gestureTimeout = 0.5f;
					break;
				}
			}
			else if(gesture.State == Gesture.GestureState.STATE_STOP) {
				SwipeGesture swipe = new SwipeGesture(gesture);
				if(testGesture(swipe)) {
					gestureTimeout = 0.5f;
					break;
				}
			}
		}
	}

	bool testCircle(CircleGesture circle) {
		if(circle.DurationSeconds >= 0.5f) {
			disp.circle(circle.DurationSeconds);
			return true;
		}
		return false;
	}

	bool testGesture(SwipeGesture swipe) {
		
		float angle = swipe.Direction.AngleTo(Vector.Left);
		if(angle < 0.8) {
			print("left");
			disp.swipe(Direction.Left, swipe.Speed);
			return true;
		}

		angle = swipe.Direction.AngleTo(Vector.Right);
		if(angle < 0.8) {
			print("right");
			disp.swipe(Direction.Right, swipe.Speed);
			return true;
		}

		angle = swipe.Direction.AngleTo(Vector.Up);
		if(angle < 0.8) {
			print("up");
			disp.swipe(Direction.Up, swipe.Speed);
			return true;
		}
		angle = swipe.Direction.AngleTo(Vector.Down);
		if(angle < 0.8) {
			print("down");
			disp.swipe(Direction.Down, swipe.Speed);
			return true;
		}
		angle = swipe.Direction.AngleTo(Vector.Forward);
		if(angle < 0.8) {
			print("forward");
			disp.swipe(Direction.Forward, swipe.Speed);
			return true;
		}
		angle = swipe.Direction.AngleTo(Vector.Backward);
		if(angle < 0.8) {
			print("backward");
			disp.swipe(Direction.Backward, swipe.Speed);
			return true;
		}
		return false;
	}
}

public enum Direction {
	Left,
	Right,
	Up,
	Down,
	Forward,
	Backward
}