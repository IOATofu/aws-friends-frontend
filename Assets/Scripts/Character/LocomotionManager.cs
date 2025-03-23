using UnityEngine;

namespace Character {
    public class LocomotionManager : MonoBehaviour {
        [SerializeField] private Transform p1; // First corner of rectangle
        [SerializeField] private Transform p2; // Second corner of rectangle

        [SerializeField] private float y;

        [SerializeField] private Color gizmoColor = Color.blue; // Color for visualization
        
        private Rect movementBounds; // Rectangle area for movement
        
        // Start is called before the first frame update
        void Start() {
            // Validate references
            if (p1 == null || p2 == null) {
                Debug.LogWarning("LocomotionManager: p1 or p2 is not assigned!");
            }
            
            // Initialize movement bounds
            UpdateMovementBounds();
        }

        // Update is called once per frame
        void Update() {
            // Update movement bounds in case transforms have moved
            UpdateMovementBounds();
        }
        
        // Update movement bounds rectangle based on p1 and p2 transforms
        private void UpdateMovementBounds() {
            if (p1 == null || p2 == null) {
                return;
            }
            
            // Get min and max values for X and Z from both transforms
            float minX = Mathf.Min(p1.position.x, p2.position.x);
            float maxX = Mathf.Max(p1.position.x, p2.position.x);
            float minZ = Mathf.Min(p1.position.z, p2.position.z);
            float maxZ = Mathf.Max(p1.position.z, p2.position.z);
            
            // Create rectangular bounds
            movementBounds = new Rect(
                minX,
                minZ,
                maxX - minX,
                maxZ - minZ
            );
        }

        public Vector3 GetNewPoint() {
            if (p1 == null || p2 == null) {
                Debug.LogWarning("LocomotionManager: p1 or p2 is not assigned!");
                return transform.position; // Return current position as fallback
            }
            
            // Make sure bounds are updated
            UpdateMovementBounds();
            
            // Generate random point within the rectangle
            float randomX = Random.Range(movementBounds.xMin, movementBounds.xMax);
            float randomZ = Random.Range(movementBounds.yMin, movementBounds.yMax);
            
            // Use the y-coordinate from p1
            return new Vector3(randomX, y, randomZ);
        }
        
        // Draw the movement bounds in the editor
        void OnDrawGizmos() {
            if (p1 == null || p2 == null) {
                return;
            }
            
            // Update bounds for gizmo drawing
            UpdateMovementBounds();
            
            // Draw the rectangle
            Gizmos.color = gizmoColor;
            Vector3 center = new Vector3(
                movementBounds.center.x,
                (p1.position.y + p2.position.y) / 2f, // Average Y position
                movementBounds.center.y  // Note: Rect.y is used for Z in 3D space
            );
            Vector3 size = new Vector3(
                movementBounds.width,
                0.1f,
                movementBounds.height  // Note: Rect.height is used for Z-depth in 3D space
            );
            
            Gizmos.DrawWireCube(center, size);
            
            // Draw points at the corners
            Gizmos.DrawSphere(p1.position, 0.2f);
            Gizmos.DrawSphere(p2.position, 0.2f);
        }
    }
}
