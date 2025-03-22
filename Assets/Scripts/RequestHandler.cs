using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using Models;

public class RequestHandler : MonoBehaviour {
    [SerializeField] private string baseUrl = "http://localhost:8080"; // Replace with your actual API base URL

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
    public IEnumerator GetAwsComponents(Action<List<AwsComponent>> callback) {
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
    /// Gets the state of an AWS component by calling the /load-state endpoint
    /// </summary>
    /// <param name="arn">The ARN of the AWS component</param>
    /// <returns>A coroutine that returns an AwsState</returns>
    public IEnumerator GetAwsState(string arn, Action<AwsState> callback) {
        // Create the request data
        string jsonData = $"{{\"arn\": \"{arn}\"}}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest webRequest = new UnityWebRequest($"{baseUrl}/load-state", "POST")) {
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
                AwsState state = ParseAwsState(jsonResponse);
                callback(state);
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
                if (Enum.TryParse(item.type, out instanceType)) {
                    AwsComponent component = new AwsComponent(item.arn, item.name, instanceType);
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

    /// <summary>
    /// Parses the JSON response from the /load-state endpoint into an AwsState
    /// </summary>
    /// <param name="json">The JSON response</param>
    /// <returns>An AwsState</returns>
    private AwsState ParseAwsState(string json) {
        try {
            // Parse the JSON object
            JsonStateResponse stateResponse = JsonUtility.FromJson<JsonStateResponse>(json);
            
            InstanceState state;
            if (Enum.TryParse(stateResponse.level.ToUpper(), out state)) {
                return new AwsState(stateResponse.arn, "", state); // Name is not provided in the response
            } else {
                Debug.LogWarning($"Unknown instance state: {stateResponse.level}");
                return null;
            }
        } catch (Exception e) {
            Debug.LogError($"Error parsing AWS state: {e.Message}");
            return null;
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
        public string name;
        public string arn;
    }

    [Serializable]
    private class JsonStateResponse {
        public string arn;
        public string level;
    }
}
