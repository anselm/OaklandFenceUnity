using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_ShowMovie : MonoBehaviour {

    [SerializeField]
    State_Data data;
    [SerializeField]
    State_ShowPostVideoImage nextState;

    int alphaCounter = 0;

    #region UNITY

	// Use this for initialization
	void OnEnable () {
        var currentState = data.mpc.GetCurrentState();

        Debug.Log("Show Movie State Start " + currentState, this);

        data.mpc.OnVideoFirstFrameReady = OnVideoFirstFrameReadyCallback;

        if(currentState == MediaPlayerCtrl.MEDIAPLAYER_STATE.READY)
        {
            OnReadyCallback();
        }
        else
        {
            if(currentState == MediaPlayerCtrl.MEDIAPLAYER_STATE.STOPPED || 
                currentState == MediaPlayerCtrl.MEDIAPLAYER_STATE.PAUSED)
            {
                Debug.Log("Setup for replay");
                data.mpc.SeekTo(0);
                data.mpc.Stop();

                OnReadyCallback();
                Invoke("OnVideoFirstFrameReadyCallback", 1f);
            }
            else
            {
                data.mpc.OnReady = OnReadyCallback;
            }
        }

	}
	

    void Update()
    {
        if(alphaCounter > 0)
        {
            alphaCounter--;
            var colour = data.staticImageMeshRenderer.material.color;
            colour.a = alphaCounter/(float)data.framesForAlphaFade;
            data.staticImageMeshRenderer.material.color = colour;
            if(alphaCounter == 0)
            {
                data.staticImageMeshRenderer.enabled = false;
            }
        }    
    }


    void OnDisable()
    {
        Debug.Log("Show Movie End", this);

        CancelInvoke();

        data.mpc.OnVideoFirstFrameReady = null;
        data.mpc.OnReady = null;
        data.mpc.OnEnd = null;

        data.staticImageMeshRenderer.material.color = Color.white;

        data.blocker.SetActive(true);
        data.videoSkipGameObject.SetActive(false);

        nextState.enabled = true;
    }


    void OnValidate()
    {
        UpdateReferences();
    }

    void Reset()
    {
        UpdateReferences();
    }

    #endregion

    public void SkipButtonClickHandler()
    {
    
        if(!this.enabled) return;

        Debug.Log("Skip Button Clicked", this);
        OnEndCallback();
    }


    void OnVideoFirstFrameReadyCallback()
    {
        if(!this.enabled) return;

        Debug.Log("Video First Frame Ready", this);
        data.mpc.OnVideoFirstFrameReady = null;
        data.videoImageMeshRenderer.enabled = true;
        alphaCounter = data.framesForAlphaFade;

        Invoke("ShowBlocker", data.delayToShowBlocker);

        data.videoSkipGameObject.SetActive(true);
    }

    void OnReadyCallback()
    {
        if(!this.enabled) return;

        Debug.Log("Video Ready.", this);

        data.mpc.OnReady = null;
        data.mpc.OnEnd = OnEndCallback;
        data.videoFullScreenQuad.aspect = (float)data.mpc.GetVideoWidth()/(float)data.mpc.GetVideoHeight();
        data.mpc.Play(); 

        // TODO remove, here for debugging
        //Invoke("OnEndCallback", 8f);
    }

    void OnEndCallback()
    {
        if(!this.enabled) return;

        Debug.Log("Video Finished", this);
        data.mpc.Stop();
        this.enabled = false;
    }

    void ShowBlocker()
    {
        if(!this.enabled) return;

        data.blocker.SetActive(true);    
    }

    void UpdateReferences()
    {
        if(data == null) data = GetComponent<State_Data>();
        if(nextState == null) nextState = GetComponent<State_ShowPostVideoImage>();

    }
}
