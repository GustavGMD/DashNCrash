using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;


public class LobbyGUI : NetworkBehaviour {

    //GUI Objects
    public Canvas scoreboard;
    public Text[] boardNames;
    public Text[] boardScores;
    public Image[] boardBackgrounds;

    public Image playerSprite;
    public Image energySprite;
    public Image[] armorSprite;

    public Image[] smallPlayerSprite;
    public Image[][] smallArmorSprite;
    public Image[] smallArmorSprite1;
    public Image[] smallArmorSprite2;
    public Image[] smallArmorSprite3;
    public Image[] smallArmorSprite4;

    public Slider colorSelector;
    public Image sliderHandle;
    public InputField nameSelector;


    public string playerName;
    [SyncVar]
    public int score;
    public Color color;

    public static int __c1 = -60;
    public static int __c2 = 60;
    
    [SyncVar]
    public int _myIndex;

    private myLobbyManager _lobbyManager;

    public void Awake()
    {
        _lobbyManager = GameObject.FindObjectOfType<myLobbyManager>();
        //_lobbyManager.onSceneChange += OnSceneChange;
        scoreboard.worldCamera = Camera.main;

        smallArmorSprite = new Image[4][];
        smallArmorSprite[0] = smallArmorSprite1;
        smallArmorSprite[1] = smallArmorSprite2;
        smallArmorSprite[2] = smallArmorSprite3;
        smallArmorSprite[3] = smallArmorSprite4;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        //UpdateScoreBoard();
        //Rpc_UpdateScoreBoard();
    }

    public override void OnStartClient()
    {
        //base.OnStartClient(); 
        Debug.Log("start client");

        DontDestroyOnLoad(gameObject);
        //colorSelector = GameObject.Find("ColorSelector").GetComponent<Slider>();
        colorSelector.onValueChanged.AddListener(OnSliderValueChanged);
        nameSelector.onEndEdit.AddListener(OnInputFieldValueSet);
        //_myIndex = _lobbyManager.ConnectionToIndex(_lobbyManager.client.connection.connectionId);
        Cmd_SetIndex(_myIndex);

        for (int i = 0; i < 4; i++)
        {
            if (i == _myIndex)
            {
                boardNames[i].gameObject.SetActive(true);
                boardScores[i].gameObject.SetActive(true);
                boardBackgrounds[i].gameObject.SetActive(true);
                break;
            }
        }

        //(_myIndex == 0) colorSelector.gameObject.SetActive(false);

        if (hasAuthority) Debug.Log("has authority");
        else Debug.Log("does not have authority");
        //colorSelector.gameObject.SetActive(false);

        SetLocalInput(false);
        StartCoroutine(ActivateLocalInput());

        //if(_lobbyManager.ConnectionToIndex(_lobbyManager.client.connection.connectionId) == _myIndex)
        //{
        //    colorSelector.gameObject.SetActive(true);
        //}
        LobbyGUI[] __temp = GameObject.FindObjectsOfType<LobbyGUI>();
        for (int i = 0; i < __temp.Length; i++)
        {
            if(__temp[i] != this)
            {
                __temp[i].UpdateForNewClient();
            }
        }

        color = Color.red;
        UpdateScoreBoard(); 
    }

    IEnumerator ActivateLocalInput()
    {
        yield return new WaitForSeconds(1);
        //here we have all network variables set up, so it is safe to do network's variables-dependant operations
        if (hasAuthority)
        {
            SetLocalInput(true);
            _lobbyManager.localLobbyGUI = this;
        }
    }
    
    [Command]
    public void Cmd_SetIndex(int p_index)
    {
        _myIndex = p_index;
        Rpc_SetIndex(p_index);
    }

    [Command]
    public void Cmd_SetColor(Color p_color)
    {
        //Debug.Log(_lobbyManager.ConnectionToIndex(p_connectionID));
        color = p_color;
        Rpc_SetColor(p_color);
    }
    [Command]
    public void Cmd_SetScore(int p_score)
    {
        //Debug.Log(_lobbyManager.ConnectionToIndex(p_connectionID));
        score = p_score;
        Rpc_SetScore(p_score);
    }
    [Command]
    public void Cmd_SetName(string p_name)
    {
        playerName = p_name;
        Rpc_SetName(p_name);
    }

    [ClientRpc]
    public void Rpc_SetIndex(int p_index)
    {
        _myIndex = p_index;
    }

    [ClientRpc]
    public void Rpc_SetColor(Color p_color)
    {
        color = p_color;
        UpdateScoreBoard();
    }
    [ClientRpc]
    public void Rpc_SetScore(int p_score)
    {
        score = p_score;
        UpdateScoreBoard();
    }
    [ClientRpc]
    public void Rpc_SetName(string p_name)
    {
        playerName = p_name;
        UpdateScoreBoard();
    }
    
    public void UpdateScoreBoard()
    {

        float[] __colorHSV = RGBtoHSV(color);
        //int __c1 = 60, __c2 = -60;
        float __h1 = __colorHSV[0] + __c1;
        float __h2 = __colorHSV[0] + __c2;
        //Debug.Log("H1: " + __h1 + " H2: "+ __h2);
        Color __color1 = HSVtoRGB((int)__h1, __colorHSV[1], __colorHSV[2]);
        Color __color2 = HSVtoRGB((int)__h2, __colorHSV[1], __colorHSV[2]);

        boardNames[_myIndex].text = playerName;
        boardScores[_myIndex].text = score.ToString();
        boardBackgrounds[_myIndex].color = color;
        boardNames[_myIndex].color = color;
        boardScores[_myIndex].color = color;

        playerSprite.color = __color2;
        smallPlayerSprite[_myIndex].color = __color2;
        energySprite.color = __color1;
        for (int i = 0; i < armorSprite.Length; i++)
        {
            armorSprite[i].color = color;
            smallArmorSprite[_myIndex][i].color = color;
        }
        sliderHandle.color = color;
    }

    //called on server only
    public void UpdateScore()
    {
        Rpc_SetScore(score);
        UpdateScoreBoard();
    }

    [ClientRpc]
    public void Rpc_UpdateScoreBoard()
    {
        UpdateScoreBoard();
    }

    public void UpdateForNewClient()
    {
        Cmd_SetColor(color);
        Cmd_SetName(playerName);
        Cmd_SetScore(score);      
    }

    public void OnLevelWasLoaded(int p_scene)
    {
        if (p_scene == 1)
        {
            scoreboard.gameObject.SetActive(true);
        }
        else
        {
            //changes the player object's color
            /**/
            StartCoroutine(ChangePlayerColor());
            Debug.Log("tried to change colors");  
            /**/          
            
            //deactivate the canvas object during game scene
            scoreboard.gameObject.SetActive(false);
        }
    }

    IEnumerator ChangePlayerColor()
    {
        yield return new WaitForSeconds(1);
        GameObject[] __temp = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < __temp.Length; i++)
        {
            if (__temp[i].GetComponent<PlayerManager>().myNetworkIndex == _myIndex)
            {
                __temp[i].GetComponent<PlayerManager>().UpdateColors(color);
            }
        }
    }

    /**/
    public void OnSliderValueChanged(float p_value)
    {
        int __hue = (int)p_value;
        Color __color = HSVtoRGB(__hue, 1, 1);
        //Debug.Log("changed color...");             
        Cmd_SetColor(__color);
    }
    public void OnInputFieldValueSet(string p_string)
    {
        playerName = p_string;
        Cmd_SetName(playerName);
    }
    public void SetLocalInput(bool p_value)
    {
        colorSelector.gameObject.SetActive(p_value);
        nameSelector.gameObject.SetActive(p_value);
        playerSprite.gameObject.SetActive(p_value);
    }
    /**/
    public static Color HSVtoRGB(int p_h, float p_s, float p_v)
    {
        float __r = 0, __g = 0, __b = 0;
        float __C, __X, __m;

        //normalizes p_h to 0 <= p_h < 360
        if (p_h < 0)
        {
            p_h += 360;
        }
        else if (p_h > 360)
        {
            p_h -= 360 * (int)(p_h / 360);
        }

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

    
    /// <summary> Returns a float[3] object, with float[0] = H, float[1] = S and float[2] = V;
    /// <para> H is in degrees! </para>
    /// </summary>
    public static float[] RGBtoHSV(Color p_color)
    {
        float __r = p_color.r,
            __g = p_color.g,
            __b = p_color.b;
        float __Cmax = Mathf.Max(new float[3] { __r, __g, __b }),
            __Cmin = Mathf.Min(new float[3] { __r, __g, __b }),
            __delta = __Cmax - __Cmin;
        float __h = 0, __s = 0, __v = 0;
        

        //HUE Calculation
        if (__delta == 0)
        {
            __h = 0;
        }
        else if (__Cmax == __r)
        {
            __h = (((__g - __b)/__delta)%6) * 60;
        }
        else if (__Cmax == __g)
        {
            __h = (((__b - __r) / __delta) + 2) * 60;
        }
        else if (__Cmax == __b)
        {
            __h = (((__r - __g) / __delta) + 4) * 60;
        }

        //SATURATION calculation
        if(__Cmax == 0)
        {
            __s = 0;
        }
        else
        {
            __s = (__delta / __Cmax);
        }

        //VALUE Calculation
        __v = __Cmax;

        return new float[3] { __h, __s, __v };
    }
}
