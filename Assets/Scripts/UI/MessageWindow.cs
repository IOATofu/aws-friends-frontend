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
        gameObject.SetActive(false); // Start with message window disabled
    }

    void Update()
    {
        
    }

    // Display text sequentially with the specified speed
    public void OnText(string message)
    {
        // Set the text immediately in case we can't start the coroutine
        text.text = message;
        
        // Only start the coroutine if the GameObject is active
        if (gameObject.activeInHierarchy)
        {
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
            Debug.LogWarning("Cannot start coroutine on inactive MessageWindow GameObject");
        }
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
