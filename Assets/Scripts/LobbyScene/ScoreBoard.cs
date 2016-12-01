using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ScoreBoard : NetworkBehaviour {

    /**/
    public Canvas scoreboard;
    //Form Server to LocalPlayer
    public Text[] boardNames;
    public Text[] boardScores;
    public Image[] boardBackgrounds;

    public string[] names;
    public int[] scores;
    public Color[] colors;

    //From LocalPlayer to Server
    public Slider colorSelector;    

    private myLobbyManager _lobbyManager;

    /**/
    public override void OnStartClient()
    {
        //base.OnStartClient();
        _lobbyManager = GameObject.FindObjectOfType<myLobbyManager>();
        Debug.Log(_lobbyManager);
        DontDestroyOnLoad(gameObject);
        colorSelector.onValueChanged.AddListener(OnSliderValueChanged);

        names = new string[4];
        scores = new int[4];
        colors = new Color[4];
    }
    /**/
    /**/
    [Command]
    public void Cmd_SetName(int p_connectionID, string p_name)
    {
        names[_lobbyManager.ConnectionToIndex(p_connectionID)] = p_name;
        Rpc_SetName(_lobbyManager.ConnectionToIndex(p_connectionID), p_name);
    }
    /**/
    [Command]
    public void Cmd_SetColor(int p_connectionID, Color p_color)
    {
        //Debug.Log(_lobbyManager.ConnectionToIndex(p_connectionID));
        colors[_lobbyManager.ConnectionToIndex(p_connectionID)] = p_color;
        Rpc_SetColor(_lobbyManager.ConnectionToIndex(p_connectionID), p_color);
    }
    /**/
    [ClientRpc]
    public void Rpc_SetName(int p_index, string p_name){
        names[p_index] = p_name;
        UpdateScoreBoard();
    }
    /**/
    [ClientRpc]
    public void Rpc_SetScore(int p_index, int p_score)
    {
        scores[p_index] = p_score;
        UpdateScoreBoard();
    }
    /**/
    [ClientRpc]
    public void Rpc_SetColor(int p_index, Color p_color)
    {
        colors[p_index] = p_color;
        UpdateScoreBoard();
    }
    /**/
    //called by LobbyManager on Server during Score Update
    public void SetScore(int p_connID, int p_score)
    {
        scores[_lobbyManager.ConnectionToIndex(p_connID)] = p_score;
        Rpc_SetScore(_lobbyManager.ConnectionToIndex(p_connID), p_score);
    }
    /**/
    public void UpdateScoreBoard()
    {
        for (int i = 0; i < 4; i++)
        {
            boardNames[i].text = names[i];
            boardScores[i].text = scores[i].ToString();
            boardBackgrounds[i].color = colors[i];
        }
    }
    /**/
    public void OnSliderValueChanged(float p_value){
        int __hue = (int)p_value;
        Color __color = HSVtoRGB(__hue, 1, 1);
        Debug.Log("changed color...");
        //Debug.Log(_lobbyManager);
        //Debug.Log(_lobbyManager.client);
        //Debug.Log(_lobbyManager.client.connection);
        //Debug.Log(_lobbyManager.client.connection.connectionId);        
        Cmd_SetColor(_lobbyManager.client.connection.connectionId, __color);
    }
    /**/
    public Color HSVtoRGB(int p_h, float p_s, float p_v)
    {
        float __r = 0, __g = 0, __b = 0;
        float __C, __X, __m;

        __C = p_v * p_s;
        __X = __C * (1 - Mathf.Abs(((float)p_h / 60) % 2 - 1));
        __m = p_v - __C;

        if (0 <= p_h && p_h < 60)
        {
            __r = __C;
            __g = __X;
            __b = 0;
        }
        else if (60 <= p_h && p_h < 120)
        {
            __r = __X;
            __g = __C;
            __b = 0;
        }
        else if (120 <= p_h && p_h < 180)
        {
            __r = 0;
            __g = __C;
            __b = __X;
        }
        else if (180 <= p_h && p_h < 240)
        {
            __r = 0;
            __g = __X;
            __b = __C;
        }
        else if (240 <= p_h && p_h < 300)
        {
            __r = __X;
            __g = 0;
            __b = __C;
        }
        else if (300 <= p_h && p_h <= 360)
        {
            __r = __C;
            __g = 0;
            __b = __X;
        }

        //Debug.Log(new Color(__r, __g, __b, __m));
        return new Color(__r + __m, __g + __m, __b + __m);
    }
    /**/
}
