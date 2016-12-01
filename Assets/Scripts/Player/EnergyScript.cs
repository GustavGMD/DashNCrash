using UnityEngine;
using System.Collections;

public class EnergyScript : MonoBehaviour {

	void Start(){
		UpdateSprite (10, 10);
	}
	
	public void UpdateSprite(float max_HP, float curr_HP){
		
		transform.localScale = new Vector3 (1, (curr_HP / max_HP), 0);
		
		/**
		GetComponent<SpriteRenderer> ().color = new Color ((((max_HP - (curr_HP)) / max_HP) * 2) < 1 ? (((max_HP - (curr_HP)) / max_HP)) * 2 : 1,
		                                                   (curr_HP / (max_HP / 2)) < 1 ? curr_HP / (max_HP / 2) : 1, 
		                                                   0);	
		**/
	}
}
