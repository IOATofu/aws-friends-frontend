using System.Collections.Generic;
using UnityEngine;

namespace Character {
    public class LocomotionManager : MonoBehaviour {
        [SerializeField] private List<Transform> locomotionPoints;
        
        // Start is called before the first frame update
        void Start() {
        
        }

        // Update is called once per frame
        void Update() {
        
        }

        public Vector3 GetNewPoint() {
            var point = locomotionPoints[Random.Range(0, locomotionPoints.Count)].position;
            return point;
        }
    }
}
