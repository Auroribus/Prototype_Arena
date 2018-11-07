using UnityEngine;

public class DestroyAfterTime : MonoBehaviour {

    public float seconds;
	// Use this for initialization
	void Start () {
        Destroy(gameObject, seconds);
	}
}
