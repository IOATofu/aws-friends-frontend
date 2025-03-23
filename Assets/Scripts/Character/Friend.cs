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
        [SerializeField] private CapsuleCollider capsuleCollider;
        [SerializeField] private float speed = 2.0f;
        [SerializeField] private float randomMoveMinTime = 3.0f;
        [SerializeField] private float randomMoveMaxTime = 10.0f;
        [SerializeField] private Vector3 defaultDirection = Vector3.forward; // Default direction to face when not moving
        [SerializeField] private float rotationSpeed = 5.0f; // Speed of rotation when returning to default direction

        private string arn;
        private string name;
        private float cost;
        
        private Dictionary<InstanceState, FriendAnim> animDict = new();
        private Animator animator;
        private LocomotionManager locomotionManager;
        private Vector3 targetPosition;
        private bool isMoving = false;
        private float nextMoveTime = 0f;
        private InstanceState currentState;
        private bool movementEnabled = true;
        private Coroutine randomRoutineCoroutine;
        private Coroutine moveCoroutine;
        private Coroutine rotateCoroutine;
        
        private void Start() {
            // Set capsuleCollider to trigger mode
            if (capsuleCollider != null) {
                capsuleCollider.isTrigger = true;
            }
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
            
            // Initialize currentState to ensure proper state tracking
            // It will be properly set when ChangeState is first called
            currentState = InstanceState.MIDDLE; // Default initial state
            
            // Start the random movement routine after a random delay
            randomRoutineCoroutine = StartCoroutine(StartRandomRoutine());
        }

        public string Arn => arn;

        public string Name => name;

        public InstanceType IType => iType;
        
        public float Cost { get; set; }

        public void ChangeState(InstanceState state) {
            // Skip if state hasn't changed
            // if (currentState == state) return;
            
            // Call OnExit for the current state (if any)
            if (animDict.ContainsKey(currentState)) {
                animDict[currentState].OnExit();
            }
            
            // Update current state
            currentState = state;
            
            // Update animator and call OnChanged for the new state
            animator.SetInteger(animatorParamName, animDict[state].AnimP);
            animDict[state].OnChanged();
        }

        // Public methods to stop and resume movement
        public void StopMovement() {
            if (!movementEnabled) return; // Already stopped
            
            movementEnabled = false;
            isMoving = false;
            
            // Stop animation
            if (animator != null) {
                animator.SetFloat("walkSpeed", 0);
            }
            
            // Stop all movement coroutines
            if (randomRoutineCoroutine != null) {
                StopCoroutine(randomRoutineCoroutine);
                randomRoutineCoroutine = null;
            }
            
            if (moveCoroutine != null) {
                StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }
            
            if (rotateCoroutine != null) {
                StopCoroutine(rotateCoroutine);
                rotateCoroutine = null;
            }
            
            // Face default direction when stopping
            rotateCoroutine = StartCoroutine(RotateToDefaultDirection());
        }
        
        public void ResumeMovement() {
            if (movementEnabled) return; // Already moving
            
            movementEnabled = true;
            
            // Start random movement again
            randomRoutineCoroutine = StartCoroutine(StartRandomRoutine());
        }
        
        // Coroutine to start the random movement routine with a delay
        private IEnumerator StartRandomRoutine() {
            // Wait for a random time before starting the movement
            yield return new WaitForSeconds(UnityEngine.Random.Range(randomMoveMinTime, randomMoveMaxTime));
            
            // Only continue if movement is enabled
            if (movementEnabled) {
                RandomRoutine();
            }
        }

        void RandomRoutine() {
            // Only move if movement is enabled
            if (!movementEnabled) return;
            
            // Call RandomLocomotion to start moving to a random point
            RandomLocomotion();
            
            // Start the movement coroutine
            moveCoroutine = StartCoroutine(MoveToDestination());
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
                    rotateCoroutine = StartCoroutine(RotateToDefaultDirection());
                    
                    // Schedule the next random movement after a delay
                    // Only start if movement is enabled
                    if (movementEnabled) {
                        randomRoutineCoroutine = StartCoroutine(StartRandomRoutine());
                    }
                    yield break;
                }
                
                // Normalize direction and apply speed
                Vector3 velocity = direction.normalized * (speed * InstanceStateHelper.InstanceStateSpeeder(currentState));
                
                // Move the character by updating transform position directly
                transform.position += velocity * Time.deltaTime;
                
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