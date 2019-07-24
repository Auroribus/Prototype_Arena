using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Refactor.Hero
{
    public class HeroInfoPanel : MonoBehaviour
    {
        [SerializeField] private Text _heroNameText;
        [SerializeField] private Text _healthPointsText;
        [SerializeField] private Text _damageText;
        [SerializeField] private Text _initiativeText;
        [SerializeField] private Text _abilityNameText;

//        hero_name = hero_info_panel.Find("Hero_name").GetComponent<Text>();
//        hero_hp = hero_info_panel.Find("HP_value").GetComponent<Text>();
//        hero_dmg = hero_info_panel.Find("DMG_value").GetComponent<Text>();
//        hero_init = hero_info_panel.Find("INIT_value").GetComponent<Text>();
//        hero_abil = hero_info_panel.Find("ABIL_name").GetComponent<Text>();
    }
}