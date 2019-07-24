using UnityEngine;

namespace _Scripts.Refactor.UI
{
    public class DamageText : MonoBehaviour {

        private TextMesh text_value;
        public int destroy_time;

        private Renderer rend;

        private void Awake()
        {
            text_value = GetComponent<TextMesh>();
            rend = GetComponent<Renderer>();
        }

        private void Start()
        {
            Destroy(gameObject, destroy_time);

            rend.sortingLayerName = "Foreground";
            rend.sortingOrder = 10;
        }

        public void SetText(string value, Color color)
        {
            text_value.text = value;
            text_value.color = color;
        }
    }
}
