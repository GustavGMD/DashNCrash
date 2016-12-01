using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class myLobbyManager : NetworkLobbyManager {

    //public NetworkMesseger myMesseger;

    //variaveis de controle do fluxo de jogo
    //List<int> playerLostOrder; //um inteiro que representa o numero da conexao    
    //public int playerCount;

    public event System.Action<string> onSceneChange;

    public Dictionary<int, int> globalScore;
    public List<int> defeatedList;
    public List<GameObject> gameScenePlayers;
    //public GameManager scoreboardManager;
    //public List<LobbyPlayerScript> lobbyPlayers;
    public GameObject[] LobbyGUI;

    public Canvas connectionCanvas;
    public Canvas lobbyCanvas;
    public Canvas startButtonCanvas;
    public Text hostIP;
    public Text[] scoreDisplay;
    public InputField connectionIP;
    public Button startButton;

    public LobbyGUI localLobbyGUI;
    public AudioManager audioManager;

    private string _LastIPKey = "LastIP";
    private GameObject _scoreboardModel;
    private List<GameObject> _spawnedLobbyGUIs;
    private bool _host = false;

    void Start()
    {
        //playerLostOrder = new List<int>();
        globalScore = new Dictionary<int, int>();
        defeatedList = new List<int>();
        _spawnedLobbyGUIs = new List<GameObject>();
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();

        string __address;
        if (PlayerPrefs.HasKey(_LastIPKey))
        {
            __address = PlayerPrefs.GetString(_LastIPKey);
        }
        else
        {
            PlayerPrefs.SetString(_LastIPKey, "localhost");
            __address = PlayerPrefs.GetString(_LastIPKey);
        }
        connectionIP.text = __address;
        networkAddress = __address;
        /**/

        //register prefabs!
        for (int i = 0; i < spawnPrefabs.Count; i++)
        {
            ClientScene.RegisterPrefab(spawnPrefabs[i]);
        }
        for (int i = 0; i < LobbyGUI.Length; i++)
        {
            ClientScene.RegisterPrefab(LobbyGUI[i]);
        }

        startButton.onClick.AddListener(delegate
        {
            //start the game here
            Debug.Log("started the game");
            audioManager.PlaySound(AudioManager.SFXType.MENU_BUTTON);
            ServerChangeScene("GameScene");
        });

        //ClientScene.RegisterSpawnHandler(spawnPrefabs[0].GetComponent<NetworkIdentity>().assetId, 

    }

    public override void OnLobbyStartHost()
    {
        base.OnLobbyStartHost();
        //GameObject __temp = (GameObject)Instantiate(scoreboardManager, Vector3.zero, Quaternion.identity);
        //NetworkServer.Spawn(__temp);
        //Debug.Log("connected");
        //server not active...

        //_scoreboardModel = (GameObject)Instantiate(spawnPrefabs[0], Vector3.zero, Quaternion.identity);        
    }
    /**/

    public override void OnServerConnect(NetworkConnection conn)
    {   
        base.OnServerConnect(conn);

        Debug.Log("called initialize");
        InitializeScore(conn.connectionId);        
        //Debug.Log("connected id: "+conn.connectionId);
        //NetworkServer.Spawn(_scoreboardModel);
        //StartCoroutine(SpawnScoreboard());
        //Debug.Log(ConnectionToIndex(conn.connectionId));
        GameObject __temp = (GameObject)Instantiate(LobbyGUI[ConnectionToIndex(conn.connectionId)], Vector3.zero, Quaternion.identity);
        __temp.GetComponent<LobbyGUI>()._myIndex = _spawnedLobbyGUIs.Count;
        _spawnedLobbyGUIs.Add(__temp);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        //Debug.Log("Registered the prefabs...");
        //Debug.Log(ClientScene.ready);
        //Debug.Log(ClientScene.readyConnection);
        //if(!ClientScene.ready) ClientScene.Ready(conn);
        //ClientScene.readyConnection;   
        //ClientScene.RegisterPrefab(spawnPrefabs[0]);        
    }

    IEnumerator SpawnScoreboard()
    {
        yield return new WaitForSeconds(10);
        Debug.Log("tried to spawn...");
        NetworkServer.Spawn(_scoreboardModel);
    }

    public override void OnLobbyClientConnect(NetworkConnection conn)
    {
        base.OnLobbyClientConnect(conn);
        //Debug.Log("connected");
    }

    public override void OnLobbyClientEnter()
    {
        base.OnLobbyClientEnter();
        //myMesseger.UpdateScore();
        //NetworkServer.SpawnObjects();
    }

    public override void OnLobbyServerPlayersReady()
    {
        //base.OnLobbyServerPlayersReady();
        //Debug.Log("player tá ready...");
        //NetworkServer.Spawn(_scoreboardModel);
        for (int i = 0; i < _spawnedLobbyGUIs.Count; i++)
        {
           // Debug.Log("network server  connections" + NetworkServer.connections.Count);
            for (int j = 0; j < NetworkServer.connections.Count; j++)
            {
                if (NetworkServer.connections[j] == null) continue;
                if(NetworkServer.connections[j].connectionId == IndexToConnection(i))
                {
                    //Debug.Log("my index: " + i);
                    NetworkServer.SpawnWithClientAuthority(_spawnedLobbyGUIs[i], NetworkServer.connections[j]);
                    //Debug.Log("spawnou net client");
                }
            }
            for (int j = 0; j < NetworkServer.localConnections.Count; j++)
            {
                if (NetworkServer.localConnections[j].connectionId == IndexToConnection(i))
                {
                    //_spawnedLobbyGUIs[i].GetComponent<LobbyGUI>().colorSelector.gameObject.SetActive(false);
                    NetworkServer.Spawn(_spawnedLobbyGUIs[i]);
                    _spawnedLobbyGUIs[i].GetComponent<LobbyGUI>().SetLocalInput(true);

                    //Debug.Log("spawnou local client");
                }
            }
        }
    }
    /**
    public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
    {
        NetworkServer.Spawn(_scoreboardModel);
        return base.OnLobbyServerCreateGamePlayer(conn, playerControllerId);
    }
    /**/

    public override GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
    {
        Debug.Log("creating lobby player...");
        return base.OnLobbyServerCreateLobbyPlayer(conn, playerControllerId);
    }
    /**
    public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
    {
        GameObject player = (GameObject)Instantiate(gamePlayerPrefab, Vector3.zero, Quaternion.identity);
        player.GetComponent<PlayerManager>().myNetworkIndex = ConnectionToIndex(conn.connectionId);
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
        return player;
    }
    /**/    

    public override void OnLobbyServerSceneChanged(string sceneName)
    {
        base.OnLobbyServerSceneChanged(sceneName);
        if (sceneName == "GameScene")
        {
            Debug.Log("Começou a spawnar as coisas...");
            //playerLostOrder.Clear();
            defeatedList.Clear();
            //NetworkServer.SpawnObjects();
            for (int i = 0; i < spawnPrefabs.Count; i++)
            {
                GameObject temp = (GameObject)Instantiate(spawnPrefabs[i], Vector3.zero, Quaternion.identity);
                NetworkServer.Spawn(temp);
            }
            Debug.Log("terminou de spawnar as coisas...");
        }
        else if (sceneName == "LobbyScene")
        {
            /**
            if (!isNetworkActive)
            {
                connectionCanvas.gameObject.SetActive(false);
            }
            /**/
        }
    }
    
    public void SetScore(int p_key, int p_score)
    {
        globalScore[p_key] = p_score;
        //UpdateLobbyGUIScore(p_key, p_score);
        _spawnedLobbyGUIs[ConnectionToIndex(p_key)].GetComponent<LobbyGUI>().score = p_score;
        _spawnedLobbyGUIs[ConnectionToIndex(p_key)].GetComponent<LobbyGUI>().UpdateScore();
    }
    public void InitializeScore(int p_key)
    {
        Debug.Log(globalScore.Count);
        Debug.Log(p_key);
        globalScore.Add(p_key, 0);
        //UpdateLobbyGUIScore(p_key, p_score);
    }
    public int MyGlobalRank(NetworkConnection conn)
    {
        //0: first, 1: second, 2: third, 3: fourth
        List<int> scoreBoard = new List<int>();
        int[] __tempKeys = new int[globalScore.Count];
        globalScore.Keys.CopyTo(__tempKeys, 0);

        for (int i = 0; i < globalScore.Count; i++)
        {
            scoreBoard.Add(globalScore[__tempKeys[i]]);
        }
        scoreBoard.Sort();

        for (int i = 0; i < scoreBoard.Count; i++)
        {
            //if (scoreBoard[i] == GetMyValue(conn.connectionId)) return i;
            if (scoreBoard[i] == globalScore[conn.connectionId]) return scoreBoard.Count-i;
        }

        return globalScore.Count - 1;
    }
    public int MyMatchRank(NetworkConnection conn)
    {
        List<int> __tempList = new List<int>();
        for (int i = 0; i < defeatedList.Count; i++)
        {
            __tempList.Add(globalScore[defeatedList[i]]);
        }
        __tempList.Sort();

        for (int i = 0; i < __tempList.Count; i++)
        {
            //if (scoreBoard[i] == GetMyValue(conn.connectionId)) return i;
            if (__tempList[i] == globalScore[conn.connectionId]) return __tempList.Count - i;
        }

        return 0;
    }
    public void ILost(NetworkConnection conn)
    {
        int[] __tempKeys = new int[globalScore.Count];
        globalScore.Keys.CopyTo(__tempKeys, 0);
        
        defeatedList.Add(conn.connectionId);

        /**/
        for (int i = 0; i < globalScore.Count; i++)
        {
            //dah um ponto para todos os players vivos
            if (!defeatedList.Contains(__tempKeys[i]))
            {
                SetScore(__tempKeys[i], globalScore[__tempKeys[i]] + 1);
            }
        }

        for (int i = 0; i < gameScenePlayers.Count; i++)
		{
            gameScenePlayers[i].GetComponent<PlayerAttributes>().UpdateScoreStats();
		}
        
        //Debug.Log("Computou os scores novos");
        //Debug.Log("Defeated Count: " + defeatedList.Count);
        /**/
        if (defeatedList.Count == globalScore.Count - 1)
        {
            //fim do jogo
            StartCoroutine("EndGame", 2);
        }
    }
    IEnumerator EndGame(float wait)
    {
        yield return new WaitForSeconds(wait);
        gameScenePlayers.Clear();
        ServerReturnToLobby();
    }
    /**
    public void UpdateScoreUI()
    {
        int[] __tempKeys = new int[globalScore.Count];
        globalScore.Keys.CopyTo(__tempKeys, 0);
        
        for (int i = 0; i < globalScore.Count; i++)
        {
            scoreDisplay[i].text = globalScore[__tempKeys[i]].ToString();
        }        
    }
    /**/
    public int ConnectionToIndex(int p_connID)
    {
        int[] __tempKeys = new int[globalScore.Count];
        int __num = -1;

        //Debug.Log("ID: " + p_connID);

        globalScore.Keys.CopyTo(__tempKeys, 0);
        for (int i = 0; i < __tempKeys.Length; i++)
        {
            // Debug.Log("Key " + i + ": " + __tempKeys[i]);
            if (__tempKeys[i] == p_connID) __num = i;
        }

        //Debug.Log("num: " + __num);
        return __num;
    }
    public int IndexToConnection(int p_index)
    {
        int[] __tempKeys = new int[globalScore.Count];        
        globalScore.Keys.CopyTo(__tempKeys, 0);
        return __tempKeys[p_index];
    }
    /**/
    void OnLevelWasLoaded(int p_scene)
    {
        if (p_scene == 1)
        {
            if (onSceneChange != null) onSceneChange("LobbyScene");
            if (isNetworkActive)
            {
                connectionCanvas.gameObject.SetActive(false);
                lobbyCanvas.gameObject.SetActive(true);
                if (_host)
                {
                    EnableStartButton(true);
                }
            }
            else
            {
                connectionCanvas.gameObject.SetActive(true);
                lobbyCanvas.gameObject.SetActive(false);
            }
        }
        else if (p_scene == 2)
        {
            if (onSceneChange != null) onSceneChange("GameScene");
            {
                connectionCanvas.gameObject.SetActive(false);
                lobbyCanvas.gameObject.SetActive(false);
                EnableStartButton(false);
            }
        }
    }
    /**/
    /**
    void OnGUI()
    {
        //pre-connection GUI
        if (!isNetworkActive)
        {
            if (GUI.Button(new Rect(GUIposition, GUIsize), "Host"))
            {
                StartHost();
                showLobbyGUI = true;
            }

            if (GUI.Button(new Rect(GUIposition + new Vector2(0, GUIsize.y * 2), GUIsize), "Connect"))
            {
                StartClient();
                showLobbyGUI = true;
            }
            networkAddress = GUI.TextField(new Rect(GUIposition + new Vector2(GUIsize.x, GUIsize.y * 2), GUIsize), networkAddress);
        }
    } 
    **/
    public void __GUI_HostButton()
    {
        StartHost();
        //showLobbyGUI = true;
        hostIP.text = Network.player.ipAddress;
        connectionCanvas.gameObject.SetActive(false);
        lobbyCanvas.gameObject.SetActive(true);
        //lobbyCanvas.gameObject.SetActive(true);
        //instantiate Scoreboard Object
        EnableStartButton(true);
        _host = true;
        audioManager.PlaySound(AudioManager.SFXType.MENU_BUTTON);
    }
    public void __GUI_ConnectButton()
    {
        StartClient();
        hostIP.text = networkAddress;
        //lobbyCanvas.gameObject.SetActive(true);
        connectionCanvas.gameObject.SetActive(false);
        //showLobbyGUI = true;
        lobbyCanvas.gameObject.SetActive(true);
        //instantiate Scoreboard Object
        _host = false;
        audioManager.PlaySound(AudioManager.SFXType.MENU_BUTTON);
    }
    public void __GUI_AddressField_ValueChange(string p_string)
    {
        networkAddress = p_string;
        audioManager.PlaySound(AudioManager.SFXType.MENU_BUTTON);
    }
    public void __GUI_AddressField_EndEdit(string p_string)
    {
        networkAddress = p_string;
        PlayerPrefs.SetString(_LastIPKey, p_string);
        Debug.Log("ended editing");
    }    

    public void EnableStartButton(bool p_value)
    {
        startButtonCanvas.gameObject.SetActive(p_value);        
    }

    public void UpdateLobbyGUIScore(int p_key, int p_score)
    {
        int __index = ConnectionToIndex(p_key);        

        Debug.Log(__index);
        Debug.Log(_spawnedLobbyGUIs.Count);
        if(__index < _spawnedLobbyGUIs.Count)
        _spawnedLobbyGUIs[__index].GetComponent<LobbyGUI>().score = p_score;
    }
}
