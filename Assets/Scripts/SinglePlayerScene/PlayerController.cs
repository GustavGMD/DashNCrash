using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    public const float maxSwipeMagnitude = 10;

    public PlayerElements myAttributes;

    public bool SwipeStarted;
    public Vector2 SwipeStartPoint;
    public Vector2 SwipeEndPoint;
    public Vector2 SwipeForce;
    public int ForceScale;
    public bool inputEnabled = false;
    public AudioSource dashCharge;

    // Use this for initialization
    public void Start()
    {
        SwipeStarted = false;      
    }
    
    void Update()
    {

        if (inputEnabled)
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
                
                SwipeForce = SwipeStartPoint - SwipeEndPoint;
                tempSwipeMagnitude = SwipeForce.magnitude / maxSwipeMagnitude;
                
                SwipeForce = SwipeForce.normalized * (tempSwipeMagnitude);
                ApplyMovement(SwipeForce);
                myAttributes.setSwipeFeedback(Vector2.zero, maxSwipeMagnitude);
            }
        }
    }

    void ApplyMovement(Vector2 SF)
    {
        gameObject.GetComponent<Rigidbody2D>().AddForce(SF * ForceScale);
    }

    public void EnableInput(bool p_bool)
    {
        inputEnabled = p_bool;
    }
}
