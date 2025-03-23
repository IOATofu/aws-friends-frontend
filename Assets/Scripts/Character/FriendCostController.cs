using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character;

/// <summary>
/// Controls the size of a Friend based on its cost and enables a GameObject when cost exceeds a threshold.
/// </summary>
public class FriendCostController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the Friend component")]
    [SerializeField] private Friend friend;

    [Header("Size Settings")]
    [Tooltip("Minimum scale to apply")]
    [SerializeField] private float minSize = 0.8f;
    
    [Tooltip("Maximum scale to apply")]
    [SerializeField] private float maxSize = 1.5f;
    
    [Header("Threshold Settings")]
    [Tooltip("Cost threshold to enable the warning GameObject")]
    [SerializeField] private float costThreshold = 2.0f;
    
    [Tooltip("GameObject to enable when cost exceeds threshold")]
    [SerializeField] private GameObject warningObject;

    [SerializeField] private float maxCost = 5f;

    private Vector3 originalScale;

    private void Start()
    {
        // Store the original scale
        originalScale = transform.localScale;
        
        // Make sure the warning object is initially disabled
        if (warningObject != null)
        {
            warningObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (friend == null) return;

        // Get the current cost from the Friend
        float cost = friend.Cost;
        
        // Scale the Friend based on cost
        UpdateScale(cost);
        
        // Enable/disable warning object based on threshold
        UpdateWarningObject(cost);
    }

    /// <summary>
    /// Updates the scale of the Friend based on its cost
    /// </summary>
    private void UpdateScale(float cost)
    {
        // Calculate a normalized scale factor between minSize and maxSize based on cost
        // For simplicity, we'll use a linear mapping, but you could use any function here
        
        // Clamp cost to a reasonable range (e.g., 0-5) for scaling purposes
        float clampedCost = Mathf.Clamp(cost, 0f, maxCost);
        
        // Normalize to 0-1 range
        float normalizedCost = clampedCost / maxCost;
        
        // Calculate scale factor between minSize and maxSize
        float scaleFactor = Mathf.Lerp(minSize, maxSize, normalizedCost);
        
        // Apply the scale
        transform.localScale = originalScale * scaleFactor;
    }

    /// <summary>
    /// Enables or disables the warning object based on cost threshold
    /// </summary>
    private void UpdateWarningObject(float cost)
    {
        if (warningObject != null)
        {
            // Enable warning object if cost exceeds threshold
            warningObject.SetActive(cost >= costThreshold);
        }
    }
}