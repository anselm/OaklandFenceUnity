
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace Vuforia {

	public class OaklandFenceLogic : MonoBehaviour {

		public GameObject CameraObject;

		string serverName = "oaklandfenceproject.org.s3-website-us-west-1.amazonaws.com";
		string serverName2 = "s3-us-west-1.amazonaws.com/oaklandfenceproject.org";

		const int DISTANCE = 500;
		const int FRAMES_BEFORE_ZOOM = 10;
		const int FRAMES_DURING_ZOOM = 5;

		bool touched = false;
		int playState = -2;
		int playCode = 0;
		int playCount = 0;
		string playName = null;
		float playSlerp = 0;
		float targetWidth = 0;
		float targetHeight = 0;
		float targetAspect = 0;
		float videoAspect = 1;
		float projectionAspect = 0;

		Material currentMaterialHandle = null;
		Material introMaterialHandle = null;
		Material preambleMaterialHandle = null;
		Material bumperMaterialHandle = null;
		Material videoMaterialHandle = null;
		Material defaultMaterialHandle = null;

		MeshRenderer mr;
		Vuforia.StateManager sm;
		MediaPlayerCtrl videoPlayerHelper;

		void Start () {
			sm = TrackerManager.Instance.GetStateManager();
			videoPlayerHelper = GetComponent<MediaPlayerCtrl>();
			videoPlayerHelper.m_bLoop = false;
   			defaultMaterialHandle = MaterialGet("default");		// this is a fallback if other loaders fail
			introMaterialHandle   = MaterialGet("startscreen");
    		currentMaterialHandle   = MaterialGet("intro");
    		mr = GetComponent<MeshRenderer>();
			videoMaterialHandle = mr.material;
			StartCoroutine(MaterialsInit());
		}
	
		void Update () {
			HandleVuforiaRecognition();
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////
        /// State Machine
		///////////////////////////////////////////////////////////////////////////////////////////////////////

		void stateSet(int state, Material material, float slerp) {
			playState = state;
			playCount = 0;
			playCode = -1;
			playSlerp = slerp;
			currentMaterialHandle = material;
		}

		void HandleVuforiaRecognition() {

			IList<TrackableBehaviour> activeTrackables = (IList<TrackableBehaviour>) sm.GetActiveTrackableBehaviours();

			/*if(false) {
				// Enable this to do a quick and dirty test of playback on tracked object
				if(activeTrackables.Count < 1) return;
				Vuforia.TrackableBehaviour t = activeTrackables[0];
				if(videoPlayerHelper.m_strFileName == null || videoPlayerHelper.m_strFileName.Length < 1) {
					videoPlayerHelper.Load("http://" + serverName2 + "/" + t.TrackableName + ".mp4" );
					videoPlayerHelper.Play();
				}
				return;
			}*/

	        if (Input.touchCount > 0) {
	        	touched = true;
	        	// Input.GetTouch(0);
	        	Debug.Log("Got touched");
	        }

#if UNITY_EDITOR
			if( Input.GetMouseButtonDown(0) ) {
				touched = true;
			}
#endif

			switch(playState) {
			case -2:
				// show intro screen
				playCount++;
				if(touched || playCount > 60*5) {
					touched = false;
					stateSet(0,null,0);
					Debug.Log("OaklandFence: transitioning out of intro");
				} else {
					currentMaterialHandle = introMaterialHandle;
					playSlerp = 0.999f;
				}
				break;

			case -1:
				// force a delay so replays cannot happen so easily
				playCount++;
				if(playCount > 60*2) {
					stateSet(0,null,0);
				}
				break;

			case 0:
				// display should be just pass-through no overlay - looking for a target
				if(activeTrackables.Count>0) {
					Vuforia.TrackableBehaviour trackable = (Vuforia.TrackableBehaviour)activeTrackables[0];
					Vector2 size = trackable.GetComponent<ImageTargetAbstractBehaviour>().GetSize();

					// Get size and aspect ratio
                    targetWidth = size.x;
                    targetHeight = size.y;
					Debug.Log("OaklandFence: found new trackable name " + trackable.TrackableName + " of size " + targetWidth + " " + targetHeight );
                    projectionAspect = targetAspect = targetHeight/targetWidth;

					// count timer and eventually start playing
					if(playCode == trackable.GetInstanceID() && currentMaterialHandle != null) {
						playCount++;
						if(playCount > FRAMES_BEFORE_ZOOM) {
							playState = 2;
							playSlerp = 0;
							playName = trackable.TrackableName;
							videoPlayerHelper.Load("http://" + serverName2 + "/" + playName + ".mp4" );
						}
					} else {
						// restart counting over from scratch
						playCount = 0;
						playCode = trackable.GetInstanceID();
						playName = trackable.TrackableName;
						preambleMaterialHandle = MaterialGet(playName);
						bumperMaterialHandle = MaterialGetBumper(playName);
						currentMaterialHandle = preambleMaterialHandle;
					}
				}
				break;

			case 1:
				playState = 2;
				break;

			case 2:
				// video is now playing - will shortly transition to a full screen overlay
				if(touched) {
					touched = false;
					videoPlayerHelper.Stop();
					stateSet(3,bumperMaterialHandle,1);
					break;
				}

	            switch(videoPlayerHelper.GetCurrentState()) {
				case MediaPlayerCtrl.MEDIAPLAYER_STATE.READY:
					Debug.Log("OaklandFence: Media player ready? now what?");
					break;
				case MediaPlayerCtrl.MEDIAPLAYER_STATE.STOPPED:
					Debug.Log("OaklandFence: Media player stopped? now what?");
					break;
                case MediaPlayerCtrl.MEDIAPLAYER_STATE.PLAYING:
                	{
                        playSlerp = playSlerp + 0.05f; if(playSlerp>1) playSlerp = 1;
                        currentMaterialHandle = videoMaterialHandle;
                        videoAspect = 1;
                        if(videoPlayerHelper.GetVideoHeight() > 0 ) {
	                        videoAspect = videoPlayerHelper.GetVideoHeight() / videoPlayerHelper.GetVideoWidth();
	                    }
                    }
                    break;
            
				case MediaPlayerCtrl.MEDIAPLAYER_STATE.END:
					Debug.Log("OaklandFence: state 2->3: Video hit end - showing end bumper");
                    touched = false;
                    stateSet(3,bumperMaterialHandle,1);
                    break;

				case MediaPlayerCtrl.MEDIAPLAYER_STATE.PAUSED:
                    currentMaterialHandle = videoMaterialHandle;
                    break;
                    
				case MediaPlayerCtrl.MEDIAPLAYER_STATE.NOT_READY:
                    // thats ok
                    break;

				case MediaPlayerCtrl.MEDIAPLAYER_STATE.ERROR:
                    Debug.Log("OaklandFence: something went wrong with media player" );
					videoPlayerHelper.Stop();
                    stateSet(-1,null,0);
                    break;
                    
                default:
                    break;
    	        }
           
				break;

			case 3:
	            // show bumper for a while
        	    currentMaterialHandle = bumperMaterialHandle;
				playSlerp = 1;
    	        playCount++;
        	    if(playCount > 60*5) {
					Debug.Log("OaklandFence: state 3->4: Bumper done - going to fade");
                	stateSet(4,bumperMaterialHandle,1);
            	}
            	bumperTouch();
				break;

			case 4:
				// fade the after blurb away quickly
	            playSlerp = playSlerp - 0.1f; if(playSlerp<0) playSlerp = 0;
    	        playCount++;
        	    if(playCount > 10) {
					videoPlayerHelper.Stop();
	                stateSet(-1,null,0);
					Debug.Log("state 4->end: Fade done");
	            }
				break;

			case 999:
	            playSlerp = playSlerp - 0.1f; if(playSlerp<0) playSlerp = 0;
    	        playState = -1;
        	    playCount = 0;
            	playCode = -1;
            	currentMaterialHandle = null;
				videoPlayerHelper.Stop();
            	Debug.Log("state 999: returning to default");
            	break;

			default:
				break;
			}

			// Render something?
    		if(currentMaterialHandle == null) {
    			Debug.Log("no material - exiting");
    			mr.enabled = false;
    			return;
    		} else {
	    		// set rendering surface and show it
				mr.enabled = true;
	    		Material[] materials = mr.materials;
	    		materials[0] = currentMaterialHandle;
	    		mr.materials = materials;
	    	}

    		// Slerp between world space and camera space to produce a nice visual transition
    		// TODO - it's upside-down and backwards and stuff right now - needs polish
       		{
				// Get slerp origin - a place in the world
				Vector3 p1 = new Vector3(0,0,0);
				Quaternion q1 = Quaternion.identity;

				// Get slerp target - a place in front of the camera that is oriented flat to camera
				Vector3 p2 = CameraObject.transform.position + CameraObject.transform.forward * DISTANCE;
				Quaternion q2 = CameraObject.transform.rotation * Quaternion.Euler(new Vector3(90,180,0));

				// Slerp between origin and target
				gameObject.transform.position = Vector3.Slerp(p1,p2,playSlerp);
				gameObject.transform.rotation = Quaternion.Slerp(q1,q2,playSlerp);
			}

			// Adjust aspect ratio a bit
			// TODO - this is not actually yet being applied so the texture is not quite the right size
			{
				// This is sloppy but the idea is:
			    //  - aspect ratio for startup image and preamble and bumper want to be coincidentally similar to the videoAspect
			    //  - aspect ratio for the other images wants to be 1
			    float aspect = targetAspect * (1.0f - playSlerp) + videoAspect * playSlerp;

			    if(currentMaterialHandle == videoMaterialHandle) {
			        aspect = videoAspect;
			    } else if(currentMaterialHandle == introMaterialHandle) {
			    } else if(currentMaterialHandle == preambleMaterialHandle) {
			        aspect = projectionAspect;
			    } else if(currentMaterialHandle == bumperMaterialHandle) {
			    }
			}

		    // Blend the intro and bumper in 
			// TODO - this is not being applied so the introduction is not blended with backdrop yet
			{
				//BOOL blend = FALSE;
		    	//if( currentMaterialHandle == introMaterialHandle ||
		    	//    currentMaterialHandle == bumperMaterialHandle) blend = TRUE;
		    }
		}

		void bumperTouch() {
		    /*
		    if(!touched) return;
		    touched = 0;
		    Debug.Log(@"state 3: touched at %f,%f",touchpoint.x,touchpoint.y);

		    CGRect mainBounds = [[UIScreen mainScreen] bounds];

		    int state = 10;
		    if(touchpoint.x > mainBounds.size.width * 0.8f) {
		        // top row
		        if(touchpoint.y < mainBounds.size.height * 0.2f) {
		            // home
		            state = 20;
		        } else if(touchpoint.y < mainBounds.size.height * 0.65f) {
		            // nothing
		        } else {
		            // menu
		            state = 30;
		        }
		    } else if (touchpoint.x > mainBounds.size.width * 0.2f) {
		        // middle row
		        if(touchpoint.y < mainBounds.size.height * 0.36f) {
		            // support
		            state = 40;
		        } else if(touchpoint.y < mainBounds.size.height * 0.65f) {
		            // learn
		            state = 50;
		        } else {
		            // join
		            state = 60;
		        }
		    } else {
		        // bottom row
		        if(touchpoint.y < mainBounds.size.height * 0.2) {
		            // replay
		            state = 70;
		        } else if(touchpoint.y < mainBounds.size.height * 0.56f) {
		            // nothing
		        } else if(touchpoint.y < mainBounds.size.height * 0.66f) {
		            // facebook
		            state = 80;
		        } else if(touchpoint.y < mainBounds.size.height * 0.76f) {
		            // twitter
		            state = 90;
		        } else if(touchpoint.y < mainBounds.size.height * 0.86f) {
		            // instagram
		            state = 100;
		        } else if(touchpoint.y < mainBounds.size.height * 0.86f) {
		            // mail
		            state = 110;
		        }
		    }
		    
		    switch(state) {
		        case 10:
		            break;
		        case 20:
		            // fall through
		        case 30:
		            Debug.Log("button: home & menu");
		            playCount = 0;
		            playState = 4;
		            break;
		        case 40:
		            if(TRUE) {
		                Debug.Log("button: support an artist");
		                media.Stop();
		                playSlerp = 0;
		                playCount = 0;
		                playCode = -1;
		                textureHandle = 0;
		                playState = 999;
		                NSString* path = [self findSupporter:playName];
		                if(path) {
		                    NSString *urlstr = [NSString stringWithFormat:@"http://%@",path];
		                    //NSString *urlstr = [NSString stringWithFormat:@"http://%@/%@-more.html",serverName,playName];
		                    NSURL *url = [NSURL URLWithString:urlstr];
		                    [[UIApplication sharedApplication] openURL:url];
		                }
		            }
		            break;
		        case 50:
		            if(TRUE) {
		                Debug.Log("button: learn more");
		                media.Stop();
		                playSlerp = 0;
		                playCount = 0;
		                playCode = -1;
		                textureHandle = 0;
		                playState = 999;
		                NSString* path = [self findLearn:playName];
		                if(path) {
		                    NSString *urlstr = [NSString stringWithFormat:@"http://%@",path];
		                    //NSString *urlstr = [NSString stringWithFormat:@"http://%@/%@-more.html",serverName,playName];
		                    NSURL *url = [NSURL URLWithString:urlstr];
		                    [[UIApplication sharedApplication] openURL:url];
		                }
		            }
		            break;
		        case 60:
		            if(TRUE) {
		                Debug.Log("button: help");
		                [videoPlayerHelper setPlayImmediately:FALSE];
		                [videoPlayerHelper stop];
		                playSlerp = 0;
		                playCount = 0;
		                playCode = -1;
		                textureHandle = 0;
		                playState = 999;
		                NSURL *url = [NSURL URLWithString:@"http://oaklandfenceproject.org/volunteer"];
		                [[UIApplication sharedApplication] openURL:url];
		            }
		            break;
		        case 70:
		            if(TRUE) {
		                Debug.Log("button: replay");
		                playCount = 0;
		                playSlerp = 1;
		                textureHandle = preambleMaterialHandle;
		                if(FALSE) {
		                    playState = 1;
		                    NSString* url = [NSString stringWithFormat:@"http://%@/%@.mp4",serverName,playName];
		                    [videoPlayerHelper stop];
		                    [videoPlayerHelper unload];
		                    [videoPlayerHelper setPlayImmediately:TRUE];
		                    [videoPlayerHelper load:url fromPosition:0];
		                } else {
		                    playState = 2;
		                    [videoPlayerHelper setPlayImmediately:TRUE];
		                    [videoPlayerHelper replay];
		                }
		            }
		            break;
		        case 80:
		            //facebook
					media.Stop();
		            playSlerp = 0;
		            playCount = 0;
		            playCode = -1;
		            textureHandle = 0;
		            playState = 999;
		            if(![[UIApplication sharedApplication] openURL:[NSURL URLWithString:@"fb://profile"]]) {
		                [[UIApplication sharedApplication] openURL:[NSURL URLWithString:@"https://www.facebook.com/oaklandfenceproject"]];
		            }
		            break;
		        case 90:
		            //twitter
					media.Stop();
		            playSlerp = 0;
		            playCount = 0;
		            playCode = -1;
		            textureHandle = 0;
		            playState = 999;
		            if(![[UIApplication sharedApplication] openURL:[NSURL URLWithString:@"twitter://user?screen_name=oaklandfenceproject"]]) {
		                [[UIApplication sharedApplication] openURL:[NSURL URLWithString:@"https://twitter.com/oaklandfenceproject"]];
		            }
		            break;
		        case 100:
		            if(TRUE) {
						media.Stop();
		                playSlerp = 0;
		                playCount = 0;
		                playCode = -1;
		                textureHandle = 0;
		                playState = 999;
		                NSURL *instagramURL = [NSURL URLWithString:@"instagram://user?username=USERNAME"];
		                if ([[UIApplication sharedApplication] canOpenURL:instagramURL]) {
		                    [[UIApplication sharedApplication] openURL:instagramURL];
		                }
		            }
		            break;
		        case 110:
		            // mail
		            if(TRUE) {
		            	media.Stop();
		                playSlerp = 0;
		                playCount = 0;
		                playCode = -1;
		                textureHandle = 0;
		                playState = 999;
		                NSString *subject = [NSString stringWithFormat:@"oaklandfenceproject.org"];
		                NSString *mail = [NSString stringWithFormat:@"Check out the oaklandfenceproject at http://oaklandfenceproject.org!"];
		                NSURL *url = [[NSURL alloc] initWithString:[NSString stringWithFormat:@"mailto:?to=%@&subject=%@",
		                                                            [mail stringByAddingPercentEscapesUsingEncoding:NSASCIIStringEncoding],
		                                                            [subject stringByAddingPercentEscapesUsingEncoding:NSASCIIStringEncoding]]];
		                [[UIApplication sharedApplication] openURL:url];
		            }
		            break;
		        case 120:
		            break;
		    }
		    */
		}


        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        /// Materials management - fetched over net once
		///////////////////////////////////////////////////////////////////////////////////////////////////////

		string databaseNameLocal = "";
		string databaseNameRemote = "";
		Hashtable textures = new Hashtable();
		Hashtable materials = new Hashtable();
		string imagemimetype = ".png";

		IEnumerator MaterialsInit() {

			// Fetch version information
			if(true) {
				// What was the previously fetched database?
				if(System.IO.File.Exists("version.txt")) {
					databaseNameLocal = System.IO.File.ReadAllText("version.txt");
				}
				// What database is the server currently wanting to show?
				var www = new WWW("http://" + serverName + "/version.txt");
				yield return www;
				databaseNameRemote = www.text;
				// If there is no server connection then no point in trying to fetch data - return
				if(databaseNameRemote == null || databaseNameRemote.Length < 3) {
					yield break;
				}
				// Write a database version hint after everything is updated
				System.IO.File.WriteAllText("version.txt",databaseNameRemote);
			}

			// Update database?
			if(databaseNameRemote.CompareTo(databaseNameLocal) != 0) {
				string url = "http://" + serverName + "/" + databaseNameRemote + ".xml";
				string path = databaseNameRemote + ".xml";
				var www = new WWW(url);
				yield return www;
			    System.IO.File.WriteAllBytes(path,www.bytes);
			    Debug.Log("Caching Trackables - saved database " + path );
				url = "http://" + serverName + "/" + databaseNameRemote + ".dat";
				path = databaseNameRemote + ".dat";
				www = new WWW(url);
				yield return www;
			    System.IO.File.WriteAllBytes(path,www.bytes);
				Debug.Log("Caching Trackables - saved database " + path );
				// TODO this would be a good opportunity to flush any previous material cache
			}

			// Update material cache?
			{
				string name,url,path;
				XmlDocument xml = new XmlDocument();
				xml.Load(databaseNameRemote+".xml");
				XmlNodeList nodes = xml.DocumentElement.SelectNodes("/QCARConfig/Tracking/ImageTarget");
				for(int i = 0; i < nodes.Count; i++) {
					name = nodes[i].Attributes["name"].Value;
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
				string name,url,path;
				XmlDocument xml = new XmlDocument();
				xml.Load(databaseNameRemote+".xml");
				XmlNodeList nodes = xml.DocumentElement.SelectNodes("/QCARConfig/Tracking/ImageTarget");
				for(int i = 0; i < nodes.Count; i++) {
					name = nodes[i].Attributes["name"].Value;
					path = System.IO.Path.Combine(Application.persistentDataPath, name + "-bumper" + imagemimetype );
					if(System.IO.File.Exists(path)) {
						continue;
					}
					url = "http://" + serverName + "/" + name + "-bumper" + imagemimetype;
					var www = new WWW(url);
				    yield return www;
				    System.IO.File.WriteAllBytes(path,www.bytes);
					Debug.Log("Cached Trackable: saved image " + name );
		        }
		    }

			// Inject into Vuforia
			ObjectTracker tracker = Vuforia.TrackerManager.Instance.GetTracker<Vuforia.ObjectTracker>();
			DataSet dataset = tracker.CreateDataSet();
			if(!dataset.Load(databaseNameRemote)) {
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
		}

		Material MaterialGetBumper(string name) {
			return MaterialGet(name + "-bumper");
		}

		Material MaterialGet(string name) {
			Material material = (Material) materials[name];
			if(material) {
				Debug.Log("OaklandFence: MaterialGet: loaded from ram " + name );
				return material;
			}
			string path = System.IO.Path.Combine(Application.persistentDataPath, name + imagemimetype );
			if(!System.IO.File.Exists(path)) {
				path = System.IO.Path.Combine(Application.streamingAssetsPath, name + imagemimetype );
				if(!System.IO.File.Exists(path)) {
					Debug.Log("OaklandFence: MaterialGet: failed to load " + path );
					return defaultMaterialHandle;
				}
			}
			var bytesRead = System.IO.File.ReadAllBytes(path);
			Texture2D texture = new Texture2D (2,2);
			texture.LoadImage (bytesRead);
			material = new Material(Shader.Find("Specular"));
		    material.mainTexture = texture;
			material.SetTextureScale("_MainTex", new Vector2(-1,1));
		    textures[name] = texture;
		    materials[name] = material;
		    Debug.Log("OaklandFence: MaterialGet: loaded from server " + name + " " + texture.width + " " + texture.height );
		    return material;
		}
	}
}
