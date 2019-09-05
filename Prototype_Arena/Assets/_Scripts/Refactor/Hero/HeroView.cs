using System.Collections;
using System.Collections.Generic;
using _Scripts.Refactor.Game;
using _Scripts.Refactor.Grid;
using _Scripts.Refactor.Hero.Abilities;
using _Scripts.Refactor.Projectile;
using _Scripts.Refactor.SFX;
using _Scripts.Refactor.UI;
using _Scripts.Refactor.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Refactor.Hero
{
    public class HeroView : MonoBehaviour {

        public HeroStatsController HeroStatsController { get; private set; }
        public HeroStatsModel HeroStatsModel { get; private set; }
        
        #region variables

        [FormerlySerializedAs("main_class")] public MainClass MainClass = MainClass.Scout;

        private Transform _selectionRing;
        private Transform _targetingRing;
        private Transform _isDrafted;

        [FormerlySerializedAs("isDrafted")] public bool IsHeroDrafted;
        [FormerlySerializedAs("hasAction")] public bool HasAction;

        //used for when the hero can be targeted by an enemy hero
        [FormerlySerializedAs("isTargeted")] public bool IsTargeted;

        public GameObject BloodSplashPrefab;
        public GameObject BloodParticles;

        public GameObject InstantHealParticles;

        public GameObject ArrowPrefab;
        public GameObject BouncingArrowPrefab;
        public GameObject ArrowRainPrefab;
    
        public GameObject FireballPrefab;
        public GameObject IceBurstPrefab;
        public GameObject ChainLightningPrefab;

        public GameObject WindSlashPrefab;

        //for storing the position of the hero in the level grids
        [System.NonSerialized] public int XPositionGrid = 0;
        [System.NonSerialized] public int YPositionGrid = 0;

        //bool for moving the hero from tile to tile
        [System.NonSerialized] public bool MoveHero;
        //end position for the movement of the hero
        [System.NonSerialized] public Vector2 TargetPosition;

        [System.NonSerialized] public bool AttackMoveHero;
        [System.NonSerialized] public Vector2 OriginalPosition;

        //speed the characters move at from tile to tile
        [FormerlySerializedAs("movement_speed")] public float MovementSpeed = 2f;

        //reference to text meshes
        [System.NonSerialized] public Transform UiTextTransform;
        [System.NonSerialized] public Transform UiImagesTransform;

        private TextMesh _healthText;
        private TextMesh _damageText;
        private TextMesh _initiativeText;

        public GameObject DamageTextPrefab;

        private GameObject _targetEnemy;
        private int _currentDamage;

        public AbilityBase HeroAbility;
        [FormerlySerializedAs("isUsingAbility")] public bool IsUsingAbility;

        private SpriteRenderer _spriteRenderer;

        private Transform _abilityUiTransform;

        private BoxCollider2D _draftCollider;
        private CircleCollider2D _mainCollider;
       
        private GameObject _projectile;
       
        private float _currentYPosition;
        private float _oldYPosition;

        [FormerlySerializedAs("chain_ended")] public bool HasChainEnded;
        #endregion

        private void Awake()
        {
            HeroStatsModel = new HeroStatsModel();
            HeroStatsController = new HeroStatsController(
                this,
                HeroStatsModel,
                BloodSplashPrefab,
                DamageTextPrefab);
            
            _selectionRing = transform.Find("SelectionRing");
            _selectionRing.gameObject.SetActive(false);

            _targetingRing = transform.Find("TargetingRing");
            _targetingRing.gameObject.SetActive(false);

            UiTextTransform = transform.Find("UI Text");
            UiImagesTransform = transform.Find("UI Images");

            _isDrafted = transform.Find("IsDrafted");
            _isDrafted.gameObject.SetActive(false);

            _healthText = UiTextTransform.Find("HealthText").GetComponent<TextMesh>();
            _damageText = UiTextTransform.Find("DamageText").GetComponent<TextMesh>();
            _initiativeText = UiTextTransform.Find("InitiativeText").GetComponent<TextMesh>();

            _abilityUiTransform = transform.Find("Ability");
            SetUI(false);

            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();    

            _draftCollider = GetComponent<BoxCollider2D>();
            _mainCollider = GetComponent<CircleCollider2D>();

            _mainCollider.enabled = false;
        }

        private void Start()
        {
            HeroAbility = HeroAbilities.instance.GenerateAbility(HeroAbilities.instance.GenerateSeed());
        }


        public void SetSelected(bool is_selected)
        {
            _selectionRing.gameObject.SetActive(is_selected);

            //reset using ability
            if(IsUsingAbility)
                IsUsingAbility = false;

            //set movement rings on tiles in same row
            for (var i = 0; i < 3; i++)
            {
                GameManager.Instance.GridPlayerOne.Grid[XPositionGrid, i].GetComponent<GridTile>().SetMovementRing(true);
            }

            //display hero info
            SetHeroInfo();
        }

        private void SetHeroInfo()
        {
            var ability_text =
                "Effect: " + HeroAbility.Ability_effect + "\n" +
                "Target: " + HeroAbility.Ability_target + "\n" +
                "AoE: " + HeroAbility.AbilityAreaOfEffect + "\n" +
                "Strength: " + HeroAbility.strength; 

            GameManager.Instance.SetHeroInfo(
                MainClass, 
                HeroStatsModel.HealthPoints, 
                HeroStatsModel.AttackDamage, 
                HeroStatsModel.Initiative, 
                ability_text);
        }

        public void SetTargeted(bool is_targeted)
        {
            _targetingRing.gameObject.SetActive(is_targeted);
            IsTargeted = is_targeted;
        }

        public void SetDrafted(bool is_drafted)
        {
            _isDrafted.gameObject.SetActive(is_drafted);
            IsHeroDrafted = is_drafted;

            SetHeroInfo();
        }

        public void SetAction(bool has_action)
        {
            _isDrafted.gameObject.SetActive(has_action);
            HasAction = has_action;
        }

        public void SetUI(bool show)
        {
            UiTextTransform.gameObject.SetActive(show);
            UiImagesTransform.gameObject.SetActive(show);
            _abilityUiTransform.gameObject.SetActive(show);

            if(show)
            { 
                //disable draft collider and enable main collider
                _draftCollider.enabled = false;
                _mainCollider.enabled = true;
            }
        }


        private void Update()
        {
            SetRenderOrder();

            _healthText.text = HeroStatsModel.HealthPoints.ToString();
            _damageText.text = HeroStatsModel.AttackDamage.ToString();
            _initiativeText.text = HeroStatsModel.Initiative.ToString();

            if (MoveHero)
            {
                transform.position = Vector2.MoveTowards(transform.position, TargetPosition, MovementSpeed * Time.deltaTime);

                var distance = Vector2.Distance(transform.position, TargetPosition);

                if (distance == 0)
                {
                    //disable move bool
                    MoveHero = false;
                    //set action ended, so next animation can play
                    GameManager.Instance.HasActionEnded = true;
                }
            }
            else if(AttackMoveHero)
            {
                transform.position = Vector2.MoveTowards(transform.position, TargetPosition, MovementSpeed * Time.deltaTime * 3);

                var distance = Vector2.Distance(transform.position, TargetPosition);

                if (distance <= 1)
                {
                    //disable attack move bool
                    AttackMoveHero = false;

                    //play melee attack sfx
                    SFXController.instance.PlaySFXClip("sword slash");

                    //apply damage to target
                    _targetEnemy.GetComponent<HeroView>().HeroStatsController.TakeDamage(_currentDamage);

                    //set target position back to original, so the hero can move back
                    TargetPosition = OriginalPosition;
                    MoveHero = true;
                }
            }
        }

    
        private void SetRenderOrder()
        {
            _currentYPosition = transform.position.y;

            if(_currentYPosition != _oldYPosition)
            {
                _oldYPosition = _currentYPosition;
                _spriteRenderer.sortingOrder = Mathf.RoundToInt(transform.position.y * 100f) * -1;            
            }
        }
    
        //basic attacks
        #region basic attacks
        public void RangedAttack(GameObject _target, int damage)
        {
            _projectile = Instantiate(ArrowPrefab, transform.position, Quaternion.identity);
            _projectile.GetComponent<Projectile.Projectile>().damage = damage;
            _projectile.GetComponent<Projectile.Projectile>().target = _target;
        }

        public void MagicAttack(int damage, int direction, string target_tag)
        {
            _projectile = Instantiate(FireballPrefab, transform.position, Quaternion.identity);
            _projectile.GetComponent<Projectile.Projectile>().Target_tag = target_tag;
            _projectile.GetComponent<Projectile.Projectile>().damage = damage;
            _projectile.GetComponent<Projectile.Projectile>().movement_speed *= direction;
        }

        public void MeleeAttack(GameObject _target, int _damage)
        {
            //save original position
            OriginalPosition = transform.position;

            //set enemy target
            _targetEnemy = _target;

            //set current damage
            _currentDamage = _damage;

            //make unit tackle enemy and then move back
            AttackMoveHero = true;

            //set target position
            TargetPosition = _target.transform.position;
        }
        #endregion

        //chain attacks
        #region Chain attacks
        public IEnumerator RangedAttack_Chain(GameObject _target, int damage)
        {
            //reset chain ended
            HasChainEnded = false;

            //set targets list of enemies
            //set start position and target object
            List<GameObject> targets = new List<GameObject>();
            Vector2 start_position = transform.position;
            GameObject target_object = _target;

            //instantiate prefab
            _projectile = Instantiate(BouncingArrowPrefab, start_position, Quaternion.identity);
            _projectile.GetComponent<Projectile.Projectile>().damage = damage;
            _projectile.GetComponent<Projectile.Projectile>().target = target_object;

            switch(tag)
            {
                case "HeroP1":
                    targets = GameManager.Instance.HeroListP2;
                    break;
                case "HeroP2":
                    targets = GameManager.Instance.HeroListP1;
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
                    yield return new WaitUntil(() => GameManager.Instance.HasActionEnded == true);

                    GameManager.Instance.HasActionEnded = false;

                    //set new target object
                    target_object = g;

                    _projectile = Instantiate(BouncingArrowPrefab, start_position, Quaternion.identity);
                    _projectile.GetComponent<Projectile.Projectile>().damage = damage;
                    _projectile.GetComponent<Projectile.Projectile>().target = target_object;

                    //set start position from previous hit target
                    start_position = target_object.transform.position;
                }
            }

            //set chain ended to true
            HasChainEnded = true;
        }

        public IEnumerator MagicAttack_Chain(GameObject _target, int damage)
        {
            //reset chain ended
            HasChainEnded = false;

            //set targets list of enemies
            //set start position and target object
            List<GameObject> targets = new List<GameObject>();
            Vector2 start_position = transform.position;
            GameObject target_object = _target;

            //instantiate prefab
            _projectile = Instantiate(ChainLightningPrefab, start_position, Quaternion.identity);
            _projectile.GetComponent<Projectile.Projectile>().damage = damage;
            _projectile.GetComponent<Projectile.Projectile>().target = target_object;

            switch (tag)
            {
                case "HeroP1":
                    targets = GameManager.Instance.HeroListP2;
                    break;
                case "HeroP2":
                    targets = GameManager.Instance.HeroListP1;
                    break;
            }

            //to hit next target in list, set start position to previous targe
            start_position = _target.transform.position;

            //hit target, arrow to next target
            foreach (GameObject g in targets)
            {
                //don't hit original target again
                if (g != _target)
                {
                    //wait till arrow hits target
                    yield return new WaitUntil(() => GameManager.Instance.HasActionEnded == true);

                    GameManager.Instance.HasActionEnded = false;

                    //set new target object
                    target_object = g;

                    _projectile = Instantiate(ChainLightningPrefab, start_position, Quaternion.identity);
                    _projectile.GetComponent<Projectile.Projectile>().damage = damage;
                    _projectile.GetComponent<Projectile.Projectile>().target = target_object;
                    _projectile.GetComponent<Projectile.Projectile>().movement_speed =  10;

                    //set start position from previous hit target
                    start_position = target_object.transform.position;
                }
            }

            //set chain ended to true
            HasChainEnded = true;
        }

        #endregion

        //other aoe ability attacks
        #region aoe attacks
        public IEnumerator RangedAbilityAttack(GameObject _target, int damage)
        {
            Instantiate(ArrowRainPrefab, _target.transform.position, Quaternion.identity);

            yield return new WaitForSeconds(1f);

            _target.GetComponent<HeroView>().HeroStatsController.TakeDamage(damage);
        }

        public void MeleeAbilityAttack(GameObject target, int damage)
        {
            //windslash projectile
            _projectile = Instantiate(WindSlashPrefab, transform.position, Quaternion.identity);
            _projectile.GetComponent<Projectile.Projectile>().damage = damage;
            _projectile.GetComponent<Projectile.Projectile>().target = target;
        }

        public IEnumerator MageAbilityAttack(GameObject target, int damage)
        {
            Instantiate(IceBurstPrefab, target.transform.position, Quaternion.identity);

            yield return new WaitForSeconds(1.5f);
            target.GetComponent<HeroView>().HeroStatsController.TakeDamage(damage);
        }

        #endregion
    }
}