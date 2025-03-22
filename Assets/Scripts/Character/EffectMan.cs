using UnityEngine;

namespace Character {
    public class EffectMan : MonoBehaviour {
        [SerializeField] private GameObject ganEffect;

        public void OnTired() {
            ganEffect.SetActive(true);
        }

        public void OnTiredExit() {
            ganEffect.SetActive(false);
        }
    }
}