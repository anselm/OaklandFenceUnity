using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

using System.Xml;

public class State_Data : MonoBehaviour {

    public event System.Action SetupCompleteEvent;

    [Header("URLS")]
    public string serverName = "oaklandfenceproject.org.s3-website-us-west-1.amazonaws.com";
    public string serverName2 = "s3-us-west-1.amazonaws.com/oaklandfenceproject.org";


    [Header("Current Trackable")]
    public string trackableName;
    public float trackableAspectRatio;
    public const string BUMPERPOSTFIX = "-bumper"; // Don't change this! Will break existing cached references. 
    public const string PRECACHE = "preCache"; // For images that ship with the apk

    [Header("Meshs")]
    public GameObject staticImage;
    public MeshRenderer staticImageMeshRenderer;
    public FullScreenQuad staticFullScreenQuad;

    public GameObject videoImage;
    public MeshRenderer videoImageMeshRenderer;
    public FullScreenQuad videoFullScreenQuad;
    public float videoImageDistanceOffset = 1;

    public MediaPlayerCtrl mpc;

    public GameObject blocker;
    public Texture2D defaultTexture;

    [Header("Camera Movement")]
    public GameObject arGameObject;
    public Camera arCamera;
    public Camera zoomCamera;
    public Transform endPosition;
    public float staticImageStartDistance = 250;
    public float staticImageEndDistance = 101;

    [Tooltip("This is how many frames it takes to move from the 'target detected' position, to the 'full frame' position. NOTE changing this also changes the 'delay to show bloacker' time.")]
    public int framesForZoom = 30;
    [Tooltip("This is how many frames the frade between the static placeholder image and the video takes.")]
    public int framesForAlphaFade = 10;

    [Header("Video")]
    public float delayToShowBlocker = .5f;
    public GameObject videoSkipGameObject;

    [Header("Other")]
    public Texture startingTexture;
    public float minStartingImageTime = 5f;
    public GameObject postVideoUIGameObject;


    public Texture2D[] textureCache;
    public TextAsset vuforiaXML;
    public TextAsset vuforiaDB;

    Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
    string imagemimetype = ".png";

    string databaseNameLocal = "";
    string databaseNameRemote = "";

    void OnValidate()
    {
        SetupRelationships();
    }



    #region PUBLIC API

    public void SetupData()
    {
        Debug.Log("SetupData", this);
        SetupRelationships();
        StartCoroutine(MaterialsInit());
    }

    public void Reset()
    {
        SetupRelationships();
    }

    public void LoadMovie()
    {
        mpc.Load("http://" + serverName2 + "/" + trackableName + ".mp4" );
    }

    public bool HasTexture(string name)
    {
        foreach(var t in textureCache)
        {
            if(t.name.ToLower().Contains(name.ToLower())) return true;
        }    
        return false;
    }


    public Texture2D TextureGet(string name)
    {
        if(textures.ContainsKey(name)) return textures[name];
        foreach(var t in textureCache)
        {
            if(t.name.ToLower() == name.ToLower()) return t;
        }
        string path = System.IO.Path.Combine(Application.persistentDataPath, name + imagemimetype );
        if(!System.IO.File.Exists(path)) {
            Debug.Log("OaklandFence: MaterialGet: failed to load " + path );
            return defaultTexture;
        }

        var bytesRead = System.IO.File.ReadAllBytes(path);
        Texture2D texture = new Texture2D (2,2);
        texture.LoadImage (bytesRead);
        textures[name] = texture;
        return texture;
    }


    public string GetPath(params string[] parts)
    {
        string output = "";
        for(int i = 0; i < parts.Length; i++)
        {
            if(i == 0) output = parts[i];
            else output += System.IO.Path.DirectorySeparatorChar + parts[i];
        }
        return output;
    }
    #endregion


    void SetupRelationships()
    {

        staticImageMeshRenderer = staticImage.GetComponent<MeshRenderer>();
        staticFullScreenQuad = staticImage.GetComponent<FullScreenQuad>();
        staticImageMeshRenderer.enabled = false;

        videoImageMeshRenderer = videoImage.GetComponent<MeshRenderer>();
        videoFullScreenQuad = videoImage.GetComponent<FullScreenQuad>();
        videoImageMeshRenderer.enabled = false;

        mpc = videoImage.GetComponent<MediaPlayerCtrl>();
        mpc.Stop();
        mpc.UnLoad();

        blocker.SetActive(false);
        postVideoUIGameObject.SetActive(false);
        videoSkipGameObject.SetActive(false);
          
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Materials management - fetched over net once
    ///////////////////////////////////////////////////////////////////////////////////////////////////////


    IEnumerator MaterialsInit() {

        Debug.Log("MaterialsInit()", this);

        var dirSep = System.IO.Path.DirectorySeparatorChar;
        string otherPath = "";

        string path = "";

        // Fetch version information
        if(true) {
            path = System.IO.Path.Combine(Application.persistentDataPath, "version.txt");
            // What was the previously fetched database?
            if(System.IO.File.Exists(path)) {
                databaseNameLocal = System.IO.File.ReadAllText(path);
            }
            // What database is the server currently wanting to show?
            var www = new WWW("http://" + serverName + "/version.txt");
            yield return www;
            databaseNameRemote = www.text;
            // If there is no server connection then no point in trying to fetch data - return
            if(databaseNameRemote == null || databaseNameRemote.Length < 3) {
                Debug.LogError("Unable to connect to internet...");
                yield break;
            }
            // Write a database version hint after everything is updated
            System.IO.File.WriteAllText(path,databaseNameRemote);
        }

        // Update database?
        Debug.Log("DB Local: " + databaseNameLocal + " BD Remote: " + databaseNameRemote, this);

        if(string.IsNullOrEmpty(databaseNameLocal) || databaseNameRemote != databaseNameLocal)
        {
            // check for a local version of the remote name. 

            if(vuforiaXML.name.Contains(databaseNameRemote))
            {
                Debug.Log("Using locally included DB", this);
                // this is still good. 
                otherPath = GetPath(Application.persistentDataPath, databaseNameRemote + ".xml");
                System.IO.File.WriteAllBytes(otherPath, vuforiaXML.bytes);

                otherPath = GetPath(Application.persistentDataPath, databaseNameRemote + ".dat");
                System.IO.File.WriteAllBytes(otherPath , vuforiaDB.bytes);
                databaseNameLocal = databaseNameRemote;
            }
        }

        if(databaseNameRemote != databaseNameLocal) {
            Debug.Log("Checking online for new DB", this);

            string url = "http://" + serverName + "/" + databaseNameRemote + ".xml";
            path = System.IO.Path.Combine(Application.persistentDataPath, databaseNameRemote + ".xml");
            var www = new WWW(url);
            yield return www;
            System.IO.File.WriteAllBytes(path,www.bytes);
            Debug.Log("Caching Trackables - saved database " + path );
            url = "http://" + serverName + "/" + databaseNameRemote + ".dat";
            path = System.IO.Path.Combine(Application.persistentDataPath, databaseNameRemote + ".dat");
            www = new WWW(url);
            yield return www;
            System.IO.File.WriteAllBytes(path,www.bytes);
            Debug.Log("Caching Trackables - saved database " + path );
            // TODO this would be a good opportunity to flush any previous material cache
        }

        Debug.Log("Creating tracker and dataset", this);

        // Inject into Vuforia
        ObjectTracker tracker = Vuforia.TrackerManager.Instance.GetTracker<Vuforia.ObjectTracker>();
        DataSet dataset = tracker.CreateDataSet();
        path = System.IO.Path.Combine(Application.persistentDataPath, databaseNameRemote + ".xml");
        if(!dataset.Load(path, VuforiaUnity.StorageType.STORAGE_ABSOLUTE)) {
            Debug.Log("Caching system - Failed to load dataset?");
            yield break;
        }
        tracker.ActivateDataSet(dataset);
        tracker.Start();

        // Tidy up the names that Vuforia made
        int counter = 0;
        IEnumerable<Vuforia.TrackableBehaviour> tbs = TrackerManager.Instance.GetStateManager().GetTrackableBehaviours();
        foreach (TrackableBehaviour tb in tbs) {
            if (tb.name != "New Game Object") {
                continue;
            }
            tb.gameObject.name = ++counter + ":DynamicImageTarget-" + tb.TrackableName;
            //tb.gameObject.AddComponent<DefaultTrackableEventHandler>();
            //tb.gameObject.AddComponent<TurnOffBehaviour>();
        }


        if(true)
        {
                
            // Update material cache?
            {
                string name,url;
                XmlDocument xml = new XmlDocument();
                path = System.IO.Path.Combine(Application.persistentDataPath, databaseNameRemote + ".xml");
                xml.Load(path);
                XmlNodeList nodes = xml.DocumentElement.SelectNodes("/QCARConfig/Tracking/ImageTarget");
                for(int i = 0; i < nodes.Count; i++) {
                    name = nodes[i].Attributes["name"].Value;

                    // Check the shipped cache for the image
                    if(HasTexture(name)) continue;

                    // Check the downloaded cache for the image
                    path = System.IO.Path.Combine(Application.persistentDataPath, name + imagemimetype );
                    if(System.IO.File.Exists(path)) {
                        continue;
                    }
                    url = "http://" + serverName + "/" + name + imagemimetype;
                    var www = new WWW(url);
                    yield return www;
                    System.IO.File.WriteAllBytes(path,www.bytes);
                    Debug.Log("Cached Trackable: saved image " + name );
                }
            }

            // Update material cache for bumpers?
            {
                string name,url;
                XmlDocument xml = new XmlDocument();
                path = System.IO.Path.Combine(Application.persistentDataPath, databaseNameRemote + ".xml");

                xml.Load(path);
                XmlNodeList nodes = xml.DocumentElement.SelectNodes("/QCARConfig/Tracking/ImageTarget");
                for(int i = 0; i < nodes.Count; i++) {
                    name = nodes[i].Attributes["name"].Value;

                    // check the shipped cache for the image
                    if(HasTexture(name + State_Data.BUMPERPOSTFIX)) continue;

                    // check the downloaded cached for the image
                    path = System.IO.Path.Combine(Application.persistentDataPath, name + State_Data.BUMPERPOSTFIX + imagemimetype );
                    if(System.IO.File.Exists(path)) {
                        continue;
                    }
                    url = "http://" + serverName + "/" + name + BUMPERPOSTFIX + imagemimetype;
                    var www = new WWW(url);
                    yield return www;
                    System.IO.File.WriteAllBytes(path,www.bytes);
                    Debug.Log("Cached Trackable: saved image " + name );
                }
            }
        }

        if(SetupCompleteEvent != null) SetupCompleteEvent.Invoke();
    }
}
