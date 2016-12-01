using UnityEngine;
using System.Collections;

public class PlayerElements : MonoBehaviour {

    //atributos de jogo da unidade
    public int HP_max, HP_curr;
    public int Energy_max;
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
    
    public AudioManager audioManager;
    //public NetworkMesseger myMesseger;
    public GameObject mySwipeFeedback;
    public SpriteRenderer mySprite;

    public ArmorScript[] myArmor;
    public HPScript myHP;
    public EnergyScript myEnergy;
    
    public int matchRank;
    public int gameRank;
    public int gameScore;

    public Color[] auraColor;
    public GameObject myAura;


    // Use this for initialization
    void Awake()
    {
        //myMesseger = myNM.myMesseger;
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();

        Armor_max = new int[4];
        Armor_curr = new int[4];
        Armor_Debuff = new bool[4]; ;
        for (int i = 0; i < 4; i++)
        {
            Armor_max[i] = 25;
            Armor_curr[i] = Armor_max[i];
            Armor_Debuff[i] = false;
        }

        //inicializa os objetos da armadura
        for (int i = 0; i < 4; i++)
        {
            myArmor[i].max_HP = Armor_max[i];
            myArmor[i].curr_HP = Armor_curr[i];
        }
    }

    public void Start()
    {        
        UpdateScoreStats();
        //mySprite.color = new Color(0, 1, 1);       
    }
    

    // Update is called once per frame
    void Update()
    {
        if (Energy_curr < Energy_max)
        {
            Energy_count += Time.deltaTime;
            if (Energy_count >= recover_rate)
            {
                Energy_curr += Energy_recovered;
                UpdateEnergySprite(Energy_max, Energy_curr);
                if (Energy_curr > Energy_max) Energy_curr = Energy_max;
                Energy_count = 0;
            }
        }
    }

    //retorna a porcentagem maxima de um Dash que esta unidade pode realizar
    public float DashSizeLimit()
    {
        float tempFloat = (float)Energy_curr / (float)max_energy_per_dash;
        return (tempFloat > 1 ? 1 : tempFloat);
    }

    public void ConsumeEnergy(float amount)
    {
        Energy_curr -= (int)(amount * max_energy_per_dash);
        if (Energy_curr < 0) Energy_curr = 0;
        UpdateEnergySprite(Energy_max, Energy_curr);
    }

    //esta funcao somente eh chamada no server
    public void ReceiveDamage(int armor_index)
    {
        //tenta causar dano na armadura, se falhar é pq não tinha então deve morrer
        if (!ApplyArmorDebuff(armor_index))
        {
            audioManager.PlaySound(AudioManager.SFXType.COLLISION_DEATH);
            GetComponent<PlayerAdministrator>().InstantiateParticle(2, transform.position);
            DeathProcedure();
        }
    }

    public bool ApplyArmorDebuff(int index)
    {
        if (!Armor_Debuff[index])
        {
            Armor_Debuff[index] = true;
            UpdateArmorSprite(index, Armor_max[index]);
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public void DeathProcedure()
    {
        UpdateScoreStats();
        DeactivateUnit();
        gameObject.SetActive(false);
    }
    
    void DeactivateUnit()
    {
        audioManager.PlaySound(AudioManager.SFXType.COLLISION_DEATH);
        gameObject.SetActive(false);
    }
        
    void UpdateArmorSprite(int armor_index, int dmg)
    {
        audioManager.PlaySound(AudioManager.SFXType.COLLISION_DAMAGE);
        GetComponent<PlayerAdministrator>().InstantiateParticles(GetComponent<PlayerAdministrator>().damageParticles, transform.position);
        myArmor[armor_index].ApplyDamage(dmg);
    }
    
    void UpdateHPSprite(float max, float curr)
    {
        myHP.UpdateSprite(max, curr);
    }
     
    public void UpdateEnergySprite(float max, float curr)
    {
        Color __baseColor = gameObject.GetComponent<PlayerAdministrator>().bodyColor;
        float[] __HSV = LobbyGUI.RGBtoHSV(__baseColor);
        float __perc = 0.75f;
        float __SV = ((curr / max) * __perc) + (1 - __perc);
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
        GetComponent<PlayerController>().dashCharge.pitch = tempSwipeMagnitude * 2f + 0.5f;

        mySwipeFeedback.transform.localScale = new Vector3(SwipeForce.magnitude * 3, 1, 1);
        mySwipeFeedback.transform.eulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * Mathf.Atan2(SwipeForce.y, SwipeForce.x));
    }

    public void UpdateScoreStats()
    {
        //gameRank = myNM.MyGlobalRank(connectionToClient);
        //matchRank = myNM.MyMatchRank(connectionToClient);
        //gameScore = myNM.globalScore[connectionToClient.connectionId];
        //Rpc_UpdateSprites();
        //if(gameObject.activeSelf) StartCoroutine(UpdateSpritesAfterSeconds());
        StartCoroutine(UpdateSpritesAfterSeconds());
    }

    IEnumerator UpdateSpritesAfterSeconds()
    {
        //yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(1f);
        UpdateSprites();
    }
    
    public void UpdateSprites()
    {
        //myAura.GetComponent<SpriteRenderer>().color = auraColor[gameRank - 1];
    }
}
