using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_ShowPostVideoImage : MonoBehaviour {

    public State_Data data;
    public State_PostVideoUIHandler nextState;

    #region UNITY

	// Use this for initialization
	void OnEnable () {
        Debug.Log("Show Post Image Start", this);

        var tex = data.TextureGet(data.trackableName + State_Data.BUMPERPOSTFIX);
        var ratio = tex.width/(float)tex.height;
        data.staticImageMeshRenderer.material.mainTexture = tex;
        data.staticFullScreenQuad.aspect = ratio;
        data.staticFullScreenQuad.distY = data.staticImageEndDistance;

        data.ShowStaticImage();
        data.HideVideoImage();


        this.enabled = false;
        nextState.enabled = true;
	}
	

    void OnDisable()
    {
        Debug.Log("Show Post Image End", this);

        // andddddd go someplace else!
    }

    void Reset()
    {
        UpdateReferences();
    }
    void OnValidate()
    {
        UpdateReferences();
    }

    #endregion

    void UpdateReferences()
    {
        data = GetComponent<State_Data>();
        nextState = GetComponent<State_PostVideoUIHandler>();
    }
}
