using UnityEngine;
using System.Collections;

public class NeuralNetwork : MonoBehaviour {

    public struct Neuron
    {
        const int BIAS = 1;

        public int inputAmount;
        public float[] inputs;
        public float[] weights; 

        static public Neuron NewNeuron(int numInputs)
        {
            Neuron __tempNeuron = new global::NeuralNetwork.Neuron();

            __tempNeuron.inputAmount = numInputs;
            __tempNeuron.inputs = new float[__tempNeuron.inputAmount];
            __tempNeuron.weights = new float[__tempNeuron.inputAmount + 1];

            return __tempNeuron;
        }

        public void SetNeuronInputs(float[] newInputs)
        {
            inputs = newInputs;
        }

        public void SetNeuronWeights(float[] newWeights)
        {
            inputAmount = newWeights.Length - 1;
            inputs = new float[newWeights.Length - 1];
            weights = newWeights;
        }

        public void RandomizeNeuron()
        {
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = Random.Range(-1f, 1f);
            }
        }

        public float NeuronOutput()
        {
            float sum = 0;
            float sigmoid = 0;

            string ___inputs = "inputs: ";
            for (int i = 0; i < inputs.Length; i++)
            {
                ___inputs += inputs[i] + " / ";
            }
            //Debug.Log(___inputs);

            for (int i = 0; i < inputs.Length; i++)
            {                
                sum += inputs[i] * weights[i];
            }
            sum += BIAS * weights[weights.Length-1];

            //sigmoid fuction
            sigmoid = 1f / (1f + Mathf.Exp(-sum));

            //Debug.Log("Sigmoid: " + sigmoid + "Sum: " + sum);

            return sigmoid;
        }        
    }

    public struct Network
    {
        int inputAmount;
        int outputAmount;
        int hiddenLayersAmount;
        int[] neuronsPerHiddenLayer;

        const float learningRate = 0.01f;

        float[] inputs;
        Neuron[] outputs;
        Neuron[][] hiddenLayers;            

        public Network NewNetwork(int p_inputAmount, int p_outputAmount, int p_hiddenLayersAmount, int[] p_neuronsPerHiddenLayer)
        {
            Network __tempNetwork = new Network();

            __tempNetwork.inputAmount = p_inputAmount;
            __tempNetwork.outputAmount = p_outputAmount;
            __tempNetwork.hiddenLayersAmount = p_hiddenLayersAmount;
            __tempNetwork.neuronsPerHiddenLayer = p_neuronsPerHiddenLayer;
            __tempNetwork.inputs = new float[p_inputAmount];
            __tempNetwork.outputs = new Neuron[p_outputAmount];
            __tempNetwork.hiddenLayers = new Neuron[p_hiddenLayersAmount][];
            for (int i = 0; i < p_hiddenLayersAmount; i++)
            {
                __tempNetwork.hiddenLayers[i] = new Neuron[p_neuronsPerHiddenLayer[i]];
            }

            //now we set up the neurons to have inputs and weights as defined by the network
            int __inputAmount = p_inputAmount;
            for (int i = 0; i < __tempNetwork.hiddenLayers.Length; i++)
            {
                for (int j = 0; j < __tempNetwork.hiddenLayers[i].Length; j++)
                {
                    __tempNetwork.hiddenLayers[i][j] = Neuron.NewNeuron(__inputAmount);
                }
                __inputAmount = p_neuronsPerHiddenLayer[i];
            }
            for (int i = 0; i < __tempNetwork.outputs.Length; i++)
            {
                __tempNetwork.outputs[i] = Neuron.NewNeuron(__inputAmount);
            }

            return __tempNetwork;
        }

        public void RandomizeNetwork()
        {
            for (int i = 0; i < hiddenLayers.Length; i++)
            {
                for (int j = 0; j < hiddenLayers[i].Length; j++)
                {
                    hiddenLayers[i][j].RandomizeNeuron();
                }
            }
            for (int i = 0; i < outputs.Length; i++)
            {
                outputs[i].RandomizeNeuron();
            }
        }

        public void SetInputs(float[] p_inputs)
        {
            inputs = p_inputs;
        }

        public void SaveNeurons()
        {
            for (int i = 0; i < hiddenLayers.Length; i++)
            {
                for (int j = 0; j < hiddenLayers[i].Length; j++)
                {                   
                    for (int k = 0; k < hiddenLayers[i][j].weights.Length; k++)
                    {
                        string __key = "Layer: " + i.ToString() + ", Neuron: " + j.ToString() + ", Weight: " + k.ToString();
                        Debug.Log(__key + " " +hiddenLayers[i][j].weights[k]);
                        PlayerPrefs.SetFloat(__key, hiddenLayers[i][j].weights[k]);
                    }
                }
            }
            for (int i = 0; i < outputs.Length; i++)
            {               
                for (int j = 0; j < outputs[i].weights.Length; j++)
                {
                    string __key = "Layer: Output, Neuron: " + i.ToString() + ", Weight: " + j.ToString();
                    PlayerPrefs.SetFloat(__key, outputs[i].weights[j]);
                }
            }
        }
        public void LoadNeurons()
        {
            for (int i = 0; i < hiddenLayers.Length; i++)
            {
                for (int j = 0; j < hiddenLayers[i].Length; j++)
                {
                    for (int k = 0; k < hiddenLayers[i][j].weights.Length; k++)
                    {
                        string __key = "Layer: " + i.ToString() + ", Neuron: " + j.ToString() + ", Weight: " + k.ToString();
                        hiddenLayers[i][j].weights[k] = PlayerPrefs.GetFloat(__key);
                    }
                }
            }
            for (int i = 0; i < outputs.Length; i++)
            {
                for (int j = 0; j < outputs[i].weights.Length; j++)
                {
                    string __key = "Layer: Output, Neuron: " + i.ToString() + ", Weight: " + j.ToString();
                    outputs[i].weights[j] = PlayerPrefs.GetFloat(__key);
                }
            }
        }

        public float[] NetworkOutput()
        {
            float[] __output = new float[outputs.Length];

            float[] __oldLayerOutputs = inputs;
            float[] __newLayerOutputs;
            for (int i = 0; i < hiddenLayers.Length; i++)
            {
                __newLayerOutputs = new float[neuronsPerHiddenLayer[i]];
                for (int j = 0; j < hiddenLayers[i].Length; j++)
                {
                    hiddenLayers[i][j].SetNeuronInputs(__oldLayerOutputs);
                    __newLayerOutputs[j] = hiddenLayers[i][j].NeuronOutput();
                }
                __oldLayerOutputs = __newLayerOutputs;
            }
            for (int i = 0; i < outputs.Length; i++)
            {
                outputs[i].SetNeuronInputs(__oldLayerOutputs);
                __output[i] = outputs[i].NeuronOutput();
            }

            return __output;
        }

        public void Backpropagation(float[] p_inputs, float[] p_outputs)
        {
            //do forward propagation and save each Nauron's output
            float[] __output = new float[outputs.Length];
            float[][] __hiddenLayersOutputs = new float[hiddenLayersAmount][];

            float[] __oldLayerOutputs = inputs;
            float[] __newLayerOutputs;
            for (int i = 0; i < hiddenLayers.Length; i++)
            {
                __newLayerOutputs = new float[neuronsPerHiddenLayer[i]];
                for (int j = 0; j < hiddenLayers[i].Length; j++)
                {
                    hiddenLayers[i][j].SetNeuronInputs(__oldLayerOutputs);
                    __newLayerOutputs[j] = hiddenLayers[i][j].NeuronOutput();
                }
                __hiddenLayersOutputs[i] = __newLayerOutputs;
                __oldLayerOutputs = __newLayerOutputs;
            }
            for (int i = 0; i < outputs.Length; i++)
            {
                outputs[i].SetNeuronInputs(__oldLayerOutputs);
                __output[i] = outputs[i].NeuronOutput();
            }

            //now we analyze the error amount and do the backpropagation
            float[] __finalOutputErrors = new float[p_outputs.Length];
            float[][] __hiddenLayersErrors = new float[hiddenLayersAmount][];
            for (int i = 0; i < __hiddenLayersErrors.Length; i++)
            {
                __hiddenLayersErrors[i] = new float[neuronsPerHiddenLayer[i]];
            }
            //Debug.Log("Errors: " + (p_outputs[0] - __output[0]) + " " + (p_outputs[1] - __output[1]) + " " + (p_outputs[2] - __output[2]));
            
            //calculate errors for output layer
            for (int i = 0; i < __finalOutputErrors.Length; i++)
            {
                __finalOutputErrors[i] = __output[i] * (1 - __output[i]) * (p_outputs[i] - __output[i]);
            }
            //change output layer weights
            for (int i = 0; i < outputs.Length; i++)
            {
                for (int j = 0; j < outputs[i].weights.Length; j++)
                {
                    if (j < outputs[i].weights.Length - 1)
                    {
                        //Debug.Log(outputs.Length + " " + i + " / " + outputs[i].weights.Length + " " + j + " / " + __finalOutputErrors.Length + " " + i + " / " + __hiddenLayersOutputs.Length + " " + (__hiddenLayersOutputs.Length - 1) + " / " + __hiddenLayersOutputs[__hiddenLayersOutputs.Length - 1].Length + " " + j);
                        outputs[i].weights[j] = outputs[i].weights[j] + (learningRate * __finalOutputErrors[i] * __hiddenLayersOutputs[__hiddenLayersOutputs.Length - 1][j]);
                    }
                    else
                    {
                        outputs[i].weights[j] = outputs[i].weights[j] + (learningRate * __finalOutputErrors[i] * 1);                        
                    }
                }
            }

            //calculate the erros for the hiden layers
            for (int i = (hiddenLayersAmount-1); i >= 0; i--)
            {
                for (int j = 0; j < neuronsPerHiddenLayer[i]; j++)
                {
                    float __result = __hiddenLayersOutputs[i][j] * (1 - __hiddenLayersOutputs[i][j]);
                    float __nextLayerErrorSum = 0;
                    if (i == (hiddenLayersAmount-1))
                    {
                        for (int k = 0; k < outputs.Length; k++)
                        {
                            __nextLayerErrorSum += __finalOutputErrors[k] * outputs[k].weights[j];
                        }
                    }
                    else
                    {
                        for (int k = 0; k < hiddenLayers[i+1].Length; k++)
                        {
                            __nextLayerErrorSum += __hiddenLayersErrors[i + 1][k] * hiddenLayers[i + 1][k].weights[j];
                        }
                    }

                    __hiddenLayersErrors[i][j] = __result * __nextLayerErrorSum;
                }
                //change the weights for the hidden layer
                for (int j = 0; j < hiddenLayers[i].Length; j++)
                {
                    for (int k = 0; k < hiddenLayers[i][j].weights.Length; k++)
                    {
                        float __inputValue;
                        if (k < hiddenLayers[i][j].weights.Length - 1)
                        {
                            if (i != 0)
                            {
                                __inputValue = __hiddenLayersOutputs[i - 1][k];
                            }
                            else
                            {
                                __inputValue = p_inputs[k];
                            }
                        }
                        else
                        {
                            __inputValue = 1;
                        }
                        hiddenLayers[i][j].weights[k] = hiddenLayers[i][j].weights[k] + (learningRate * __hiddenLayersErrors[i][j] * __inputValue);
                    }
                }
            }
        }

        public float GetError(float[] p_inputs, float[] p_outputs)
        {
            float[] __output = new float[outputs.Length];
            float[][] __hiddenLayersOutputs = new float[hiddenLayersAmount][];

            float[] __oldLayerOutputs = inputs;
            float[] __newLayerOutputs;
            for (int i = 0; i < hiddenLayers.Length; i++)
            {
                __newLayerOutputs = new float[neuronsPerHiddenLayer[i]];
                for (int j = 0; j < hiddenLayers[i].Length; j++)
                {
                    hiddenLayers[i][j].SetNeuronInputs(__oldLayerOutputs);
                    __newLayerOutputs[j] = hiddenLayers[i][j].NeuronOutput();
                }
                __hiddenLayersOutputs[i] = __newLayerOutputs;
                __oldLayerOutputs = __newLayerOutputs;
            }
            for (int i = 0; i < outputs.Length; i++)
            {
                outputs[i].SetNeuronInputs(__oldLayerOutputs);
                __output[i] = outputs[i].NeuronOutput();
            }

            float __totalError = 0;
            for (int i = 0; i < outputs.Length; i++)
            {
                __totalError += p_outputs[i] - __output[i];
            }

            return __totalError / outputs.Length;
        }

    }
}
