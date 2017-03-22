using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomCamera : MonoBehaviour {


    public event System.Action ZoomCompleteEvent;

    public Camera cam;
    public Transform startingPosition;
    public Transform endingPosition;
     
    public int framesForZoom = 100;


    Camera thisCamera;

	// Use this for initialization
	void Start () {
        thisCamera = GetComponent<Camera>();
	}
	
    void Update()
    {
        thisCamera.fieldOfView = cam.fieldOfView;
    }

    public void StartZoomCamera()
    {
        StartCoroutine(MoveToEndingPosition());
    }

    IEnumerator MoveToEndingPosition()
    {
        //cam.enabled = false;
        var startRot = startingPosition.rotation;
        var startPos = startingPosition.position;

        for(int i = 1; i < framesForZoom + 1; i++)
        {
            this.transform.rotation = Quaternion.Slerp(startRot, endingPosition.rotation, i/(float)framesForZoom);
            this.transform.position = Vector3.Lerp(startPos, endingPosition.position, i/(float)framesForZoom);
            yield return null;
        }

        this.transform.rotation = endingPosition.rotation;
        this.transform.position = endingPosition.position;

        OnZoomCompleteEvent();
    }

    void OnZoomCompleteEvent()
    {
        if(ZoomCompleteEvent != null) ZoomCompleteEvent.Invoke();
    }
}
