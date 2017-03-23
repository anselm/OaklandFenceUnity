using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class State_DetectTarget : MonoBehaviour {

    [SerializeField]
    State_Data data;
    [SerializeField]
    State_ShowTargetImage nextState;

    IList<TrackableBehaviour> activeTrackables;
    //StateManager sm;


    [SerializeField]
    bool breakOnDetection = false;


    int minFramesOfDetection = 10;
    int _counter;

    void OnEnable()
    {
        data.Reset();
        //sm = TrackerManager.Instance.GetStateManager();
    }
	
	// Update is called once per frame
	void Update () {

        data.zoomCamera.fieldOfView = data.arCamera.fieldOfView;

        activeTrackables = data.GetActiveTrackables();// (IList<TrackableBehaviour>) sm.GetActiveTrackableBehaviours();

        if(activeTrackables.Count < 1) return;

        var trackable = activeTrackables[0];

        if(trackable.TrackableName == data.trackableName)
        {
            _counter--;
            if(_counter < 0)
            {
                this.enabled = false;
            }
            return;
        }
        else
        {
            _counter = minFramesOfDetection;
        }

        var image = data.TextureGet(trackable.TrackableName);

        Debug.Log("w,d " + image.width + ", " + image.height + " " + (image.width/(float)image.height), this);

        data.trackableAspectRatio = image.width/(float)image.height;
        data.trackableName = trackable.TrackableName;

//        if(breakOnDetection) Debug.Break();
//
//        this.enabled = false;
	}

    void OnDisable()
    {
        nextState.enabled = true;
    }

    void Reset()
    {
        data = GetComponent<State_Data>();
        nextState = GetComponent<State_ShowTargetImage>();
    }

    void OnValidate()
    {
        Reset();
    }
}
