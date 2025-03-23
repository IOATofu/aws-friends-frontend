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
        [SerializeField] private LocomotionManager locomotionManager;
        
        List<Friend> friends = new();
        Dictionary<InstanceType, GameObject> prefabs = new();
        // Dictionary to track existing ARNs
        Dictionary<string, Friend> arnToFriendMap = new();
        
        private bool isInitialized = false;
        private bool isUpdateOccured = false;
        private bool endRoutine = false;

        private void Awake() {
            foreach (var p in friendParams) {
                prefabs.Add(p.IType, p.CharaPrefab);
            }
        }
        
        private void Start() {
            Debug.Log("Started");
            StartCoroutine(requestHandler.GetAwsComponents((components) => {
                Debug.Log("Callback");
                InitializeInstances(components);
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
        
        // Initialize instances on first load
        private void InitializeInstances(List<AwsComponent> components) {
            foreach (var c in components) {
                if(!prefabs.ContainsKey(c.IType))
                    continue;
                
                CreateFriendInstance(c);
            }
        }
        
        // Create a new Friend instance
        private Friend CreateFriendInstance(AwsComponent component) {
            var friend = Instantiate(prefabs[component.IType]).GetComponent<Friend>();
            friends.Add(friend);
            friend.InitFriend(component.Arn, component.InstanceName, locomotionManager);
            friend.Cost = component.Cost;
            friend.ChangeState(component.IState);
            
            // Add to tracking dictionary
            arnToFriendMap[component.Arn] = friend;
            
            Debug.Log($"Created new instance: {component.InstanceName} ({component.Arn})");
            return friend;
        }
        
        // Remove a Friend instance
        private void RemoveFriendInstance(string arn) {
            if (arnToFriendMap.TryGetValue(arn, out Friend friend)) {
                friends.Remove(friend);
                arnToFriendMap.Remove(arn);
                Destroy(friend.gameObject);
                Debug.Log($"Removed instance: {friend.Name} ({arn})");
            }
        }
        
        IEnumerator UpdateRoutine() {
            if (!isInitialized) {
                Debug.LogError("Invalid updateRoutine");
                yield break;
            }

            while (!endRoutine) {
                StartCoroutine(requestHandler.GetAwsComponents(components => {
                    // Create a set of current ARNs for quick lookup
                    HashSet<string> currentArns = new HashSet<string>();
                    foreach (var component in components) {
                        currentArns.Add(component.Arn);
                        
                        // Check if this is a new instance
                        if (!arnToFriendMap.ContainsKey(component.Arn)) {
                            // New instance found, create it if we have the prefab
                            if (prefabs.ContainsKey(component.IType)) {
                                CreateFriendInstance(component);
                            }
                        } else {
                            // Existing instance, update its state and cost
                            Friend friend = arnToFriendMap[component.Arn];
                            friend.ChangeState(component.IState);
                            friend.Cost = component.Cost;
                        }
                    }
                    
                    // Find instances that no longer exist and remove them
                    List<string> arnsToRemove = new List<string>();
                    foreach (var kvp in arnToFriendMap) {
                        if (!currentArns.Contains(kvp.Key)) {
                            arnsToRemove.Add(kvp.Key);
                        }
                    }
                    
                    // Remove instances that no longer exist
                    foreach (var arn in arnsToRemove) {
                        RemoveFriendInstance(arn);
                    }
                }));
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