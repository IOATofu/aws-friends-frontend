using UnityEngine;
using TMPro;
using Character;

namespace UI
{
    public class Nameplate : MonoBehaviour
    {
        [SerializeField] private TextMeshPro nameplateText;
        [SerializeField] private Friend friend;
        [SerializeField] private bool faceCamera = true;
        [SerializeField] private Vector3 rotationOffset = Vector3.zero;

        private Camera mainCamera;

        private void Start()
        {
            // Find the main camera
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("Nameplate: Main camera not found. Billboard effect will not work.");
            }

            // Find the Friend component if not assigned
            if (friend == null)
            {
                friend = GetComponentInParent<Friend>();
                if (friend == null)
                {
                    Debug.LogError("Nameplate: No Friend component found in parent hierarchy");
                    return;
                }
            }

            UpdateNameplate();
        }

        private void Update()
        {
            // Make the nameplate face the camera
            if (faceCamera && mainCamera != null)
            {
                // Look at the camera
                transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                                mainCamera.transform.rotation * Vector3.up);
                
                // Apply rotation offset if needed
                transform.Rotate(rotationOffset);
            }
        }

        public void SetFriend(Friend newFriend)
        {
            friend = newFriend;
            UpdateNameplate();
        }

        public void UpdateNameplate()
        {
            if (friend == null || nameplateText == null)
                return;

            // Format the text with name on first line and cost with $ on second line
            nameplateText.text = $"{friend.Name}\n${friend.Cost:F2}";
        }
    }
}