using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopClock : MonoBehaviour
{
    public float maximum;
    public float current;

    public Ring level;
    public Image mask;
    public Sprite[] sprites;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        current = level.GetAngle(); 
        if (current < 0.0f)
        {
            transform.GetChild(0).GetComponent<Image>().fillClockwise = true;
        }
        else 
        {
            transform.GetChild(0).GetComponent<Image>().fillClockwise = false;
        }

        current = Mathf.Abs(current);
        GetCurrentFill();
    }

    void GetCurrentFill()
    {
        float fillAmount = current / maximum;
        mask.fillAmount = fillAmount;
        if (fillAmount > 1.0f)
        {
            FindObjectOfType<AudioManager>().Play("Lose");
            level.ResetRing(this);
        }
    }
}
