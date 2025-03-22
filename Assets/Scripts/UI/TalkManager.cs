using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character;

public class TalkManager : MonoBehaviour {
    [SerializeField] private RequestHandler requestHandler;
    [SerializeField] private string thinkingText = "Thinking...";
    [SerializeField] private string layer = "Friend";
    [SerializeField] private MessageWindow messageWindow;
    [SerializeField] private bool talkOnClick = true;
    
    private Friend currentFriend;
    private bool isTalking = false;
    private int layerMask;

    void Start()
    {
        // Convert layer name to layer mask for raycasting
        layerMask = 1 << LayerMask.NameToLayer(layer);
        
        // Ensure message window is disabled at start
        if (messageWindow != null)
        {
            messageWindow.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Handle mouse click to select a character
        if (Input.GetMouseButtonDown(0) && !isTalking)
        {
            HandleCharacterClick();
        }
        
        // Handle ESC key to end conversation
        if (Input.GetKeyDown(KeyCode.Escape) && isTalking)
        {
            EndConversation();
        }
    }
    
    // Handle character selection via mouse click
    private void HandleCharacterClick()
    {
        Debug.Log("Ray casted");
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        // Debug layer information
        Debug.Log($"Looking for objects on layer: {layer} (Layer index: {LayerMask.NameToLayer(layer)}, Mask: {layerMask})");
        
        // First try without layer mask to see if we're hitting anything at all
        if (Physics.Raycast(ray, out hit, 100f))
        {
            Debug.Log($"Hit something: {hit.collider.gameObject.name} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            
            // Check if the hit object or any of its parents has a Friend component
            Friend friend = hit.collider.GetComponentInParent<Friend>();
            if (friend != null)
            {
                Debug.Log($"Found Friend component on {friend.gameObject.name}");
                StartConversation(friend);
                return;
            }
            else
            {
                Debug.Log("No Friend component found on hit object or its parents");
                
                // Try to find Friend component in children
                friend = hit.collider.gameObject.GetComponentInChildren<Friend>();
                if (friend != null)
                {
                    Debug.Log($"Found Friend component in children of {hit.collider.gameObject.name}");
                    StartConversation(friend);
                    return;
                }
            }
        }
        else
        {
            Debug.Log("Raycast didn't hit anything");
        }
        
        // Try with layer mask as a fallback
        if (Physics.Raycast(ray, out hit, 1000f, layerMask))
        {
            Debug.Log($"Hit something with layer mask: {hit.collider.gameObject.name}");
            
            // Try to get the Friend component from the hit object or its parents
            Friend friend = hit.collider.GetComponentInParent<Friend>();
            if (friend != null)
            {
                Debug.Log($"Found Friend component on {friend.gameObject.name}");
                StartConversation(friend);
            }
            else
            {
                Debug.Log("No Friend component found on hit object with layer mask");
            }
        }
    }
    
    // Start conversation with a character
    private void StartConversation(Friend friend)
    {
        if (isTalking)
            return;
            
        isTalking = true;
        currentFriend = friend;
        
        // Clear previous conversation logs
        requestHandler.ClearLog();
        
        // Enable message window
        if (messageWindow != null)
        {
            messageWindow.gameObject.SetActive(true);
            
            // Wait a frame to ensure the GameObject is fully activated
            StartCoroutine(StartConversationAfterActivation());
        }
    }
    
    // Coroutine to start conversation after ensuring the message window is active
    private IEnumerator StartConversationAfterActivation()
    {
        // Wait for the end of the frame to ensure the GameObject is fully activated
        yield return new WaitForEndOfFrame();
        
        // If talkOnClick is enabled, automatically start conversation
        if (talkOnClick && currentFriend != null)
        {
            Debug.Log("Starting initial conversation");
            
            // Show thinking message
            messageWindow.OnText(thinkingText);
            
            // Start initial chat
            StartCoroutine(requestHandler.Chat(currentFriend.Arn, OnChatResponse));
        }
    }
    
    // End the current conversation
    private void EndConversation()
    {
        if (!isTalking)
            return;
            
        isTalking = false;
        currentFriend = null;
        
        // Clear conversation logs
        requestHandler.ClearLog();
        
        // Disable message window
        if (messageWindow != null)
        {
            messageWindow.gameObject.SetActive(false);
        }
    }
    
    // Handle user input during conversation
    public void OnInputted(string msg)
    {
        if (!isTalking || currentFriend == null)
            return;
        
        // Make sure message window is active
        if (messageWindow != null && !messageWindow.gameObject.activeInHierarchy)
        {
            messageWindow.gameObject.SetActive(true);
        }
        
        // Use coroutine to ensure GameObject is active before showing message
        StartCoroutine(SendMessageAfterActivation(msg));
    }
    
    // Coroutine to send message after ensuring the message window is active
    private IEnumerator SendMessageAfterActivation(string msg)
    {
        // Wait for the end of the frame to ensure the GameObject is fully activated
        yield return new WaitForEndOfFrame();
        
        // Show thinking message
        messageWindow.OnText(thinkingText);
        
        // Send message to the API
        StartCoroutine(requestHandler.Talk(currentFriend.Arn, msg, OnChatResponse));
    }
    
    // Callback for chat responses
    private void OnChatResponse(string response)
    {
        if (response != null && messageWindow != null)
        {
            // Make sure message window is active
            if (!messageWindow.gameObject.activeInHierarchy)
            {
                messageWindow.gameObject.SetActive(true);
                // Use coroutine to ensure GameObject is active before showing message
                StartCoroutine(ShowResponseAfterActivation(response));
            }
            else
            {
                // Display the response in the message window
                messageWindow.OnText(response);
            }
        }
    }
    
    // Coroutine to show response after ensuring the message window is active
    private IEnumerator ShowResponseAfterActivation(string response)
    {
        // Wait for the end of the frame to ensure the GameObject is fully activated
        yield return new WaitForEndOfFrame();
        
        // Display the response in the message window
        messageWindow.OnText(response);
    }
}
