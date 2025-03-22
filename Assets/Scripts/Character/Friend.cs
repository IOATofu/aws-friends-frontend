using System;
using System.Collections.Generic;
using Models;
using UnityEngine;

namespace Character {
    public class Friend : MonoBehaviour {
        [SerializeField] private string animatorParamName = "level";
        [SerializeField] private InstanceType iType;
        [SerializeField] private List<FriendAnim> anim;

        private string arn;
        private string name;

        private Dictionary<InstanceState, int> animDict = new();
        private Animator animator;
        
        
        void Awake() {
            foreach (var p in anim) {
                animDict.Add(p.State, p.AnimP);
            }
        }

        private void Start() {
            animator = GetComponent<Animator>();
        }

        private void Update() {
            
        }


        public void InitFriend(string arn, string name) {
            this.arn = arn;
            this.name = name;
        }

        public string Arn => arn;

        public string Name => name;

        public InstanceType IType => iType;

        public void ChangeState(InstanceState state) {
            animator.SetInteger(animatorParamName, animDict[state]);
        }

        public void Chat(string prompt) {
            // TODO Implement later: DO NOT TOUCH!!!!!!
        }

        void RandomRoutine() {
            // TODO Implement later: DO NOT TOUCH!!!!!!
        }
    }

    [Serializable]
    public class FriendAnim {
        [SerializeField] private InstanceState state;
        [SerializeField] private int animP;

        public InstanceState State => state;

        public int AnimP => animP;
    }
}