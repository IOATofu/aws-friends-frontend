using UnityEngine;

namespace CameraControl {
    public class CameraController : MonoBehaviour {
        [SerializeField] private Transform left;
        [SerializeField] private Transform right;
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private float horizontalSensitivity = 10f;
        [SerializeField] private float verticalSensitivity = 10f;
        [SerializeField] private float y;
        
        private Vector3 targetPosition;
        private Rect movementBounds;
        
        // Start is called before the first frame update
        void Start() {
            // Initialize target position to current position
            targetPosition = transform.position;
            
            // Validate references
            if (left == null || right == null) {
                Debug.LogError("CameraController: Left or Right boundary Transform is not assigned!");
            }
            
            // Initialize movement bounds
            UpdateMovementBounds();
            
            var center = movementBounds.center;
            transform.position = new Vector3(center.x, y, center.y);
        }
        
        // Update movement bounds rectangle based on left and right transforms
        private void UpdateMovementBounds() {
            if (left == null || right == null) {
                return;
            }
            
            // Get min and max values for X and Z from both transforms
            float minX = Mathf.Min(left.position.x, right.position.x);
            float maxX = Mathf.Max(left.position.x, right.position.x);
            float minZ = Mathf.Min(left.position.z, right.position.z);
            float maxZ = Mathf.Max(left.position.z, right.position.z);
            
            // Create rectangular bounds
            movementBounds = new Rect(
                minX,
                minZ,
                maxX - minX,
                maxZ - minZ
            );
        }

        // Update is called once per frame
        void Update() {
            // Update movement bounds in case transforms have moved
            UpdateMovementBounds();
            
            // Get keyboard input for both horizontal and vertical movement
            float horizontalInput = Input.GetAxis("Horizontal") * horizontalSensitivity;
            float verticalInput = Input.GetAxis("Vertical") * verticalSensitivity;
            
            // Update target position based on input
            targetPosition += new Vector3(
                horizontalInput * Time.deltaTime,  // X movement (left/right)
                0,                                 // Y stays the same (height)
                verticalInput * Time.deltaTime     // Z movement (forward/backward)
            );
            
            // Clamp target position within rectangular bounds
            Vector3 clampedPosition = targetPosition;
            clampedPosition.x = Mathf.Clamp(targetPosition.x, movementBounds.xMin, movementBounds.xMax);
            clampedPosition.z = Mathf.Clamp(targetPosition.z, movementBounds.yMin, movementBounds.yMax);
            
            targetPosition = clampedPosition;
        }
        
        void LateUpdate() {
            // Smoothly move camera towards target position
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                smoothSpeed * Time.deltaTime
            );
        }
        
        // Draw the movement bounds in the editor
        void OnDrawGizmos() {
            if (left == null || right == null) {
                return;
            }
            
            // Update bounds for gizmo drawing
            UpdateMovementBounds();
            
            // Draw the rectangle
            Gizmos.color = Color.green;
            Vector3 center = new Vector3(
                movementBounds.center.x,
                transform.position.y,
                movementBounds.center.y  // Note: Rect.y is used for Z in 3D space
            );
            Vector3 size = new Vector3(
                movementBounds.width,
                0.1f,
                movementBounds.height  // Note: Rect.height is used for Z-depth in 3D space
            );
            
            Gizmos.DrawWireCube(center, size);
        }
    }
}
