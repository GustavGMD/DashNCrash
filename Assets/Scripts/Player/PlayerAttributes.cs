using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerAttributes : NetworkBehaviour {

   	//atributos de jogo da unidade
	public int HP_max, HP_curr;
    [SyncVar]
	public int Energy_max;
    [SyncVar]
    public int Energy_curr;
    public int Energy_recovered;
    public float recover_rate;
    float Energy_count;
    public int max_energy_per_dash;

	//indices da armadura: 0 norte; 1 leste; 2 sul; 3 oeste;
	public int[] Armor_max;
	public int[] Armor_curr;
	public bool[] Armor_Debuff; //false for intact armor, true for destroyed armor

	public int base_dmg;

    public myLobbyManager myNM;
    public AudioManager audioManager;
    //public NetworkMesseger myMesseger;
    public GameObject mySwipeFeedback;
    public SpriteRenderer mySprite;

	public ArmorScript[] myArmor;
	public HPScript myHP;
	public EnergyScript myEnergy;

    [SyncVar]
    public int matchRank;
    [SyncVar]
    public int gameRank;
    [SyncVar]
    public int gameScore;

    public Color[] auraColor;    
    public GameObject myAura;


	// Use this for initialization
	void Awake () {
        //Debug.Log("Player Attributes: Iniciou o Awake");
		myNM = GameObject.Find ("LobbyManager").GetComponent<myLobbyManager>();
        //myMesseger = myNM.myMesseger;
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
                
        Armor_max = new int[4];
		Armor_curr = new int[4];
		Armor_Debuff = new bool[4];;
		for (int i = 0; i < 4; i++) {
			Armor_max[i] = 25;
			Armor_curr[i] = Armor_max[i];
			Armor_Debuff[i] = false;
		}

		//inicializa os objetos da armadura
		for (int i = 0; i < 4; i++) {
			myArmor [i].max_HP = Armor_max [i];
			myArmor [i].curr_HP = Armor_curr [i];
		}
	}

    public override void OnStartClient()
    {
        base.OnStartClient();
        //Debug.Log("Player Attributes: Iniciou o Start");
        if (isServer)
        {
            Debug.Log("called");
            UpdateScoreStats();
        }
        //myAura.GetComponent<SpriteRenderer>().color = auraColor[gameRank - 1];
        //mySprite.color = new Color(1, 0, 1);

        myNM.gameScenePlayers.Add(gameObject);

        //Debug.Log("Player Attributes: Terminou o Start");
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        //mySprite.color = new Color(0, 1, 1);
    }
	
	// Update is called once per frame
	[ServerCallback]
	void Update () {
        if (Energy_curr < Energy_max)
        {
            Energy_count += Time.deltaTime;
            if (Energy_count >= recover_rate)
            {
                Energy_curr += Energy_recovered;
				Rpc_UpdateEnergySprite(Energy_max, Energy_curr);
                if (Energy_curr > Energy_max) Energy_curr = Energy_max;
                Energy_count = 0;
            }
        }
	}

    //retorna a porcentagem maxima de um Dash que esta unidade pode realizar
	public float DashSizeLimit()
    {
        float tempFloat = (float)Energy_curr / (float)max_energy_per_dash;
        return (tempFloat > 1 ? 1:tempFloat);
    }
	
    public void ConsumeEnergy(float amount)
    {
        Energy_curr -= (int)(amount*max_energy_per_dash);
        if (Energy_curr < 0) Energy_curr = 0;
		Rpc_UpdateEnergySprite(Energy_max, Energy_curr);
    }

	//esta funcao somente eh chamada no server
	public void ReceiveDamage(int armor_index){

        /**
		//confere se o dano eh mortal ou nao
		if ((!Armor_Debuff[armor_index] && HP_curr >= dmg) || (Armor_Debuff[armor_index] && HP_curr >= dmg*2)) {
			//aplica o dano
			HP_curr -= (Armor_Debuff[armor_index] ? dmg*2:dmg);
			Rpc_UpdateHPSprite(HP_max, HP_curr);
		} else {
			//morre
			DeathProcedure();
		}

		//se a armadura nao foi destruida ainda, confere se ela vai ser destruida agora
		if (!Armor_Debuff [armor_index]) {
			//danifica a armadura
			if(dmg >= Armor_curr[armor_index]){
				//quebra a armadura e aplica debuff
				ApplyArmorDebuff(armor_index);
			}
			Armor_curr[armor_index] -= dmg;
			//modifica os objetos de armadura
			myArmor[armor_index].ApplyDamage(dmg);
			Rpc_UpdateArmorSprite(armor_index, dmg);
		}
        /**/

        //tenta causar dano na armadura, se falhar é pq não tinha então deve morrer
        if (!ApplyArmorDebuff(armor_index))
        {
            audioManager.PlaySound(AudioManager.SFXType.COLLISION_DEATH);
            GetComponent<PlayerManager>().Rpc_InstantiateParticle(2, transform.position);
            DeathProcedure();
        }
	}

	public bool ApplyArmorDebuff(int index){
        if (!Armor_Debuff[index])
        {
            Armor_Debuff[index] = true;
            Rpc_UpdateArmorSprite(index, Armor_max[index]);
            return true;
        }
        else
        {
            return false;
        }
	}

	[Server]
	public void DeathProcedure(){
		//Debug.Log ("Unidade Morreu");
		//myNM.OnServerPlayerLost (connectionToClient);
        //myMesseger.ILost(connectionToClient);
        //myMesseger.MyRank(connectionToClient);
        myNM.ILost(connectionToClient);
        UpdateScoreStats();
		Rpc_DeactivateUnit ();
        gameObject.SetActive(false);        
        //Debug.Log("Finalizou procesos de morte");
	}

	[ClientRpc]
	void Rpc_DeactivateUnit(){
        audioManager.PlaySound(AudioManager.SFXType.COLLISION_DEATH);
        gameObject.SetActive (false);
	}

	[ClientRpc]
	void Rpc_UpdateArmorSprite(int armor_index, int dmg){
        audioManager.PlaySound(AudioManager.SFXType.COLLISION_DAMAGE);        
        GetComponent<PlayerManager>().InstantiateParticles(GetComponent<PlayerManager>().damageParticles, transform.position);
        myArmor[armor_index].ApplyDamage(dmg);
	}

	[ClientRpc]
	void Rpc_UpdateHPSprite(float max, float curr){
		myHP.UpdateSprite (max, curr);
	}

	[ClientRpc]
	void Rpc_UpdateEnergySprite(float max, float curr){
        //myEnergy.UpdateSprite (max, curr)
        UpdateEnergySprite(max, curr);
	}

    public void UpdateEnergySprite(float max, float curr)
    {
        Color __baseColor = gameObject.GetComponent<PlayerManager>().bodyColor;
        float[] __HSV = LobbyGUI.RGBtoHSV(__baseColor);
        float __perc = 0.75f;
        float __SV = ((curr / max) * __perc) + (1-__perc);
        mySprite.color = LobbyGUI.HSVtoRGB((int)__HSV[0], __SV, __SV);
    }

    public void setSwipeFeedback(Vector2 SwipeForce, float maxSwipeMagnitude)
    {
        float tempSwipeMagnitude;       
        
        //tempSwipeMagnitude = Mathf.Sqrt(SwipeForce.sqrMagnitude / (maxSwipeMagnitude * maxSwipeMagnitude));
        tempSwipeMagnitude = SwipeForce.magnitude / (maxSwipeMagnitude);

        //calcula consumo de energia, e limita o tamanho do dash se necessario
        //Debug.Log("dashLimit " + DashSizeLimit().ToString());
        //Debug.Log("SwipeMagnitude " + tempSwipeMagnitude.ToString());
        if (DashSizeLimit() < tempSwipeMagnitude)
        {
			tempSwipeMagnitude = DashSizeLimit();
        }
        //Debug.Log("Feedback Swipe: " + tempSwipeMagnitude);
        SwipeForce = SwipeForce.normalized * (tempSwipeMagnitude);
        GetComponent<PlayerMovement>().dashCharge.pitch = tempSwipeMagnitude*2f + 0.5f;

        mySwipeFeedback.transform.localScale = new Vector3(SwipeForce.magnitude*3, 1, 1);
        mySwipeFeedback.transform.eulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * Mathf.Atan2(SwipeForce.y, SwipeForce.x));
    }

    public void UpdateScoreStats()
    {
        gameRank = myNM.MyGlobalRank(connectionToClient);
        matchRank = myNM.MyMatchRank(connectionToClient);
        gameScore = myNM.globalScore[connectionToClient.connectionId];
        //Rpc_UpdateSprites();
        //if(gameObject.activeSelf) StartCoroutine(UpdateSpritesAfterSeconds());
        StartCoroutine(UpdateSpritesAfterSeconds());
    }

    IEnumerator UpdateSpritesAfterSeconds()
    {
        //yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(1f);
        Rpc_UpdateSprites();
    }

    [ClientRpc]
    public void Rpc_UpdateSprites()
    {
        myAura.GetComponent<SpriteRenderer>().color = auraColor[gameRank - 1];
    }    
}