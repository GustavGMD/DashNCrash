using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class LobbyPlayerScript : NetworkLobbyPlayer
{
    public override void OnClientEnterLobby()
    {
        base.OnClientEnterLobby();
        //SendReadyToBeginMessage();
        //OnClientReady(true);
        //Debug.Log(readyToBegin);
        //OnClientReady(true);
        StartCoroutine(SetAsReady());
    }

    IEnumerator SetAsReady()
    {
        yield return new WaitForSeconds(1);
        SendReadyToBeginMessage();
        OnClientReady(true);
        //Debug.Log(readyToBegin);
    }
}
