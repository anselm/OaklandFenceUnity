using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_ShowTargetImage : MonoBehaviour {
    
    public State_Data data;
    public State_ZoomInOnTarget nextState;
    public State_DetectTarget prevState;

    public int debugMinCounter = 10;
    int _counter;
    bool _isPlaying = false;

    int _isPlayingCounter = 0;
	// Use this for initialization
	void OnEnable () {
        
        data.staticImageMeshRenderer.material.mainTexture = data.TextureGet(data.trackableName);
        data.staticFullScreenQuad.aspect = data.trackableAspectRatio;
        data.staticFullScreenQuad.distY = data.staticImageStartDistance;

        data.ShowStaticImage(true);

        data.LoadMovie();

        data.zoomCamera.gameObject.transform.rotation = data.arCamera.transform.rotation;
        data.zoomCamera.gameObject.transform.position = data.arCamera.transform.position;

        data.zoomCamera.enabled = true;
        _counter = debugMinCounter;
        _isPlaying = false;
	}
	
    void Update()
    {
        data.zoomCamera.gameObject.transform.rotation = data.arCamera.transform.rotation;
        data.zoomCamera.gameObject.transform.position = data.arCamera.transform.position;

        // if we loose the target, hide and go back to target seeking mode. 
        if(data.GetActiveTrackables().Count < 1 && _isPlayingCounter == 0)
        {
            // lost the target before we got all setup, bail!
            data.HideStaticImage();
            data.zoomCamera.enabled = false;

            data.mpc.Stop();
            data.mpc.UnLoad();

            this.prevState.enabled = true;
            this.enabled = false;
        }

        if(data.mpc.GetCurrentState() == MediaPlayerCtrl.MEDIAPLAYER_STATE.READY && !_isPlaying)
        {
            data.mpc.OnVideoFirstFrameReady = OnVideoFirstFrameReadyCallback;
            data.mpc.Play();
            _isPlaying = true;
            //nextState.enabled = true;
            //this.enabled = false;
        }

        if(_isPlayingCounter > 0)
        {
            _isPlayingCounter--;

            if(_isPlayingCounter <= 0)
            {
                this.enabled = false;
                this.nextState.enabled = true;
            }
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

    void OnVideoFirstFrameReadyCallback()
    {
        data.mpc.OnVideoFirstFrameReady = null;
        _isPlayingCounter = data.prePlaybVideoFrameCount;
    }


    void UpdateReferences()
    {
        if(data == null) data = GetComponent<State_Data>();
        if(nextState == null) nextState = GetComponent<State_ZoomInOnTarget>();
        prevState = GetComponent<State_DetectTarget>();

    }
}
