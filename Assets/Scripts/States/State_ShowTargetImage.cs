using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_ShowTargetImage : MonoBehaviour {
    
    public State_Data data;
    public State_ZoomInOnTarget nextState;

	// Use this for initialization
	void OnEnable () {
        data.staticImageMeshRenderer.material.mainTexture = data.TextureGet(data.trackableName);
        data.staticFullScreenQuad.aspect = data.trackableAspectRatio;
	}
	
    void Update()
    {
        this.enabled = false;
    }

    void OnDisable()
    {
        data.staticImageMeshRenderer.enabled = true;
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

    void UpdateReferences()
    {
        if(data == null) data = GetComponent<State_Data>();
        if(nextState == null) nextState = GetComponent<State_ZoomInOnTarget>();

    }
}
