using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullScreenQuad : MonoBehaviour {

    public float distY = 10f;

    public Camera cam;

    public bool updatePosition = true;
    public bool updateRotation = true;
    public float aspect = 1;
    public bool invertScale = false;

    Vector3 _scale = new Vector3();

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        float pos = distY - cam.nearClipPlane;

        if(updatePosition) transform.position = cam.transform.position + cam.transform.forward * pos;

        float h = Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * 0.5f) * pos * 2f;

        if(updateRotation) transform.LookAt(cam.transform);

        _scale.x = h * cam.aspect * (invertScale ? -1 : 1);
        _scale.y = 0.1f;
        _scale.z = _scale.x/aspect;

        transform.localScale = _scale;//new Vector3(h * cam.aspect, (h * cam.aspect)/aspect, .1f);

	}
}
