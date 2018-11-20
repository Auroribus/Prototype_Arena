using UnityEngine;

public class ActionPrefab : MonoBehaviour {

    public Transform Checkmark;
    public SpriteRenderer check_sprite;
    public PlayerTurn player;

	// Use this for initialization
	void Start () {
        Checkmark = transform.Find("Checkmark");
        check_sprite = Checkmark.GetComponent<SpriteRenderer>();

        Checkmark.gameObject.SetActive(false);
	}

    public void SetCheckmark(bool is_done, Color color)
    {
        Checkmark.gameObject.SetActive(is_done);
        check_sprite.color = color;
    }
}
