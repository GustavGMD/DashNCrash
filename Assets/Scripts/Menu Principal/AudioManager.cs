using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AudioManager : MonoBehaviour
{
    public enum SFXType
    {
        COLLISION_NORMAL,
        COLLISION_DAMAGE,
        COLLISION_DEATH,
        POWER_UP_COLLECT,
        POWER_UP_SPAWN,
        POWER_UP_SHIELD,
        SLOW_AREA,
        SLIDE_AREA,
        DASH_CHARGE,
        DASH_RELEASE,
        COUNTDOWN,
        GAME_START,
        MENU_BUTTON,
        LOBBY_NAME_CHANGE,
        LOBBY_COLOR_CHANGE
    }

    public GameObject bgmMenu;
    public GameObject bgmLobby;
    public GameObject bgmGame;

    //SFX POOL
    #region SFX
    public List<List<GameObject>> sfxAvailable;
    public List<List<GameObject>> sfxActive;
    #endregion

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);

        sfxAvailable = new List<List<GameObject>>();
        sfxActive = new List<List<GameObject>>();
        for (int i = 0; i < Enum.GetValues(typeof(SFXType)).Length; i++)
        {
            sfxAvailable.Add(new List<GameObject>());
            sfxActive.Add(new List<GameObject>());
        }

        for (int i = 0; i < Enum.GetValues(typeof(SFXType)).Length; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                int __num = i;
                GameObject __temp = (GameObject)Instantiate(Resources.Load("Audio/" + ((SFXType)i)), Vector3.zero, Quaternion.identity);
                __temp.SetActive(false);
                __temp.transform.parent = gameObject.transform;
                __temp.GetComponent<AudioObject>().onFinishedPlaying += delegate
                {
                    OnFinishedPlaying((SFXType)__num, __temp);
                };
                sfxAvailable[i].Add(__temp);
            }
        }

        UpdateBGM();
    }

    public void UpdateBGM()
    {
        bgmMenu.SetActive(Application.loadedLevel == 0);
        bgmLobby.SetActive(Application.loadedLevel == 1);
        bgmGame.SetActive(Application.loadedLevel == 2);        
    }

    void OnLevelWasLoaded(int level)
    {
        UpdateBGM();
    }

    public void PlaySound(SFXType p_type)
    {
        sfxAvailable[(int)p_type][0].SetActive(true);
        sfxActive[(int)p_type].Add(sfxAvailable[(int)p_type][0]);
        sfxAvailable[(int)p_type].RemoveAt(0);
    }

    public void PlayPitchedSound(SFXType p_type, float p_pitch)
    {
        sfxAvailable[(int)p_type][0].SetActive(true);
        sfxAvailable[(int)p_type][0].GetComponent<AudioSource>().pitch = p_pitch;
        sfxActive[(int)p_type].Add(sfxAvailable[(int)p_type][0]);
        sfxAvailable[(int)p_type].RemoveAt(0);
    }

    public void OnFinishedPlaying(SFXType p_type, GameObject p_object)
    {
        p_object.SetActive(false);
        sfxActive[(int)p_type].Remove(p_object);
        sfxAvailable[(int)p_type].Add(p_object);
    }
}