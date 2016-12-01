using UnityEngine;
using System.Collections;

public class ParticleKiller : MonoBehaviour {

    public float duration;
    public bool randomColored = false;
    public bool armorColored = false;
    public bool bodyColored = false;
    public Color bodyColor;
    public Color armorColor;

    private float counter = 0;
      
    void Start()
    {
        if (randomColored)
        {
            Color __randColor = LobbyGUI.HSVtoRGB((int)((Random.Range(0f, 1f)) * 360), 1, 1);
            GetComponent<ParticleSystem>().startColor = __randColor;
            ParticleSystem[] __temp = GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < __temp.Length; i++)
            {
                __temp[i].startColor = __randColor;
            }
        }
        else if (armorColored)
        {
            //this.GetComponent<ParticleSystem>().startColor = armorColor;
            ParticleSystem[] __temp = GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < __temp.Length; i++)
            {
                __temp[i].startColor = armorColor;
            }
        }
        else if (bodyColored)
        {
            //this.GetComponent<ParticleSystem>().startColor = bodyColor;
            ParticleSystem[] __temp = GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < __temp.Length; i++)
            {
                __temp[i].startColor = bodyColor;
            }
        }


        GetComponent<ParticleSystemRenderer>().sortingOrder = 5;
        this.GetComponent<ParticleSystem>().Play(true);
    }
	void Update()
    {
        counter += Time.deltaTime;
        if(counter >= duration)
        {
            Destroy(gameObject);
        }
    }
}
