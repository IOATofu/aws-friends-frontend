﻿using System;
using System.Collections;
using System.Collections.Generic;
using Models;
using UnityEngine;
using UnityEngine.Events;

namespace Character {
    public class Friend : MonoBehaviour {
        [SerializeField] private string animatorParamName = "level";
        [SerializeField] private InstanceType iType;
        [SerializeField] private List<FriendAnim> anim;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private float speed = 2.0f;
        [SerializeField] private float randomMoveMinTime = 3.0f;
        [SerializeField] private float randomMoveMaxTime = 10.0f;
        [SerializeField] private Vector3 defaultDirection = Vector3.forward; // Default direction to face when not moving
        [SerializeField] private float rotationSpeed = 5.0f; // Speed of rotation when returning to default direction

        private string arn;
        private string name;
        
        private Dictionary<InstanceState, FriendAnim> animDict = new();
        private Animator animator;
        private LocomotionManager locomotionManager;
        private Vector3 targetPosition;
        private bool isMoving = false;
        private float nextMoveTime = 0f;
        private Action exitCallback = () => {};
        
        private void Start() {
        }

        private void Update() {
            // Empty - movement is handled by coroutines
        }


        public void InitFriend(string arn, string name, LocomotionManager locomotionManager) {
            foreach (var p in anim) {
                animDict.Add(p.State, p);
            }
            this.arn = arn;
            this.name = name;
            this.locomotionManager = locomotionManager;
            animator = GetComponent<Animator>();
            // Start the random movement routine after a random delay
            StartCoroutine(StartRandomRoutine());
        }

        public string Arn => arn;

        public string Name => name;

        public InstanceType IType => iType;

        public void ChangeState(InstanceState state) {
            exitCallback?.Invoke();
            animator.SetInteger(animatorParamName, animDict[state].AnimP);
            animDict[state].OnChanged();
            exitCallback = animDict[state].OnExit;
        }

        public void Chat(string prompt) {
            // TODO Implement later: DO NOT TOUCH!!!!!!
        }

        // Coroutine to start the random movement routine with a delay
        private IEnumerator StartRandomRoutine() {
            // Wait for a random time before starting the movement
            yield return new WaitForSeconds(UnityEngine.Random.Range(randomMoveMinTime, randomMoveMaxTime));
            RandomRoutine();
        }

        void RandomRoutine() {
            // Call RandomLocomotion to start moving to a random point
            RandomLocomotion();
            
            // Start the movement coroutine
            StartCoroutine(MoveToDestination());
        }

        void RandomLocomotion() {
            // Get a new random destination point from the locomotion manager
            targetPosition = locomotionManager.GetNewPoint();
            
            // Start moving
            isMoving = true;
        }
        
        // Coroutine to handle movement to the destination
        private IEnumerator MoveToDestination() {
            while (isMoving) {
                // Calculate direction to the target
                Vector3 direction = targetPosition - transform.position;
                direction.y = 0; // Keep movement on the horizontal plane
                
                // Check if we're close enough to the destination
                if (direction.magnitude < 0.1f) {
                    // We've reached the destination
                    isMoving = false;
                    animator.SetFloat("walkSpeed", 0);
                    
                    // Rotate to default direction before scheduling next movement
                    StartCoroutine(RotateToDefaultDirection());
                    
                    // Schedule the next random movement after a delay
                    StartCoroutine(StartRandomRoutine());
                    yield break;
                }
                
                // Normalize direction and apply speed
                Vector3 velocity = direction.normalized * speed;
                
                // Move the character using CharacterController
                characterController.SimpleMove(velocity);
                
                // Update the animator speed parameter
                animator.SetFloat("walkSpeed", velocity.magnitude);
                
                // Rotate the character to face the movement direction
                if (direction != Vector3.zero) {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
                }
                
                // Wait for the next frame
                yield return null;
            }
        }
        
        // Coroutine to handle rotation to the default direction
        private IEnumerator RotateToDefaultDirection() {
            // Make sure defaultDirection is not zero
            if (defaultDirection != Vector3.zero) {
                // Create a horizontal direction (ignore Y component)
                Vector3 targetDir = defaultDirection;
                targetDir.y = 0;
                targetDir = targetDir.normalized;
                
                // Create the target rotation
                Quaternion targetRotation = Quaternion.LookRotation(targetDir);
                
                // Keep rotating until we're very close to the target rotation
                while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f) {
                    // Smoothly rotate towards the target direction
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime
                    );
                    
                    // Wait for the next frame
                    yield return null;
                }
                
                // Ensure exact rotation at the end
                transform.rotation = targetRotation;
            }
        }
    }

    [Serializable]
    public class FriendAnim {
        [SerializeField] private InstanceState state;
        [SerializeField] private int animP;
        [SerializeField] private UnityEvent onChanged;
        [SerializeField] private UnityEvent onExit;
        
        public InstanceState State => state;

        public int AnimP => animP;

        public void OnChanged() {
            onChanged?.Invoke();
        }

        public void OnExit() {
            onExit?.Invoke();
        }
    }
}