using UnityEngine;
using UnityEditor;
using System.Collections;

public class ClearPlayerPrefs : MonoBehaviour 
{
	[MenuItem("Tools/ClearPlayerPrefs")] 
	static void DeleteMyPlayerPrefs() 
	{ 
		PlayerPrefs.DeleteAll();
	} 
}
