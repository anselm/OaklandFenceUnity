using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_ShowTargetImage : MonoBehaviour {
    
    public State_Data data;
    public State_ZoomInOnTarget nextState;
    public State_DetectTarget prevState;

    public int debugMinCounter = 10;
    int _counter;

	// Use this for initialization
	void OnEnable () {
        
        data.staticImageMeshRenderer.material.mainTexture = data.TextureGet(data.trackableName);
        data.staticFullScreenQuad.aspect = data.trackableAspectRatio;
        data.staticFullScreenQuad.distY = data.staticImageStartDistance;
        data.staticImageMeshRenderer.enabled = true;

        data.LoadMovie();

        data.zoomCamera.gameObject.transform.rotation = data.arCamera.transform.rotation;
        data.zoomCamera.gameObject.transform.position = data.arCamera.transform.position;

        data.zoomCamera.enabled = true;
        _counter = debugMinCounter;
	}
	
    void Update()
    {
        data.zoomCamera.gameObject.transform.rotation = data.arCamera.transform.rotation;
        data.zoomCamera.gameObject.transform.position = data.arCamera.transform.position;

        // if we loose the target, hide and go back to target seeking mode. 
        if(data.GetActiveTrackables().Count < 1)
        {
            // lost the target before we got all setup, bail!

            data.staticImageMeshRenderer.enabled = false;
            data.zoomCamera.enabled = false;


            this.prevState.enabled = true;
            this.enabled = false;
        }

        if(data.mpc.GetCurrentState() == MediaPlayerCtrl.MEDIAPLAYER_STATE.READY || _counter-- < 0)
        {
            nextState.enabled = true;
            this.enabled = false;
        }
    }

    void OnDisable()
    {
    }

    void OnValidate()
    {
        UpdateReferences();
    }

    void Reset()
    {
        UpdateReferences();
    }

    void UpdateReferences()
    {
        if(data == null) data = GetComponent<State_Data>();
        if(nextState == null) nextState = GetComponent<State_ZoomInOnTarget>();
        prevState = GetComponent<State_DetectTarget>();

    }
}
