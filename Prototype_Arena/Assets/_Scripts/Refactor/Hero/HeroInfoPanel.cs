using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Refactor.Hero
{
    public class HeroInfoPanel : MonoBehaviour
    {
        [SerializeField] private Text _heroNameText;
        [SerializeField] private Text _healthPointsText;
        [SerializeField] private Text _attackDamageText;
        [SerializeField] private Text _initiativeText;
        [SerializeField] private Text _abilityNameText;

        public void SetHeroName(string name)
        {
            _heroNameText.text = name;
        }

        public void SetHealthPoints(int healthPoints)
        {
            _healthPointsText.text = healthPoints.ToString();
        }

        public void SetAttackDamage(int attackDamage)
        {
            _attackDamageText.text = attackDamage.ToString();
        }

        public void SetInitiative(int initiative)
        {
            _initiativeText.text = initiative.ToString();
        }

        public void SetAbilityName(string abilityName)
        {
            _abilityNameText.text = abilityName;
        }
    }
}