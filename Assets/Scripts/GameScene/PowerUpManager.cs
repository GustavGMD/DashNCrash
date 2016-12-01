using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class PowerUpManager : NetworkBehaviour {

    public List<bool> isItemSpawned;
    public List<Transform> spawnLocations;
    public List<PowerUp> powerUps;
    public GameObject particle;
    public AudioManager audioManager;
    
    [SyncVar]
    public int spawnCooldown = 10; //in seconds

    public void Start()
    {
        isItemSpawned = new List<bool>();
        for (int i = 0; i < powerUps.Count; i++)
        {
            powerUps[i].transform.position = spawnLocations[i].position;
            isItemSpawned.Add(false);
        }

        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
    }

    public override void OnStartServer()
    {
 	    base.OnStartServer();
        for (int i = 0; i < powerUps.Count; i++)
        {
            StartCoroutine(SpawnRoutine(i, 2));
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }
        

    public void Spawn(int p_index)
    {
        isItemSpawned[p_index] = true;
        powerUps[p_index].gameObject.SetActive(true);
        Rpc_Spawn(p_index);
    }
    
    public void PowerUpCaught(PowerUp p_pu)
    {
        for (int i = 0; i < powerUps.Count; i++)
        {
            if (powerUps[i] == p_pu)
            {
                isItemSpawned[i] = false;
                powerUps[i].gameObject.SetActive(false);
                Rpc_Remove(i);
                StartCoroutine(SpawnRoutine(i, spawnCooldown));
            }
        }
    }

    [ClientRpc]
    public void Rpc_Remove(int p_index)
    {
        if (!isServer)
        {
            powerUps[p_index].gameObject.SetActive(false);
        }
    }

    [ClientRpc]
    public void Rpc_Spawn(int p_index)
    {
        if (!isServer)
        {
            powerUps[p_index].gameObject.SetActive(true);
        }        
        GameObject __temp = (GameObject)Instantiate(particle, powerUps[p_index].transform.position, Quaternion.identity);
        __temp.GetComponent<ParticleSystemRenderer>().sortingOrder = 1;
        audioManager.PlaySound(AudioManager.SFXType.POWER_UP_SPAWN);
    }

    IEnumerator SpawnRoutine(int p_index, int p_waitingTime)
    {
        yield return new WaitForSeconds(p_waitingTime);
        Spawn(p_index);
    }
}
