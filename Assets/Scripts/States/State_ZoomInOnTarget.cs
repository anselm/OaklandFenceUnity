using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_ZoomInOnTarget : MonoBehaviour {

    [SerializeField]
    State_Data data;
    [SerializeField]
    State_ShowMovie nextState;

    int _currentFrames = 0;
    Quaternion _startRot;
    Vector3 _startPos;
    float _currentPercentage;


	// Use this for initialization
	void OnEnable () {
		// Update Zoomable Camera to AR Camera position
        // Update Aspect Ratio for Static Image Mesh
        // Turn on Static Image Mesh
        Debug.Log("Zoom In State Start", this);

        _startRot = data.arGameObject.transform.rotation;
        _startPos = data.arGameObject.transform.position;
        data.staticFullScreenQuad.distY = data.staticImageStartDistance;

        data.zoomCamera.gameObject.transform.rotation = _startRot;
        data.zoomCamera.gameObject.transform.position = _startPos;

        data.LoadMovie();

        _currentFrames = 0;
	}
	
	// Update is called once per frame
	void Update () {

        _currentPercentage = _currentFrames/(float)data.framesForZoom;

        data.zoomCamera.gameObject.transform.rotation = Quaternion.Slerp(_startRot, data.endPosition.rotation, _currentPercentage);
        data.zoomCamera.gameObject.transform.position = Vector3.Lerp(_startPos, data.endPosition.position, _currentPercentage);
        data.staticFullScreenQuad.distY = Mathf.Lerp(data.staticImageStartDistance, data.staticImageEndDistance, _currentPercentage);
        data.videoFullScreenQuad.distY = data.staticFullScreenQuad.distY + data.videoImageDistanceOffset;

        // Get to the video playback asap.
        if(data.mpc.GetCurrentState() == MediaPlayerCtrl.MEDIAPLAYER_STATE.READY)
        {
            this.nextState.enabled = true;
        }

        _currentFrames++;

        if(_currentFrames >= data.framesForZoom) this.enabled = false;
	}

    void OnDisable()
    {
        data.zoomCamera.gameObject.transform.rotation = data.endPosition.rotation;
        data.zoomCamera.gameObject.transform.position = data.endPosition.position;
        data.staticFullScreenQuad.distY = data.staticImageEndDistance;
        data.videoFullScreenQuad.distY = data.staticFullScreenQuad.distY + 10;

        //data.blocker.SetActive(true);

        this.nextState.enabled = true;

        Debug.Log("Zoom In State End", this);
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
        if(nextState == null) nextState = GetComponent<State_ShowMovie>();
        
    }
}
