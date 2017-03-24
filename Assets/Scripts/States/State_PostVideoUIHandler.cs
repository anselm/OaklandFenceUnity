using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Prime31;

public class State_PostVideoUIHandler : MonoBehaviour {

    public State_Data data;
    public State_DetectTarget homeState;
    public State_ShowMovie showMovieState;
    #region UNITY

	// Use this for initialization
	void OnEnable () {
        Debug.Log("Video UI Handler Start", this);
        data.postVideoUIGameObject.GetComponent<AspectRatioFitter>().aspectRatio = data.staticFullScreenQuad.aspect;
        data.postVideoUIGameObject.SetActive(true);

	}

    void OnDisable()
    {
        Debug.Log("Video UI Handler End", this);
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

    #region UI CALLBACKS

    public void SupportButtonClickHandler()
    {
        if(!this.enabled) return;
        WebViewHelper("support");
        Debug.Log("Support", this);
    }

    public void LearnMoreButtonClickHandler()
    {
        if(!this.enabled) return;
        WebViewHelper(data.trackableName);
        Debug.Log("Learn", this);
    }

    public void JoinButtonClickHandler()
    {
        if(!this.enabled) return;
        WebViewHelper("join");
        Debug.Log("Join", this);
    }

    public void HomeButtonClickHandler()
    {
        if(!this.enabled) return;

        // Clear everything from the screen.. .
        Debug.Log("Home", this);

        // hide everything. 
        //data.Reset();
        data.HideVideoImage();
        data.HideStaticImage();
        data.HideBlockerImage();

        data.postVideoUIGameObject.SetActive(false);

        homeState.enabled = true;

        this.enabled = false;
    }


    public void ReplayButtonClickHandler()
    {
        if(!this.enabled) return;

        Debug.Log("Replay", this);

        data.HideStaticImage(false);
        data.postVideoUIGameObject.SetActive(false);

        showMovieState.enabled = true;
        this.enabled = false;
    }

    #endregion

    void WebViewHelper(string loc)
    {
        string url = string.Format("http://{0}/{1}.html", data.serverName, loc);
        Debug.Log("URL: " + url, this);
        EtceteraAndroid.showWebView(url);
    }

    void UpdateReferences()
    {
        data = GetComponent<State_Data>();
        homeState = GetComponent<State_DetectTarget>();
        showMovieState = GetComponent<State_ShowMovie>();
    }
}
