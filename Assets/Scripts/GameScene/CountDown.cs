using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.Networking;

public class CountDown : NetworkBehaviour {

    public event Action<bool> onGameStart;

    public Text countDisplay;    
    public float timer;
    [SyncVar]
    private float _currentTime = 0;

    public AudioManager audioManager;
	
    public void Start()
    {
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
    }
	// Update is called once per frame
    public override void OnStartClient()
    {
        base.OnStartClient();
        countDisplay.text = ((int)(timer - _currentTime)).ToString();
        for (int i = 0; i < GameObject.FindObjectsOfType<PlayerMovement>().Length; i++)
        {
            onGameStart += GameObject.FindObjectsOfType<PlayerMovement>()[i].EnableInput;
        }
        
    }

	void Update () {
        if (_currentTime < timer)
        {
            //Debug.Log("entered first if");
            _currentTime += Time.deltaTime;
            int __check;
            if (Int32.TryParse(countDisplay.text, out __check)) {
                //Debug.Log("parsed");
                if (__check != (int)(timer - _currentTime))
                {
                    if (((int)(timer - _currentTime) != 0))
                    {
                        countDisplay.text = ((int)(timer - _currentTime)).ToString();
                        audioManager.PlaySound(AudioManager.SFXType.COUNTDOWN);
                    }
                    else
                    {
                        countDisplay.text = "Go!";
                        audioManager.PlaySound(AudioManager.SFXType.GAME_START);
                    }
                }
            }
        }
        else
        {
            if (onGameStart != null) onGameStart(true);
            gameObject.SetActive(false);
        }
	}
}
