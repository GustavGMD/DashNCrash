using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerMovement : NetworkBehaviour {
    
    public const float maxSwipeMagnitude = 10;

	public PlayerAttributes myAttributes;

	public bool SwipeStarted;
	public Vector2 SwipeStartPoint;
	public Vector2 SwipeEndPoint;
	public Vector2 SwipeForce;
	public int ForceScale;
    public bool inputEnabled = false;
    public AudioSource dashCharge;

	// Use this for initialization
	public override void OnStartLocalPlayer()
    {
 	    base.OnStartLocalPlayer();        

		SwipeStarted = false;
        //myMesseger = (NetworkMesseger)GameObject.FindObjectOfType(typeof(NetworkMesseger));        
	}
	
	[ClientCallback]
	void Update () {

        if (inputEnabled)
        {
            if (isLocalPlayer)
            {
                //Quando o usuario pressionar a tela, ou clicar com o mouse, a computacao do Swipe tem inicio
                if (Input.GetMouseButtonDown(0))
                {
                    SwipeStarted = true;
                    SwipeStartPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    dashCharge.volume = 1;
                }

                //enquanto o swipe estiver ativo, exibe um feedback visual com uma preview da forca
                if (SwipeStarted)
                {
                    Vector2 tempMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    myAttributes.setSwipeFeedback(tempMousePos - SwipeStartPoint, maxSwipeMagnitude);
                }

                //ao soltar a tela, ou soltar o botao do mouse, o Swipe chega ao fim
                if (Input.GetMouseButtonUp(0))
                {
                    float tempSwipeMagnitude;
                    SwipeStarted = false;
                    SwipeEndPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    dashCharge.volume = 0;

                    //gameObject.SendMessage("ApplyMovement");
                    SwipeForce = SwipeStartPoint - SwipeEndPoint;
                    //tempSwipeMagnitude = Mathf.Sqrt(SwipeForce.sqrMagnitude / (maxSwipeMagnitude * maxSwipeMagnitude));
                    tempSwipeMagnitude = SwipeForce.magnitude / maxSwipeMagnitude;


                    //myAttributes.ConsumeEnergy(tempSwipeMagnitude);
                    //Debug.Log("Movement Swipe: " + tempSwipeMagnitude);

                    SwipeForce = SwipeForce.normalized * (tempSwipeMagnitude);
                    Cmd_ApplyMovement(SwipeForce);
                    //Local_ApplyMovement(SwipeForce);
                    myAttributes.setSwipeFeedback(Vector2.zero, maxSwipeMagnitude);
                }
            }
        }
	}

	[Command]
	void Cmd_ApplyMovement(Vector2 SF){
        /**
        if (myAttributes.DashSizeLimit() < SF.magnitude)
        {
            SF = SF.normalized * myAttributes.DashSizeLimit();
        }
        myAttributes.ConsumeEnergy(SF.magnitude);	
        /**/
        //Debug.Log (SwipeStartPoint.ToString () + "     " + SwipeEndPoint.ToString ());	
        GetComponent<PlayerManager>().Rpc_PlaySound(AudioManager.SFXType.DASH_RELEASE);
        gameObject.GetComponent<Rigidbody2D> ().AddForce (SF * ForceScale);		
	}
    
	void Local_ApplyMovement(Vector2 SF){
		//if (isServer || isLocalPlayer) {
		//Debug.Log (SwipeStartPoint.ToString () + "     " + SwipeEndPoint.ToString ());
		//SwipeForce = SwipeStartPoint - SwipeEndPoint;
		gameObject.GetComponent<Rigidbody2D> ().AddForce (SF * ForceScale);
		//}
	}

    public void EnableInput(bool p_bool)
    {
        inputEnabled = p_bool;
    }
}
