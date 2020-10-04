using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring : MonoBehaviour
{

    public float rotation = 10f;
    public float radius = 1f;
    public Transform player;
    private bool resetting;
    private int resetFrames;
    private Transform[] childTransforms;

    private float angle;

    // Start is called before the first frame update
    void Start()
    {
        int children = transform.childCount;
        childTransforms = new Transform[children];
        for (int i = 0; i < children; i++)
        {
            childTransforms[i] = transform.GetChild(i);
        }

        resetting = false;
        resetFrames = 0;
        angle = 0.0f;
        transform.localScale = new Vector3(radius, radius, 1.0f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        angle += (rotation/radius) * Time.deltaTime;

        GetComponent<Rigidbody2D>().MoveRotation(angle);


    }

    public void ResetRing()
    {
        resetting = true;
        angle = 0.0f;
        foreach (Transform child in childTransforms)
        {
            child.gameObject.GetComponent<FreezeBlocks>().SetIsFrozen(false);
            child.parent = child.gameObject.GetComponent<FreezeBlocks>().parentTransform;
            child.transform.localPosition = child.gameObject.GetComponent<FreezeBlocks>().resetTransform.position;
            child.transform.localRotation = child.gameObject.GetComponent<FreezeBlocks>().resetTransform.rotation;
        }
        player.localPosition = player.GetComponent<Entity>().resetTransform.position;
        player.localRotation = player.GetComponent<Entity>().resetTransform.rotation;
    }

    public float GetAngle()
    {
        return angle;
    }
}

public class ResetTransform
{
    public Vector3 position;
    public Quaternion rotation;

    public ResetTransform(Transform resetTransform)
    {
        position = resetTransform.localPosition;

        rotation = resetTransform.localRotation;
    }
}

