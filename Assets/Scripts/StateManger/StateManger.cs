using System;
using System.Collections;
using System.Collections.Generic;
using Character;
using Models;
using UnityEngine;

namespace StateManger {
    public class StateManger: MonoBehaviour {
        [SerializeField] private RequestHandler requestHandler;
        [SerializeField] private List<FriendParams> friendParams;
        [SerializeField] private InstanceState defaultState;
        [SerializeField] private float updatePeriod = 0.1f;
        
        List<Friend> friends = new();
        Dictionary<InstanceType, GameObject> prefabs = new();
        
        private bool isInitialized = false;
        private bool isUpdateOccured = false;
        private bool endRoutine = false;

        private void Awake() {
            foreach (var p in friendParams) {
                prefabs.Add(p.IType, p.CharaPrefab);
            }
        }
        
        private void Start() {
            StartCoroutine(requestHandler.GetAwsComponents((components) => {
                foreach (var c in components) {
                    var friend = prefabs[c.IType].GetComponent<Friend>();
                    friends.Add(friend);
                    friend.InitFriend(c.Arn, c.InstanceName);
                    friend.ChangeState(defaultState);
                }
                isInitialized = true;
            }));
        }

        void Update() {
            if (isInitialized && !isUpdateOccured) {
                endRoutine = false;
                StartCoroutine(UpdateRoutine());
                isUpdateOccured = true;
            }
        }
        
        IEnumerator UpdateRoutine() {
            if (!isInitialized) {
                Debug.LogError("Invalid updateRoutine");
                yield break;
            }

            while (!endRoutine) {
                foreach (var friend in friends) {
                    StartCoroutine(requestHandler.GetAwsState(friend.Arn, state => {
                        friend.ChangeState(state.IState);
                    }));
                }
                yield return new WaitForSeconds(updatePeriod);
            }
        }

        private void OnDestroy() {
            endRoutine = true;
        }
    }

    [Serializable]
    class FriendParams {
        [SerializeField] private InstanceType iType;
        [SerializeField] private GameObject charaPrefab;

        public InstanceType IType => iType;

        public GameObject CharaPrefab => charaPrefab;
    }
}