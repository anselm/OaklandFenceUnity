using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31;

[RequireComponent(typeof(State_Data))]
public class State_Setup : MonoBehaviour {

    [SerializeField]
    State_Data data;
    [SerializeField]
    State_DetectTarget nextState;

    float _startTime = 0;
    bool _finishedSetup = false;

    void Start()
    {
        Invoke("StartHelper", 0.1f);
        _startTime = Time.time;
	}

    void Update()
    {
        if(Time.time - _startTime > data.minStartingImageTime)
        {
            this.enabled = false;
        }
    }

    void OnDisable()
    {
        nextState.enabled = true;
    }

    void StartHelper()
    {

        data.SetupCompleteEvent += Data_SetupCompleteEvent;
        data.SetupData();

        data.zoomCamera.gameObject.transform.position = data.endPosition.position;
        data.zoomCamera.gameObject.transform.rotation = data.endPosition.rotation;
        data.zoomCamera.enabled = true;

        data.staticImageMeshRenderer.material.mainTexture = data.startingTexture;

        data.staticFullScreenQuad.aspect = data.startingTexture.width/(float)data.startingTexture.height;
        data.staticFullScreenQuad.distY = data.staticImageEndDistance;

        data.staticImageMeshRenderer.enabled = true;
        data.blocker.SetActive(true);

        _startTime = Time.time;

    }


    void Data_SetupCompleteEvent ()
    { 
        Debug.Log("Finished Caching all the images.", this);
        _finishedSetup = true;
    }

    void Reset()
    {
        data = GetComponent<State_Data>();
        nextState = GetComponent<State_DetectTarget>();
    }
}
