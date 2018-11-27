using System.Collections;
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

    public GameObject InstantHealParticles;

    public GameObject ArrowPrefab;
    public GameObject BouncingArrowPrefab;
    public GameObject ArrowRainPrefab;
    
    public GameObject FireballPrefab;
    public GameObject IceBurstPrefab;

    public GameObject WindSlashPrefab;

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

    private Transform AbilityUI;

    private CameraShake cam_shake;

    private BoxCollider2D draft_collider;
    private CircleCollider2D main_collider;
       
    private GameObject projectile;
       
    private float current_y;
    private float old_y;

    public bool chain_ended = false;
    #endregion

    private void Awake()
    {
        selection_ring = transform.Find("SelectionRing");
        selection_ring.gameObject.SetActive(false);

        targeting_ring = transform.Find("TargetingRing");
        targeting_ring.gameObject.SetActive(false);

        UiText = transform.Find("UI Text");
        UiImages = transform.Find("UI Images");

        IsDrafted = transform.Find("IsDrafted");
        IsDrafted.gameObject.SetActive(false);

        health_text = UiText.Find("HealthText").GetComponent<TextMesh>();
        damage_text = UiText.Find("DamageText").GetComponent<TextMesh>();
        initiative_text = UiText.Find("InitiativeText").GetComponent<TextMesh>();

        AbilityUI = transform.Find("Ability");
        SetUI(false);

        sprite_renderer = GetComponentInChildren<SpriteRenderer>();        
        cam_shake = Camera.main.transform.GetComponent<CameraShake>();

        draft_collider = GetComponent<BoxCollider2D>();
        main_collider = GetComponent<CircleCollider2D>();

        main_collider.enabled = false;
    }

    private void Start()
    {
        HeroAbility = HeroAbilities.instance.GenerateAbility(HeroAbilities.instance.GenerateSeed());
    }


    public void SetSelected(bool is_selected)
    {
        selection_ring.gameObject.SetActive(is_selected);

        //reset using ability
        if(isUsingAbility)
            isUsingAbility = false;

        //set movement rings on tiles in same row
        for (int i = 0; i < 3; i++)
        {
            GameManager.instance.Grid_P1.Grid[x_position_grid, i].GetComponent<GridTile>().SetMovementRing(true);
        }

        //display hero info
        SetHeroInfo();
    }

    private void SetHeroInfo()
    {
        string ability_text =
            "Effect: " + HeroAbility.Ability_effect + "\n" +
            "Target: " + HeroAbility.Ability_target + "\n" +
            "AoE: " + HeroAbility.Ability_aoe + "\n" +
            "Strength: " + HeroAbility.strength; //+ "\n" +
                                                 //"Duration: " + ability.duration + "\n" +
                                                 //"Delay: " + ability.delay;

        GameManager.instance.SetHeroInfo(main_class, Healthpoints, Damage, Initiative, ability_text);
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

        SetHeroInfo();
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
        AbilityUI.gameObject.SetActive(show);

        if(show)
        { 
            //disable draft collider and enable main collider
            draft_collider.enabled = false;
            main_collider.enabled = true;
        }
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
                SFXController.instance.PlaySFXClip("sword slash");

                //apply damage to target
                target_enemy.GetComponent<Hero>().TakeDamage(current_damage);

                //set target position back to original, so the hero can move back
                target_position = original_position;
                move_hero = true;
            }
        }
    }

    
    private void SetRenderOrder()
    {
        current_y = transform.position.y;

        if(current_y != old_y)
        {
            old_y = current_y;
            sprite_renderer.sortingOrder = Mathf.RoundToInt(transform.position.y * 100f) * -1;            
        }
    }
    

    public void TakeDamage(float damage_value)
    {
        //shake camera
        cam_shake.shakeDuration = .2f;

        SFXController.instance.PlaySFXClip("hit");

        float amount = damage_value;

        Healthpoints -= (int)amount;
        //Instantiate(BloodSplashPrefab, transform.position, Quaternion.identity);
        Instantiate(BloodParticles, transform.position, Quaternion.identity);

        GameObject damage_text = Instantiate(DamageTextPrefab, transform.position, Quaternion.identity, transform);
        damage_text.GetComponent<DamageText>().SetText("-" + amount, Color.red);

        if (Healthpoints <= 0)
        {
            //blood/hit effect
            //Instantiate(BloodSplashPrefab, transform.position, Quaternion.identity);

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

    public void HealHero(float heal_value)
    {
        //PlaySFX("hit");

        float amount = heal_value;

        Healthpoints += (int)amount;

        //heal vfx
        Instantiate(InstantHealParticles, transform.position, Quaternion.identity);

        GameObject heal_text = Instantiate(DamageTextPrefab, transform.position, Quaternion.identity, transform);
        heal_text.GetComponent<DamageText>().SetText("+" + amount, Color.green);
        
    }

    //basic attacks
    #region basic attacks
    public void RangedAttack(GameObject _target, int damage)
    {
        projectile = Instantiate(ArrowPrefab, transform.position, Quaternion.identity);
        projectile.GetComponent<Projectile>().damage = damage;
        projectile.GetComponent<Projectile>().target = _target;
    }

    public void MagicAttack(int damage, int direction, string target_tag)
    {
        projectile = Instantiate(FireballPrefab, transform.position, Quaternion.identity);
        projectile.GetComponent<Projectile>().Target_tag = target_tag;
        projectile.GetComponent<Projectile>().damage = damage;
        projectile.GetComponent<Projectile>().movement_speed *= direction;
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
    #endregion

    //chain attacks
    #region Chain attacks
    public IEnumerator RangedAttack_Chain(GameObject _target, int damage)
    {
        //reset chain ended
        chain_ended = false;

        //set targets list of enemies
        //set start position and target object
        List<GameObject> targets = new List<GameObject>();
        Vector2 start_position = transform.position;
        GameObject target_object = _target;

        //instantiate prefab
        projectile = Instantiate(BouncingArrowPrefab, start_position, Quaternion.identity);
        projectile.GetComponent<Projectile>().damage = damage;
        projectile.GetComponent<Projectile>().target = target_object;

        switch(tag)
        {
            case "HeroP1":
                targets = GameManager.instance.HeroList_P2;
                break;
            case "HeroP2":
                targets = GameManager.instance.HeroList_P1;
                break;
        }

        //to hit next target in list, set start position to previous targe
        start_position = _target.transform.position;
        
        //hit target, arrow to next target
        foreach (GameObject g in targets)
        {
            //don't hit original target again
            if(g != _target)
            { 
                //wait till arrow hits target
                yield return new WaitUntil(() => GameManager.instance.action_ended == true);

                GameManager.instance.action_ended = false;

                //set new target object
                target_object = g;

                projectile = Instantiate(BouncingArrowPrefab, start_position, Quaternion.identity);
                projectile.GetComponent<Projectile>().damage = damage;
                projectile.GetComponent<Projectile>().target = target_object;

                //set start position from previous hit target
                start_position = target_object.transform.position;
            }
        }

        //set chain ended to true
        chain_ended = true;
    }

    public void MagicAttack_Chain(GameObject _target, int damage)
    {
        
    }

    public void MeleeAttack_Chain()
    {

    }
    #endregion

    //other aoe ability attacks
    #region aoe attacks
    public IEnumerator RangedAbilityAttack(GameObject _target, int damage)
    {
        Instantiate(ArrowRainPrefab, _target.transform.position, Quaternion.identity);

        yield return new WaitForSeconds(1f);

        _target.GetComponent<Hero>().TakeDamage(damage);
    }

    public void MeleeAbilityAttack(GameObject _target, int damage)
    {
        //windslash projectile
        projectile = Instantiate(WindSlashPrefab, transform.position, Quaternion.identity);
        projectile.GetComponent<Projectile>().damage = damage;
        projectile.GetComponent<Projectile>().target = _target;
    }

    public IEnumerator MageAbilityAttack(GameObject _target, int damage)
    {
        Instantiate(IceBurstPrefab, _target.transform.position, Quaternion.identity);

        yield return new WaitForSeconds(1.5f);
        _target.GetComponent<Hero>().TakeDamage(damage);
    }

    #endregion

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
