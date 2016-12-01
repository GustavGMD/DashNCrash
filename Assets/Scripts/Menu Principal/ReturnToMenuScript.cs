using UnityEngine;
using System.Collections;

public class ReturnToMenuScript : MonoBehaviour {

	Rect button1;

	// Use this for initialization
	void Start () {
		button1 = new Rect (Screen.width*7 / 8 - 50, Screen.height*7 / 8 - 10, 100, 20);
	}

	void OnGUI(){
		if (GUI.Button (button1, "Back to Menu")) {
			Application.LoadLevel(0);
		}
	}
}
