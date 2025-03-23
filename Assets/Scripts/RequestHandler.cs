using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using Models;

[Serializable]
public class ChatMessage {
    public string role;
    public string message;

    public ChatMessage(string role, string message) {
        this.role = role;
        this.message = message;
    }
}

[Serializable]
public class ChatResponse {
    public string arn;
    public ReturnMessage return_message;
}

[Serializable]
public class ReturnMessage {
    public string role;
    public string message;
}

public class RequestHandler : MonoBehaviour {
    [SerializeField] private string baseUrl = "http://localhost:8080"; // Replace with your actual API base URL
    
    private List<ChatMessage> chatLog = new List<ChatMessage>();

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }

    /// <summary>
    /// Gets a list of AWS components by calling the /instances endpoint
    /// </summary>
    /// <returns>A coroutine that returns a List of AwsComponent</returns>
    public virtual IEnumerator GetAwsComponents(Action<List<AwsComponent>> callback) {
        using (UnityWebRequest webRequest = UnityWebRequest.Get($"{baseUrl}/instances")) {
            // Send the request and wait for a response
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError) {
                Debug.LogError($"Error: {webRequest.error}");
                callback(null);
            } else {
                // Parse the JSON response
                string jsonResponse = webRequest.downloadHandler.text;
                List<AwsComponent> components = ParseAwsComponents(jsonResponse);
                callback(components);
            }
        }
    }

    /// <summary>
    /// Parses the JSON response from the /instances endpoint into a List of AwsComponent
    /// </summary>
    /// <param name="json">The JSON response</param>
    /// <returns>A List of AwsComponent</returns>
    private List<AwsComponent> ParseAwsComponents(string json) {
        try {
            // Parse the JSON array
            List<AwsComponent> components = new List<AwsComponent>();
            JsonArray jsonArray = JsonUtility.FromJson<JsonArray>($"{{\"items\":{json}}}");

            foreach (JsonComponent item in jsonArray.items) {
                InstanceType instanceType;
                InstanceState instanceState;
                
                if (Enum.TryParse(item.type, true, out instanceType)) {
                    // Parse the state string to InstanceState enum
                    if (item.state.ToLower() == "low") {
                        instanceState = InstanceState.LOW;
                    } else if (item.state.ToLower() == "medium" || item.state.ToLower() == "middle") {
                        instanceState = InstanceState.MIDDLE;
                    } else if (item.state.ToLower() == "high") {
                        instanceState = InstanceState.HIGH;
                    } else {
                        Debug.LogWarning($"Unknown instance state: {item.state}, defaulting to MIDDLE");
                        instanceState = InstanceState.MIDDLE;
                    }
                    
                    // Create a new AwsComponent with the parsed type, state, and cost
                    AwsComponent component = new AwsComponent(item.arn, item.name, instanceType, instanceState, item.cost);
                    components.Add(component);
                } else {
                    Debug.LogWarning($"Unknown instance type: {item.type}");
                }
            }

            return components;
        } catch (Exception e) {
            Debug.LogError($"Error parsing AWS components: {e.Message}");
            return new List<AwsComponent>();
        }
    }

    // Helper classes for JSON deserialization
    [Serializable]
    private class JsonArray {
        public JsonComponent[] items;
    }

    [Serializable]
    private class JsonComponent {
        public string type;
        public string arn;
        public string name;
        public string state;
        public float cost;
    }

    /// <summary>
    /// Sends a chat message to the /talk endpoint and returns the response
    /// </summary>
    /// <param name="arn">The ARN of the AWS component</param>
    /// <param name="msg">The message to send</param>
    /// <param name="callback">Callback function that receives the assistant's response message</param>
    /// <returns>A coroutine that returns the assistant's response message</returns>
    public virtual IEnumerator Talk(string arn, string msg, Action<string> callback) {
        // Add the user's message to the chat log
        chatLog.Add(new ChatMessage("user", msg));

        // Create the request data
        string jsonData = JsonUtility.ToJson(new ChatRequest {
            arn = arn,
            log = chatLog.ToArray()
        });
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest webRequest = new UnityWebRequest($"{baseUrl}/talk", "POST")) {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // Send the request and wait for a response
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError) {
                Debug.LogError($"Error: {webRequest.error}");
                callback(null);
            } else {
                // Parse the JSON response
                string jsonResponse = webRequest.downloadHandler.text;
                ChatResponse response = JsonUtility.FromJson<ChatResponse>(jsonResponse);
                
                // Add the assistant's response to the chat log
                chatLog.Add(new ChatMessage(response.return_message.role, response.return_message.message));
                Debug.Log(jsonResponse);
                // Return the assistant's message
                callback(response.return_message.message);
            }
        }
    }

    /// <summary>
    /// Clears the chat log
    /// </summary>
    public void ClearLog() {
        chatLog.Clear();
    }

    /// <summary>
    /// Calls the /chat endpoint and returns the response without requiring a user message
    /// </summary>
    /// <param name="arn">The ARN of the AWS component</param>
    /// <param name="callback">Callback function that receives the assistant's response message</param>
    /// <returns>A coroutine that returns the assistant's response message</returns>
    public virtual IEnumerator Chat(string arn, Action<string> callback) {
        // Create the request data with only the ARN in form-urlencoded format
        string formData = $"arn={UnityWebRequest.EscapeURL(arn)}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(formData);

        using (UnityWebRequest webRequest = new UnityWebRequest($"{baseUrl}/chat", "POST")) {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            // Debug logging - detailed request information
            Debug.Log($"[DEBUG] Chat Request URL: {baseUrl}/chat");
            Debug.Log($"[DEBUG] Chat Request Method: POST");
            Debug.Log($"[DEBUG] Chat Request Headers: Content-Type: application/x-www-form-urlencoded");
            Debug.Log($"[DEBUG] Chat Request Body: {formData}");

            // Send the request and wait for a response
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError) {
                // Debug logging - detailed error information
                Debug.LogError($"[DEBUG] Chat Request Error: {webRequest.error}");
                Debug.LogError($"[DEBUG] Chat Response Code: {webRequest.responseCode}");
                if (webRequest.downloadHandler != null && webRequest.downloadHandler.text != null) {
                    Debug.LogError($"[DEBUG] Chat Error Response Body: {webRequest.downloadHandler.text}");
                }
                callback(null);
            } else {
                // Parse the JSON response
                string jsonResponse = webRequest.downloadHandler.text;
                
                // Debug logging - detailed response information
                Debug.Log($"[DEBUG] Chat Response Status: {webRequest.responseCode}");
                Debug.Log($"[DEBUG] Chat Response Body: {jsonResponse}");
                
                ChatResponse response = JsonUtility.FromJson<ChatResponse>(jsonResponse);
                
                // Add the assistant's response to the chat log for next talk
                chatLog.Add(new ChatMessage(response.return_message.role, response.return_message.message));
                
                // Return the assistant's message
                callback(response.return_message.message);
            }
        }
    }

    // Helper class for chat request serialization
    [Serializable]
    private class ChatRequest {
        public string arn;
        public ChatMessage[] log;
    }
}
