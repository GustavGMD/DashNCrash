using UnityEngine;
using System.Collections;

public class ArtificialAgent : MonoBehaviour {

    public struct TestData
    {
        public float[] inputs;
        public float[] outputs;
    }

    public Vector2 distanteToClosestEnemy;
    public bool[] myArmorState = new bool[4];
    public float[] wallProximitySensors = new float[8];
    public bool[] floorState = new bool[3]; //regular, slow, slide

    public NeuralNetwork.Network brain;
    public TestData[] allTestData;

    public static int testDataAmount = 0;
    public int currentTrainIndex = 0;
    public bool trainingEnabled = false;

	// Use this for initialization
	void Start () {
        brain = brain.NewNetwork(19, 3, 2, new int[] { 10, 10 });
        //brain.RandomizeNetwork();
        //brain.SaveNeurons();
        brain.LoadNeurons();
        LoadTestDataAmount();
        allTestData = new TestData[testDataAmount];
        for (int i = 0; i < testDataAmount; i++)
        {
            allTestData[i] = LoadTestData(i);
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            Debug.Log("Randomizing Network Weights...");
            brain.RandomizeNetwork();
        }
        else if(Input.GetKeyUp(KeyCode.Alpha2))
        {
            Debug.Log("Saving Network Neurons... This shouldn't be used with many ArtificialAgents instances in the scene!");
            brain.SaveNeurons();
        }
        else if(Input.GetKeyUp(KeyCode.Alpha3))
        {
            Debug.Log("Loading Network Neurons...");
            brain.LoadNeurons();
            Debug.Log("Loaded network Error: " + CalculateTotalError());
        }
        else if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            Debug.Log("Starting Training!");
            trainingEnabled = true;
        }
        else if (Input.GetKeyUp(KeyCode.Alpha5))
        {
            Debug.Log("PausingTraining!");
            trainingEnabled = false;
        }
        else if (Input.GetKeyUp(KeyCode.Alpha6))
        {
            Debug.Log("Training Sessions so far: " + currentTrainIndex);
        }
        else if (Input.GetKeyUp(KeyCode.Alpha7))
        {
            Debug.Log("Current Error: " + CalculateTotalError());
        }
        else if (Input.GetKeyUp(KeyCode.Alpha8))
        {
            Debug.Log("Randomizing network...");
            brain.RandomizeNetwork();
        }

        //Do BackPropagation training
        if (trainingEnabled)
        {
            for (int i = 0; i < testDataAmount; i++)
            {
                brain.Backpropagation(allTestData[i].inputs, allTestData[i].outputs);
            }
            currentTrainIndex++;
            Debug.Log("Finished another training cicle! Current error: " + Mathf.Abs(CalculateTotalError()));
        }

    }

    public Vector2 AgentOutput()
    {
        float[] __networkOutput = brain.NetworkOutput();        
        Vector2 __result = new Vector2(((__networkOutput[0] - 0.5f) *2), ((__networkOutput[1] - 0.5f) * 2)).normalized * __networkOutput[2];

        //Debug.Log("Outputs: " + ((__networkOutput[0] - 0.5f) * 2) + " / " + ((__networkOutput[1] - 0.5f) * 2) + " / " + __networkOutput[2]);

        return __result;
    }

    public static void SaveTestData(float[] p_inputs, float[] p_outputs)
    {
        string __key = "TestData[" + testDataAmount + "]_";
        for (int i = 0; i < p_inputs.Length; i++)
        {
            PlayerPrefs.SetFloat(__key + "input[" + i + "]", p_inputs[i]);
        }
        for (int i = 0; i < p_outputs.Length; i++)
        {
            PlayerPrefs.SetFloat(__key + "output[" + i + "]", p_outputs[i]);
        }

        testDataAmount++;
        SaveTestDataAmount();
    }
    public static TestData LoadTestData(int dataIndex)
    {
        TestData __data = new TestData();

        string __key = "TestData[" + dataIndex + "]_";
        float[] __inputs = new float[19];
        float[] __outputs = new float[3];

        for (int i = 0; i < 19; i++)
        {
            __inputs[i] = PlayerPrefs.GetFloat(__key + "input[" + i + "]");
        }
        for (int i = 0; i < 3; i++)
        {
            __outputs[i] = PlayerPrefs.GetFloat(__key + "output[" + i + "]");
        }

        __data.inputs = __inputs;
        __data.outputs = __outputs;

        return __data;
    }

    public static void SaveTestDataAmount()
    {
        Debug.Log("TestData amount saved: " + testDataAmount);
        string __key = "testDataAmount";
        PlayerPrefs.SetInt(__key, testDataAmount);
    }
    public static void LoadTestDataAmount()
    {
        string __key = "testDataAmount";
        if (!PlayerPrefs.HasKey(__key)) PlayerPrefs.SetInt(__key, 0);
        testDataAmount = PlayerPrefs.GetInt(__key);
    }

    public float CalculateTotalError()
    {
        float __totalError = 0;

        for (int i = 0; i < testDataAmount; i++)
        {
            __totalError += brain.GetError(allTestData[i].inputs, allTestData[i].outputs);
        }

        return __totalError / testDataAmount;
    }
}
