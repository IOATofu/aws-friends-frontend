using UnityEngine;

namespace Character {
    public class EffectMan : MonoBehaviour {
        [SerializeField] private GameObject ganEffect;

        void Start(){
            ganEffect.SetActive(false);
        }

        public void OnTired() {
            ganEffect.SetActive(true);
        }

        public void OnTiredExit() {
            ganEffect.SetActive(false);
        }
    }
}