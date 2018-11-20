using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour {

    #region variables

    //hero stats, values set in the inspector
    public int Healthpoints = 1;
    public int Damage = 1;
    public int Defense = 1;
    public int Initiative = 1;

    public MainClass main_class = MainClass.Scout;

    private Transform selection_ring;
    private Transform targeting_ring;
    private Transform IsDrafted;

    public bool isDrafted = false;

    public bool hasAction = false;

    //used for when the hero can be targeted by an enemy hero
    public bool isTargeted = false;

    public GameObject BloodSplashPrefab;
    public GameObject BloodParticles;

    public GameObject ArrowPrefab;
    public GameObject SpellPrefab;

    //for storing the position of the hero in the level grids
    [System.NonSerialized] public int x_position_grid = 0;
    [System.NonSerialized] public int y_position_grid = 0;

    //bool for moving the hero from tile to tile
    [System.NonSerialized] public bool move_hero = false;
    //end position for the movement of the hero
    [System.NonSerialized] public Vector2 target_position;

    [System.NonSerialized] public bool attack_move_hero = false;
    [System.NonSerialized] public Vector2 original_position;

    //speed the characters move at from tile to tile
    public float movement_speed = 2f;

    //reference to text meshes
    [System.NonSerialized] public Transform UiText;
    [System.NonSerialized] public Transform UiImages;

    private TextMesh health_text;
    private TextMesh damage_text;
    private TextMesh initiative_text;

    public GameObject DamageTextPrefab;

    private GameObject target_enemy;
    private int current_damage;

    public AbilityBase HeroAbility;
    public bool isUsingAbility = false;

    private SpriteRenderer sprite_renderer;
    private AudioSource audio_source;

    public List<AudioClip> hit_sfx = new List<AudioClip>();
    public List<AudioClip> melee_sfx = new List<AudioClip>();
    #endregion

    private void Awake()
    {
        selection_ring = transform.Find("SelectionRing");
        selection_ring.gameObject.SetActive(false);

        targeting_ring = transform.Find("TargetingRing");
        targeting_ring.gameObject.SetActive(false);

        UiText = transform.Find("UI Text");
        UiImages = transform.Find("UI Images");
        SetUI(false);

        IsDrafted = transform.Find("IsDrafted");
        IsDrafted.gameObject.SetActive(false);

        health_text = UiText.Find("HealthText").GetComponent<TextMesh>();
        damage_text = UiText.Find("DamageText").GetComponent<TextMesh>();
        initiative_text = UiText.Find("InitiativeText").GetComponent<TextMesh>();

        sprite_renderer = GetComponentInChildren<SpriteRenderer>();

        audio_source = GetComponent<AudioSource>();
    }

    private void Start()
    {
        HeroAbility = HeroAbilities.instance.GenerateAbility(HeroAbilities.instance.GenerateSeed());
    }

    public void SetSelected(bool is_selected)
    {
        selection_ring.gameObject.SetActive(is_selected);

        //set movement rings on tiles in same row
        for (int i = 0; i < 3; i++)
        {
            GameManager.instance.Grid_P1.Grid[x_position_grid, i].GetComponent<GridTile>().SetMovementRing(true);
        }
    }

    public void SetTargeted(bool is_targeted)
    {
        targeting_ring.gameObject.SetActive(is_targeted);
        isTargeted = is_targeted;
    }

    public void SetDrafted(bool is_drafted)
    {
        IsDrafted.gameObject.SetActive(is_drafted);
        isDrafted = is_drafted;
    }

    public void SetAction(bool has_action)
    {
        IsDrafted.gameObject.SetActive(has_action);
        hasAction = has_action;
    }

    public void SetUI(bool show)
    {
        UiText.gameObject.SetActive(show);
        UiImages.gameObject.SetActive(show);
    }

    private void Update()
    {
        SetRenderOrder();

        health_text.text = Healthpoints.ToString();
        damage_text.text = Damage.ToString();
        initiative_text.text = Initiative.ToString();

        if (move_hero)
        {
            transform.position = Vector2.MoveTowards(transform.position, target_position, movement_speed * Time.deltaTime);

            float distance = Vector2.Distance(transform.position, target_position);

            if (distance == 0)
            {
                //disable move bool
                move_hero = false;
                //set action ended, so next animation can play
                GameManager.instance.action_ended = true;
            }
        }
        else if(attack_move_hero)
        {
            transform.position = Vector2.MoveTowards(transform.position, target_position, movement_speed * Time.deltaTime * 3);

            float distance = Vector2.Distance(transform.position, target_position);

            if (distance <= 1)
            {
                //disable attack move bool
                attack_move_hero = false;

                //play melee attack sfx
                PlaySFX("melee");

                //apply damage to target
                target_enemy.GetComponent<Hero>().TakeDamage(current_damage);

                //set target position back to original, so the hero can move back
                target_position = original_position;
                move_hero = true;
            }
        }
    }

    private void PlaySFX(string action)
    {
        int random_clip;

        switch (action)
        {
            case "melee":
                random_clip = Random.Range(0, melee_sfx.Count);
                audio_source.PlayOneShot(melee_sfx[random_clip]);
                break;
            case "hit":
                random_clip = Random.Range(0, hit_sfx.Count);
                audio_source.PlayOneShot(hit_sfx[random_clip]);
                break;
        }
    }

    float current_y;
    float old_y;

    private void SetRenderOrder()
    {
        current_y = transform.position.y;

        if(current_y != old_y)
        {
            old_y = current_y;
            sprite_renderer.sortingOrder = Mathf.RoundToInt(transform.position.y * 100f) * -1;            
        }
    }

    public void TakeDamage(int damage_value)
    {
        PlaySFX("hit");

        Healthpoints -= damage_value;
        Instantiate(BloodSplashPrefab, transform.position, Quaternion.identity);
        Instantiate(BloodParticles, transform.position, Quaternion.identity);

        GameObject damage_text = Instantiate(DamageTextPrefab, transform.position, Quaternion.identity, transform);
        damage_text.GetComponent<DamageText>().SetText("-" + damage_value, Color.red);

        if (Healthpoints <= 0)
        {
            //blood/hit effect
            Instantiate(BloodSplashPrefab, transform.position, Quaternion.identity);

            //update gridtile to no longer be occupied
            switch (gameObject.tag)
            {
                case "HeroP1":
                    GameManager.instance.Grid_P1.Grid[x_position_grid, y_position_grid]
                        .GetComponent<GridTile>().isOccupied = false;
                    break;
                case "HeroP2":
                    GameManager.instance.Grid_P2.Grid[x_position_grid, y_position_grid]
                        .GetComponent<GridTile>().isOccupied = false;
                    break;
            }
        }
    }

    GameObject projectile;

    public void RangedAttack(GameObject target, int damage)
    {        
        switch (main_class)
        {
            case MainClass.Scout:
                projectile = Instantiate(ArrowPrefab, transform.position, Quaternion.identity);
                break;
            case MainClass.Mage:
                projectile = Instantiate(SpellPrefab, transform.position, Quaternion.identity);
                break;
        }

        projectile.GetComponent<Projectile>().target = target;
        projectile.GetComponent<Projectile>().damage = damage;
    }

    public void MeleeAttack(GameObject _target, int _damage)
    {
        //save original position
        original_position = transform.position;

        //set enemy target
        target_enemy = _target;

        //set current damage
        current_damage = _damage;

        //make unit tackle enemy and then move back
        attack_move_hero = true;

        //set target position
        target_position = _target.transform.position;
    }

    private void DestroyAfterTime(float seconds)
    {
        Destroy(gameObject, seconds);
    }
}

[System.Serializable]
public class StatBuff
{
    //buff for hp, dmg or initative
    int buff_value;
    int turns;

    //updates each turn
    //if turns == 0, destroy object
}

[System.Serializable]
public class StatMultiplier
{
    float multiplier;
    int turns;
}
