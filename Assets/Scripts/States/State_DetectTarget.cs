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
    StateManager sm;


    [SerializeField]
    bool breakOnDetection = false;

    void OnEnable()
    {
        data.Reset();
        sm = TrackerManager.Instance.GetStateManager();
    }
	
	// Update is called once per frame
	void Update () {

        data.zoomCamera.fieldOfView = data.arCamera.fieldOfView;

        activeTrackables = (IList<TrackableBehaviour>) sm.GetActiveTrackableBehaviours();

        if(activeTrackables.Count < 1) return;

        var trackable = activeTrackables[0];

        var size = trackable.GetComponent<ImageTargetAbstractBehaviour>().GetSize();


        var image = data.TextureGet(trackable.TrackableName);

        Debug.Log("w,d " + image.width + ", " + image.height + " " + (image.width/(float)image.height), this);

        data.trackableAspectRatio = image.width/(float)image.height;
        data.trackableName = trackable.TrackableName;

        if(breakOnDetection) Debug.Break();

        this.enabled = false;
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
