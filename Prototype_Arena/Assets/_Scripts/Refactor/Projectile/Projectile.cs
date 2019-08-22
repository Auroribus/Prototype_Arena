using _Scripts.Refactor.Game;
using _Scripts.Refactor.Hero;
using _Scripts.Refactor.SFX;
using UnityEngine;

namespace _Scripts.Refactor.Projectile
{
    public class Projectile : MonoBehaviour
    {
        public GameObject target;
        public float movement_speed = 1f;
        public int damage;
        public float rotation_correction = 180f;
        public bool rotate_towards_target;

        public GameObject HitEffect;

        public ProjectileType Projectile_type;
        public string Target_tag;

        public bool projectile_reached_end;

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
                case ProjectileType.BounceArrow:
                    //SFXController.instance.PlaySFXClip("boomerang");
                    break;
            }
        }

        // Update is called once per frame
        void Update()
        {
            switch (Projectile_type)
            {
                case ProjectileType.Arrow:
                    if (rotate_towards_target)
                        RotateTowardsTarget();

                    transform.position = Vector2.MoveTowards(transform.position, target.transform.position,
                        movement_speed * Time.deltaTime);

                    if (Vector2.Distance(transform.position, target.transform.position) == 0)
                    {
                        target.GetComponent<HeroView>().HeroStatsController.TakeDamage(damage);

                        //projectile hit, action ends
                        GameManager.Instance.HasActionEnded = true;
                        Instantiate(HitEffect, transform.position, Quaternion.identity);
                        Destroy(gameObject);
                    }

                    break;
                case ProjectileType.BounceArrow:

                    transform.position = Vector2.MoveTowards(transform.position, target.transform.position,
                        movement_speed * Time.deltaTime);

                    if (Vector2.Distance(transform.position, target.transform.position) == 0)
                    {
                        target.GetComponent<HeroView>().HeroStatsController.TakeDamage(damage);

                        //projectile hit, action ends
                        GameManager.Instance.HasActionEnded = true;

                        Instantiate(HitEffect, transform.position, Quaternion.identity);
                        Destroy(gameObject);
                    }

                    break;
                case ProjectileType.Fireball:

                    transform.position = new Vector2(transform.position.x + (movement_speed * Time.deltaTime),
                        transform.position.y);

                    break;

                case ProjectileType.WindSlash:

                    if (rotate_towards_target)
                        RotateTowardsTarget();

                    transform.position = Vector2.MoveTowards(transform.position, target.transform.position,
                        movement_speed * Time.deltaTime);

                    if (Vector2.Distance(transform.position, target.transform.position) == 0)
                    {
                        target.GetComponent<HeroView>().HeroStatsController.TakeDamage(damage);
                        Instantiate(HitEffect, transform.position, Quaternion.identity);
                        Destroy(gameObject);
                    }

                    break;

                case ProjectileType.ChainLightning:

                    transform.position = Vector2.MoveTowards(transform.position, target.transform.position,
                        movement_speed * Time.deltaTime);

                    if (Vector2.Distance(transform.position, target.transform.position) == 0)
                    {
                        target.GetComponent<HeroView>().HeroStatsController.TakeDamage(damage);

                        //projectile hit, action ends
                        GameManager.Instance.HasActionEnded = true;

                        Instantiate(HitEffect, transform.position, Quaternion.identity);
                        Destroy(gameObject);
                    }

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
                case ProjectileType.Fireball:
                    if (collision.tag == Target_tag)
                    {
                        collision.transform.GetComponent<HeroView>().HeroStatsController.TakeDamage(damage);
                        Instantiate(HitEffect, collision.transform.position, Quaternion.identity);
                    }

                    break;
            }
        }
    }
}