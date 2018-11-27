using UnityEngine;

public class Projectile : MonoBehaviour {

    public GameObject target;
    public float movement_speed = 1f;
    public int damage;
    public float rotation_correction = 180f;
    public bool rotate_towards_target = false;

    public GameObject FireBurst;
    public GameObject BounceHit;

    public ProjectileType Projectile_type;
    public string Target_tag;

    public bool projectile_reached_end = false;

    private void Start()
    {
        switch (Projectile_type)
        {
            case ProjectileType.Arrow:
                SFXController.instance.PlaySFXClip("shoot arrow");
                break;
            case ProjectileType.Fireball:
                SFXController.instance.PlaySFXClip("fire spell");
                break;
        }
    }

    // Update is called once per frame
    void Update () {

        switch (Projectile_type)
        {
            case ProjectileType.Arrow:
                if (rotate_towards_target)
                    RotateTowardsTarget();

                transform.position = Vector2.MoveTowards(transform.position, target.transform.position, movement_speed * Time.deltaTime);

                if (Vector2.Distance(transform.position, target.transform.position) == 0)
                {
                    target.GetComponent<Hero>().TakeDamage(damage);

                    //projectile hit, action ends
                    GameManager.instance.action_ended = true;
                                       
                    Destroy(gameObject);
                }

                break;
            case ProjectileType.BounceArrow:

                transform.position = Vector2.MoveTowards(transform.position, target.transform.position, movement_speed * Time.deltaTime);

                if (Vector2.Distance(transform.position, target.transform.position) == 0)
                {
                    target.GetComponent<Hero>().TakeDamage(damage);

                    //projectile hit, action ends
                    GameManager.instance.action_ended = true;

                    Instantiate(BounceHit, transform.position, Quaternion.identity);
                    Destroy(gameObject);
                }

                break;
            case ProjectileType.Fireball:

                transform.position = new Vector2(transform.position.x + (movement_speed * Time.deltaTime), transform.position.y);

                break;
        }
	}

    private void RotateTowardsTarget()
    {
        var dir = target.transform.position - transform.position;
        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - rotation_correction, Vector3.forward); // - 90
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (Projectile_type)
        {
            case ProjectileType.Arrow:

                break;
            case ProjectileType.Fireball:
                if (collision.tag == Target_tag)
                {
                    collision.transform.GetComponent<Hero>().TakeDamage(damage);
                    Instantiate(FireBurst, collision.transform.position, Quaternion.identity);
                }
                break;
        }
    }
}
