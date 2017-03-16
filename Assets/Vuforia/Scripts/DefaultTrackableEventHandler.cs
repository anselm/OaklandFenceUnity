
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Vuforia {

    public class DefaultTrackableEventHandler : MonoBehaviour, ITrackableEventHandler {

        #region PRIVATE_MEMBER_VARIABLES
        private TrackableBehaviour mTrackableBehaviour;
        #endregion

        #region UNTIY_MONOBEHAVIOUR_METHODS
        void Start() {
            mTrackableBehaviour = GetComponent<TrackableBehaviour>();
            if (mTrackableBehaviour) {
                mTrackableBehaviour.RegisterTrackableEventHandler(this);
            }
        }
        #endregion

        #region PUBLIC_METHODS
        /// <summary>
        /// Implementation of the ITrackableEventHandler function called when the
        /// tracking state changes.
        /// </summary>
        public void OnTrackableStateChanged(TrackableBehaviour.Status previousStatus, TrackableBehaviour.Status newStatus) {
            if (newStatus == TrackableBehaviour.Status.DETECTED ||
                newStatus == TrackableBehaviour.Status.TRACKED ||
                newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED) {
                OnTrackingFound();
            } else {
                OnTrackingLost();
            }
        }
        #endregion

        #region PRIVATE_METHODS
        private void OnTrackingFound() {
   			foreach(Transform child in transform) {
				child.gameObject.SetActive(true);
			}
        }
        private void OnTrackingLost() {
			foreach(Transform child in transform) {
				child.gameObject.SetActive(false);
			}
        }
        #endregion
	}      
}
