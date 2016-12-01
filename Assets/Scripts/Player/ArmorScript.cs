using UnityEngine;
using System.Collections;

public class ArmorScript : MonoBehaviour {
	
	public int max_HP;
	public int curr_HP;	

	
	public void ApplyDamage(int damage){
		curr_HP -= damage;		
		GetComponent<SpriteRenderer> ().color = new Color ((((float)(max_HP - (curr_HP)) / (float)max_HP) * 2) < 1 ? (((float)(max_HP - (curr_HP)) / (float)max_HP)) * 2 : 1,
		                                                   ((float)curr_HP / (float)(max_HP / 2)) < 1 ? ((float)curr_HP / (float)(max_HP / 2)) : 1, 
		                                                   0);	
		if(curr_HP <= 0){
			gameObject.SetActive(false);
		}
	}
}
