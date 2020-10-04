using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeBlocks : MonoBehaviour
{

    private bool isFrozen;
    public Transform parentTransform;

    public ResetTransform resetTransform;

    // Start is called before the first frame update
    void Start()
    {
        resetTransform = new ResetTransform(transform);
        parentTransform = transform.parent;
        SetIsFrozen(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (GetIsFrozen())
        {
            GetComponent<SpriteRenderer>().color = Color.grey;
            transform.parent = null;
        }
        else if (!GetIsFrozen() && gameObject.GetComponent<BoxCollider2D>().enabled)
        {
            GetComponent<SpriteRenderer>().color = new Color(0.05490196f, 0.5950408f, 0.8784314f, 1.0f);
            transform.parent = parentTransform;
        }
    }

    public bool GetIsFrozen()
    {
        return isFrozen;
    }

    public void SetIsFrozen(bool frozen)
    {
        isFrozen = frozen;
    }
}
