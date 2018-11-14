using UnityEngine;

public class Projectile : MonoBehaviour {

    public GameObject target;
    public float movement_speed = 1f;
    public int damage;
    public float rotation_correction = 180f;
    public bool rotate_towards_target = false;
    
    // Update is called once per frame
    void Update () {
		if(target != null)
        {
            if(rotate_towards_target)
                RotateTowardsTarget();

            transform.position = Vector2.MoveTowards(transform.position, target.transform.position, movement_speed * Time.deltaTime);

            if(Vector2.Distance(transform.position, target.transform.position) == 0)
            {
                target.GetComponent<Hero>().TakeDamage(damage);

                //projectile hit, action ends
                GameManager.instance.action_ended = true; 

                Destroy(gameObject);
            }
        }
	}

    private void RotateTowardsTarget()
    {
        var dir = target.transform.position - transform.position;
        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - rotation_correction, Vector3.forward); // - 90
    }
}
