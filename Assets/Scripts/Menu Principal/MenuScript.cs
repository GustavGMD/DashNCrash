using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour {

    public AudioManager audioManager;
    public Button[] buttons;
    

	// Use this for initialization
	void Start ()
    {
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].onClick.AddListener(delegate
            {
                audioManager.PlaySound(AudioManager.SFXType.MENU_BUTTON);
            });
        }
    }
    

    public void OnStartGame()
    {        
        Application.LoadLevel(1);
    }

    public void OnQuitGame()
    {
        Application.Quit();
    }
}
