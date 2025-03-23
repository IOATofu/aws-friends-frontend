using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MessageWindow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float textSpeed = 0.05f; // Speed of text display (seconds per character)
    
    private Coroutine displayTextCoroutine;
    
    void Start()
    {
        // No need to set active state here, TalkManager will control it
    }

    void Update()
    {
        
    }

    // Display text sequentially with the specified speed
    public void OnText(string message)
    {
        Debug.Log($"MessageWindow.OnText called with message: {message}");
        
        // Set the text immediately in case we can't start the coroutine
        if (text != null)
        {
            text.text = message;
        }
        else
        {
            Debug.LogError("Text component is null in MessageWindow");
            return;
        }
        
        // Only start the coroutine if the GameObject is active
        if (gameObject.activeInHierarchy)
        {
            Debug.Log("MessageWindow is active, starting text display coroutine");
            
            // Stop any existing text display coroutine
            if (displayTextCoroutine != null)
            {
                StopCoroutine(displayTextCoroutine);
            }
            
            // Start new text display coroutine
            displayTextCoroutine = StartCoroutine(DisplayTextSequentially(message));
        }
        else
        {
            Debug.LogWarning("Cannot start coroutine on inactive MessageWindow GameObject. Activating now.");
            gameObject.SetActive(true);
            
            // Try again after activation
            StartCoroutine(DisplayAfterActivation(message));
        }
    }
    
    // Coroutine to display text after ensuring the GameObject is active
    private IEnumerator DisplayAfterActivation(string message)
    {
        // Wait a frame to ensure the GameObject is fully activated
        yield return new WaitForEndOfFrame();
        
        // Stop any existing text display coroutine
        if (displayTextCoroutine != null)
        {
            StopCoroutine(displayTextCoroutine);
        }
        
        // Start new text display coroutine
        displayTextCoroutine = StartCoroutine(DisplayTextSequentially(message));
    }
    
    // Coroutine to display text character by character
    private IEnumerator DisplayTextSequentially(string message)
    {
        text.text = ""; // Clear existing text
        
        // Display each character one by one
        for (int i = 0; i < message.Length; i++)
        {
            text.text += message[i]; // Add next character
            yield return new WaitForSeconds(textSpeed); // Wait before showing next character
        }
        
        displayTextCoroutine = null;
    }
    
    // Method to immediately show all text (skip animation)
    public void ShowFullText(string message)
    {
        if (displayTextCoroutine != null)
        {
            StopCoroutine(displayTextCoroutine);
            displayTextCoroutine = null;
        }
        
        text.text = message;
    }
}
