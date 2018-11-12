using UnityEngine;

public class Projectile : MonoBehaviour {

    public GameObject target;
    public float movement_speed = 1f;
    public int damage;

    private Transform sprite;

    private void Awake()
    {
        sprite = transform.Find("Sprite");
    }

    // Update is called once per frame
    void Update () {
		if(target != null)
        {
            RotateTowardsPlayer();
            transform.position = Vector2.MoveTowards(transform.position, target.transform.position, movement_speed * Time.deltaTime);

            if(Vector2.Distance(transform.position, target.transform.position) == 0)
            {
                target.GetComponent<Hero>().TakeDamage(damage);
                Destroy(gameObject);
            }
        }
	}

    public void FlipSprite(bool flip)
    {
        //sprite.GetComponent<SpriteRenderer>().flipY = flip;
    }

    private void RotateTowardsPlayer()
    {
        var dir = target.transform.position - transform.position;
        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 180, Vector3.forward); // - 90
    }
}
