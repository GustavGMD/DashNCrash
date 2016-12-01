using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CollisionManager : NetworkBehaviour {
    
    public ContactPoint2D[] tempContact;
    public PowerUpManager powerUpManager;
    public AudioManager audioManager;

    public void Awake()
    {
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
    }

    public override void OnStartClient()
    {
        StartCoroutine(GetLobbyManager());
    }

    IEnumerator GetLobbyManager()
    {
        yield return new WaitForSeconds(1);
        powerUpManager = GameObject.FindObjectOfType<PowerUpManager>();

    }

    [ServerCallback]
    void OnCollisionEnter2D(Collision2D thisCollision)
    {
        tempContact = thisCollision.contacts;

        for (int i = 0; i < tempContact.Length; i++)
        {
            //Debug.Log((tempContact[i].point - new Vector2(transform.position.x, transform.position.y)).normalized);
            //Debug.DrawLine(new Vector2(transform.position.x, transform.position.y), tempContact[i].point, Color.red);
            float angle = Mathf.Atan2((tempContact[i].point - new Vector2(transform.position.x, transform.position.y)).normalized.y,
                                      (tempContact[i].point - new Vector2(transform.position.x, transform.position.y)).normalized.x);
            //Debug.Log ("Collision Angle: "+(Mathf.Rad2Deg* angle).ToString());
            int arm_index = -1;
            //Atan2 retorna angulos entre PI e -PI
            if (angle <= Mathf.PI / 4 && angle > -Mathf.PI / 4)
            {
                arm_index = 1;
            }
            else if (angle <= Mathf.PI * 3 / 4 && angle > Mathf.PI / 4)
            {
                arm_index = 0;
            }
            else if (angle > Mathf.PI * 3 / 4 || angle <= -Mathf.PI * 3 / 4)
            {
                arm_index = 3;
            }
            else if (angle <= -Mathf.PI / 4 && angle > -Mathf.PI * 3 / 4)
            {
                arm_index = 2;
            }

            if (thisCollision.gameObject.tag == "LaserWall")
            {
                //myAttributes.ReceiveDamage(arm_index, myAttributes.base_dmg*5);
                GetComponent<PlayerManager>().OnReceiveDamage(arm_index, thisCollision.contacts[0].point);
            }            
            else if (thisCollision.gameObject.tag == "Player")
            {
                //confere se o player com quem colidiu possui algum powerUp
                //se sim, trata de acordo, se não, não faz nada
                if (thisCollision.gameObject.GetComponent<PlayerManager>().powerUp != null)
                {
                    switch (thisCollision.gameObject.GetComponent<PlayerManager>().powerUp.type)
                    {
                        case PowerUp.PowerUpType.DAMAGE:
                            thisCollision.gameObject.GetComponent<PlayerManager>().RemovePowerUp();
                            GetComponent<PlayerManager>().OnReceiveDamage(arm_index, thisCollision.contacts[0].point);
                            break;
                        case PowerUp.PowerUpType.SHIELD:
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    if(GetComponent<PlayerManager>().powerUp == null)
                    {
                        GetComponent<PlayerManager>().Rpc_PlaySound(AudioManager.SFXType.COLLISION_NORMAL);
                        GetComponent<PlayerManager>().Rpc_InstantiateParticle(0, thisCollision.contacts[0].point);
                    }
                }
            }
        }
    }

    [ServerCallback]
    void OnTriggerEnter2D(Collider2D p_collider)
    {
        if (p_collider.tag == "DragArea")
        {
            GetComponent<Rigidbody2D>().drag += p_collider.GetComponent<DragArea>().dragValue;
            if(p_collider.GetComponent<DragArea>().dragValue < 0)
            {
                GetComponent<PlayerManager>().Rpc_PlaySound(AudioManager.SFXType.SLIDE_AREA);
            }
            else
            {
                GetComponent<PlayerManager>().Rpc_PlaySound(AudioManager.SFXType.SLOW_AREA);
            }
        }
        else if (p_collider.gameObject.tag == "PowerUp")
        {
            //pega o power Up
            //atribui ele a si mesmo
            GetComponent<PlayerManager>().ReceivePowerUp(p_collider.gameObject.GetComponent<PowerUp>());
            //modifica o efeito visual d power up
            //avisa o powerUp Manager que este foi pego
            powerUpManager.PowerUpCaught(p_collider.gameObject.GetComponent<PowerUp>());
        }
    }

    [ServerCallback]
    void OnTriggerExit2D(Collider2D p_collider)
    {
        if (p_collider.tag == "DragArea")
        {
            GetComponent<Rigidbody2D>().drag -= p_collider.GetComponent<DragArea>().dragValue;
        }
    }
}
