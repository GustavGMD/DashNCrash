using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;

public class PlayerManager : NetworkBehaviour
{
    [SyncVar]
    public int myNetworkIndex = -10;

    public PowerUp powerUp;
    public GameObject[] powerUpVisualEffects;

    public PlayerAttributes playerAttributes;
    public CollisionManager collisionManager;
    public PlayerMovement playerMovement;
    public myLobbyManager lobbyManager;
    public Renderer bodySprite;
    public Color bodyColor;
    public Color armorColor;

    public GameObject deathParticles;
    public GameObject damageParticles;
    public GameObject normalParticles;

    public AudioManager audioManager;

    void Awake()
    {
        lobbyManager = GameObject.Find("LobbyManager").GetComponent<myLobbyManager>();
        Camera.main.GetComponent<CameraFocus>().AddPlayerObject(gameObject);
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
    }

    public override void OnStartServer()
    {
        //base.OnStartLocalPlayer();
        myNetworkIndex = GameObject.Find("LobbyManager").GetComponent<myLobbyManager>().ConnectionToIndex(connectionToClient.connectionId);
        //Cmd_SetIndex(myNetworkIndex);
        //Debug.Log("connections " + ClientScene.readyConnection);
        //Debug.Log("connections " + connectionToServer);
        //Debug.Log("connections " + connectionToClient);
        //Debug.Log("connections " + GetComponent<NetworkIdentity>().clientAuthorityOwner);
        //GetComponent<NetworkIdentity>().
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Camera.main.GetComponent<CameraFocus>().AddLocalPlayerObject(gameObject);
    }

    public void OnReceiveDamage(int p_armorIndex, Vector2 p_contact)
    {
        if (powerUp == null)
        {
            gameObject.GetComponent<PlayerAttributes>().ReceiveDamage(p_armorIndex);
            //Rpc_PlaySound(AudioManager.SFXType.COLLISION_DAMAGE);
        }
        else
        {
            if (powerUp.type == PowerUp.PowerUpType.SHIELD)
            {
                RemovePowerUp();
                audioManager.PlaySound(AudioManager.SFXType.POWER_UP_SHIELD);
                Rpc_PlaySound(AudioManager.SFXType.POWER_UP_SHIELD);
                GetComponent<PlayerManager>().Rpc_InstantiateParticle(0, transform.position);
            }
            else
            {
                gameObject.GetComponent<PlayerAttributes>().ReceiveDamage(p_armorIndex);
                //Rpc_PlaySound(AudioManager.SFXType.COLLISION_DAMAGE);
            }
        }
    }

    public void RemovePowerUp()
    {
        UpdateVisualEffect(powerUp.type, false);
        powerUp = null;
    }

    public void ReceivePowerUp(PowerUp p_powerup)
    {
        UpdateVisualEffect(p_powerup.type, true);
        powerUp = p_powerup;
        audioManager.PlaySound(AudioManager.SFXType.POWER_UP_COLLECT);
        Rpc_PlaySound(AudioManager.SFXType.POWER_UP_COLLECT);
    }

    [ClientRpc]
    public void Rpc_updateVisualEffect(PowerUp.PowerUpType p_index, bool p_value)
    {
        if (!isServer)
        {
            if (powerUpVisualEffects[(int)p_index] != null) powerUpVisualEffects[(int)p_index].SetActive(p_value);
        }
    }

    public void UpdateVisualEffect(PowerUp.PowerUpType p_index, bool p_value)
    {
        for (int i = 0; i < powerUpVisualEffects.Length; i++)
        {
            powerUpVisualEffects[i].SetActive(false);
        }

        if (powerUpVisualEffects[(int)p_index] != null) powerUpVisualEffects[(int)p_index].SetActive(p_value);

        Rpc_updateVisualEffect(p_index, p_value);
    }

    public void UpdateColors(Color p_baseColor)
    {
        float[] __colorHSV = LobbyGUI.RGBtoHSV(p_baseColor);
        float __h1 = __colorHSV[0] + LobbyGUI.__c1;
        float __h2 = __colorHSV[0] + LobbyGUI.__c2;
        Color __color1 = LobbyGUI.HSVtoRGB((int)__h1, __colorHSV[1], __colorHSV[2]);
        Color __color2 = LobbyGUI.HSVtoRGB((int)__h2, __colorHSV[1], __colorHSV[2]);

        bodyColor = __color2;
        armorColor = p_baseColor;

        for (int i = 0; i < playerAttributes.myArmor.Length; i++)
        {
            //change armor sprite color
            playerAttributes.myArmor[i].GetComponent<SpriteRenderer>().color = armorColor;
        }
        playerAttributes.mySprite.GetComponent<SpriteRenderer>().color = bodyColor;
        //changer energy sprite color
        playerAttributes.myEnergy.transform.GetChild(0).GetComponent<SpriteRenderer>().color = __color1;

        Material __mat = GetComponentInChildren<TrailRenderer>().material; 
        __mat.SetColor("_Color", bodyColor);
    }

    [Command]
    public void Cmd_SetIndex(int p_index)
    {
        myNetworkIndex = p_index;
        Debug.Log("my net index " + p_index);
        //Rpc_SetIndex(p_index);
    }
    [ClientRpc]
    public void Rpc_SetIndex(int p_index)
    {
        myNetworkIndex = p_index;
    }

    [ClientRpc]
    public void Rpc_InstantiateParticle(int p_particleIndex, Vector2 p_pos)
    {
        if(p_particleIndex == 0)
        {
            InstantiateParticles(normalParticles, p_pos);
        }
        else if (p_particleIndex == 1)
        {
            InstantiateParticles(damageParticles, p_pos);
        }
        else if (p_particleIndex == 2)
        {
            InstantiateParticles(deathParticles, p_pos);
        }
    }

    [ClientRpc]
    public void Rpc_PlaySound(AudioManager.SFXType p_sound)
    {
        if(p_sound == AudioManager.SFXType.DASH_RELEASE)
        {
            float __pitch = (Mathf.Sin(Mathf.Deg2Rad * LobbyGUI.RGBtoHSV(bodyColor)[0])* 1.5f) + 1.5f;
            audioManager.PlayPitchedSound(p_sound, __pitch);
        }
        else
        {
            audioManager.PlaySound(p_sound);
        }
    }

    public void InstantiateParticles(GameObject p_object, Vector2 p_pos)
    {
        GameObject __temp = (GameObject)Instantiate(p_object, p_pos, Quaternion.identity);
        __temp.GetComponent<ParticleKiller>().armorColor = armorColor;
        __temp.GetComponent<ParticleKiller>().bodyColor = bodyColor;
    }


}
    
