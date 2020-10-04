using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CamMovement : MonoBehaviour
{

    public float zoomRate;
    public float offset;

    private float desiredOrthographicSize;
    private CinemachineVirtualCamera vcam; 

    // Start is called before the first frame update
    void Start()
    {
        var brain =  GetComponent<CinemachineBrain>();
        vcam = (brain == null) ? null : brain.ActiveVirtualCamera as CinemachineVirtualCamera;
        if (vcam != null)
            desiredOrthographicSize = vcam.m_Lens.OrthographicSize;
        else
            Debug.LogError("Please stop!");


    }

    // Update is called once per frame
    void Update()
    {
        if (desiredOrthographicSize > vcam.m_Lens.OrthographicSize + offset || desiredOrthographicSize < vcam.m_Lens.OrthographicSize - offset)
        {
            float rate = (desiredOrthographicSize < vcam.m_Lens.OrthographicSize) ? -zoomRate : zoomRate;
            vcam.m_Lens.OrthographicSize += rate * Time.deltaTime;
        }
        else
        {
            vcam.m_Lens.OrthographicSize = desiredOrthographicSize;
        }
    }

    public float GetDesiredOrthographicSize()
    {
        return desiredOrthographicSize;
    }

    public void SetDesiredOrthographicSize(float size)
    {
        desiredOrthographicSize = size;
    }
}
