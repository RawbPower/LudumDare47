using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode()]
public class LoopClock : MonoBehaviour
{
    public float maximum;
    public float current;

    public Ring level;
    public Image mask;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        current = Mathf.Abs(level.GetAngle()); 
        GetCurrentFill();
    }

    void GetCurrentFill()
    {
        float fillAmount = current / maximum;
        mask.fillAmount = fillAmount;
        if (fillAmount > 1.0f)
        {
            level.ResetRing();
        }
    }
}
