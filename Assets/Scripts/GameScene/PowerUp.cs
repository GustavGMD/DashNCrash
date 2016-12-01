using UnityEngine;
using System.Collections;

public class PowerUp : MonoBehaviour {

    public enum PowerUpType
    {
        DAMAGE,
        SHIELD
    };

    public PowerUpType type;

    //public GameObject[] visualEffect;    
}
