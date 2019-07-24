using UnityEngine;

namespace _Scripts.Refactor.Utility
{
	public class DestroyAfterTime : MonoBehaviour {

		public float seconds;
		// Use this for initialization
		void Start () {
			Destroy(gameObject, seconds);
		}
	}
}
