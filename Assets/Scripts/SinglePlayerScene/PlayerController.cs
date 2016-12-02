using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    public bool aiControlled = false;
    public float aiActionTimer = 0;
    public float aiActionsPerSecond = 2;
    public float aiActionTimerThreshold;
    public ArtificialAgent myAI;

    public const float maxSwipeMagnitude = 10;

    public PlayerElements myAttributes;

    public bool SwipeStarted;
    public Vector2 SwipeStartPoint;
    public Vector2 SwipeEndPoint;
    public Vector2 SwipeForce;
    public int ForceScale;
    public bool inputEnabled = false;
    public AudioSource dashCharge;

    private bool _aiEnabled = false;
   
    // Use this for initialization
    public void Start()
    {
        SwipeStarted = false;
        aiActionTimerThreshold = 1f / aiActionsPerSecond;
        myAI = GetComponent<ArtificialAgent>();
        
    }
    
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Q))
        {
            _aiEnabled = !_aiEnabled;
        }
        if (!aiControlled)
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

                    //LogData(SwipeForce);
                }
            }
        }
        else
        {
            if (_aiEnabled)
            {
                aiActionTimer += Time.deltaTime;
                if (aiActionTimer >= aiActionTimerThreshold)
                {
                    float[] __inputs = CalculateInputs();

                    string __log = "Inputs: ";
                    for (int i = 0; i < __inputs.Length; i++)
                    {
                        __log += __inputs[i] + " / ";
                    }
                    //Debug.Log(__log);

                    myAI.brain.SetInputs(__inputs);

                    //Debug.Log("Acted " + Time.timeSinceLevelLoad);
                    aiActionTimer = 0;

                    //insert here AI scrip
                    //Vector2 direction = new Vector2(Random.Range(-1, 1), Random.Range(-1, 1));
                    Vector2 direction = myAI.AgentOutput();
                    float magnitude = maxSwipeMagnitude;
                    SwipeForce = direction.normalized * magnitude / (5 * 2);

                    //Debug.Log(SwipeForce.magnitude);
                    ApplyMovement(SwipeForce);
                }
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

    public void LogData(Vector2 p_action)
    {
        float[] __inputs = CalculateInputs();
        float[] __outputs = new float[3];
        __outputs[0] = (float)(p_action.normalized.x / 2 + 0.5);
        __outputs[1] = (float)(p_action.normalized.y / 2 + 0.5);
        __outputs[2] = p_action.magnitude / maxSwipeMagnitude;

        ArtificialAgent.SaveTestData(__inputs, __outputs);
        Debug.Log("Saving Test Data");

        string __log = "";
        __log += "Inputs: ";
        for (int i = 0; i < __inputs.Length; i++)
        {
            __log += __inputs[i] + " / ";
        }
        __log += "Outputs: ";
        for (int i = 0; i < __outputs.Length; i++)
        {
            __log += __outputs[i] + " / ";
        }
        Debug.Log(__log);
    }

    public float[] CalculateInputs()
    {
        float[] __inputs = new float[19];
        Vector2 __distVec = Vector2.zero;
        bool[] __armorState = this.GetComponent<PlayerElements>().Armor_Debuff;
        //we need: distance from other players
        //the state of our armors
        //the result of the 8 wall checkers
        //the state of the floor we're steppiing in
        PlayerController[] __tempObjects = (PlayerController[])FindObjectsOfType(typeof(PlayerController));
        float __nearestPlayer = 1000;
        for (int i = 0; i < __tempObjects.Length; i++)
        {
            if (__tempObjects[i] != this)
            {
                float __newPlayerDist = Vector2.Distance(transform.position, __tempObjects[i].transform.position);
                if (__newPlayerDist < __nearestPlayer)
                {
                    __nearestPlayer = __newPlayerDist;
                    __distVec = __tempObjects[i].transform.position - transform.position;
                }
            }
        }
        __inputs[0] = __distVec.x;
        __inputs[1] = __distVec.y;

        __inputs[2] = __armorState[0] ? 1 : 0;
        __inputs[3] = __armorState[1] ? 1 : 0;
        __inputs[4] = __armorState[2] ? 1 : 0;
        __inputs[5] = __armorState[3] ? 1 : 0;

        LayerMask __mask = LayerMask.GetMask("Walls");

        float[] __rays = new float[8];
        for (int i = 0; i < 8; i++)
        {
            float __angle = ((float)i / 8) * (2 * Mathf.PI);
            RaycastHit2D __hit = Physics2D.Raycast(transform.position, new Vector2(Mathf.Cos(__angle), Mathf.Sin(__angle)), 10f, __mask);
            if (__hit.collider != null)
            {
                //Debug.Log("RaycastHit: " + __hit.collider.name);
                __rays[i] = __hit.distance / 10;
            }
            else
            {
                __rays[i] = 1;
            }
        }
        __inputs[6] = __rays[0];
        __inputs[7] = __rays[1];
        __inputs[8] = __rays[2];
        __inputs[9] = __rays[3];
        __inputs[10] = __rays[4];
        __inputs[11] = __rays[5];
        __inputs[12] = __rays[6];
        __inputs[13] = __rays[7];

        int __floorState = GetComponent<PlayerCollision>().floorState;
        __inputs[14] = __floorState == 0 ? 1 : 0;
        __inputs[15] = __floorState == 1 ? 1 : 0;
        __inputs[16] = __floorState == 2 ? 1 : 0;

        Vector2 __velocity = GetComponent<Rigidbody2D>().velocity;
        __inputs[17] = __velocity.x;
        __inputs[18] = __velocity.y;

        return __inputs;
    }
}
