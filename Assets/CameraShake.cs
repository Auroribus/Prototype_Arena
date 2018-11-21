using UnityEngine;

public class CameraShake : MonoBehaviour {

    #region Variables
    
    //public Transform camTransform;

    // How long the object should shake for.
    public float shakeDuration = 3f;

    // Amplitude of the shake. A larger value shakes the camera harder.
    public float shakeAmount = 0.7f;
    public float decreaseFactor = 1.0f;

    Vector3 originalPos;

    #endregion

    private void Awake()
    {
        
    }

    // Use this for initialization
    void Start () {
        originalPos = transform.localPosition;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        ShakeCamera();
    }

    private void ShakeCamera()
    {
        if (shakeDuration > 0)
        {
            transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;
            //Debug.Log(camTransform.localPosition);
            shakeDuration -= Time.deltaTime * decreaseFactor;
            //Debug.Log(shakeDuration);
        }
        else
        {
            shakeDuration = 0f;
            transform.localPosition = originalPos;
        }
    }
}
