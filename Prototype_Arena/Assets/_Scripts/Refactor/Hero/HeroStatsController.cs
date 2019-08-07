using _Scripts.Refactor.Game;
using _Scripts.Refactor.Grid;
using _Scripts.Refactor.SFX;
using _Scripts.Refactor.UI;
using _Scripts.Refactor.Utility;
using UnityEngine;

namespace _Scripts.Refactor.Hero
{
    public class HeroStatsController
    {
        private HeroView _heroView;
        private HeroStatsModel _heroStatsModel;
        private CameraShake _cameraShake;
        private GameObject _bloodParticlesPrefab;
        private GameObject _damageTextPrefab;
        private GameObject _instantHealPrefab;

        public HeroStatsController(
            HeroView heroView,
            HeroStatsModel heroStatsModel,
            GameObject bloodParticlesPrefab,
            GameObject damageTextPrefab)
        {
            _heroView = heroView;
            _heroStatsModel = heroStatsModel;
            _bloodParticlesPrefab = bloodParticlesPrefab;
            _damageTextPrefab = damageTextPrefab;

            _cameraShake = Camera.main.GetComponent<CameraShake>();
        }

        public void TakeDamage(int damageTaken)
        {
            _cameraShake.shakeDuration = .2f;
            SFXController.instance.PlaySFXClip("hit");

            var healthPoints = _heroStatsModel.HealthPoints;
            healthPoints -= damageTaken;
            _heroStatsModel.SetHealthPoints(healthPoints);

            Object.Instantiate(_bloodParticlesPrefab, _heroView.transform.position, Quaternion.identity);
            var damage_text = Object.Instantiate(
                _damageTextPrefab, 
                _heroView.transform.position, 
                Quaternion.identity,
                _heroView.transform);
            damage_text.GetComponent<DamageText>().SetText("-" + damageTaken, Color.red);

            if (_heroStatsModel.HealthPoints <= 0)
            {
                var xPositionInGrid = _heroStatsModel.XPositionInGrid;
                var yPositionInGrid = _heroStatsModel.YPositionInGrid;

                //update gridtile to no longer be occupied
                switch (_heroView.gameObject.tag)
                {
                    case "HeroP1":
                        GameManager.Instance.GridPlayerOne.Grid[xPositionInGrid, yPositionInGrid]
                            .GetComponent<GridTile>().isOccupied = false;
                        break;
                    case "HeroP2":
                        GameManager.Instance.GridPlayerTwo.Grid[xPositionInGrid, yPositionInGrid]
                            .GetComponent<GridTile>().isOccupied = false;
                        break;
                }
            }
        }

        public void HealHero(float heal_value)
        {
            var healthpoints = _heroStatsModel.HealthPoints;
            healthpoints += (int) heal_value;
            _heroStatsModel.SetHealthPoints(healthpoints);

            //heal vfx
            Object.Instantiate(_instantHealPrefab, _heroView.transform.position, Quaternion.identity);

            var healTextPrefab = Object.Instantiate(
                _damageTextPrefab, 
                _heroView.transform.position,
                Quaternion.identity, 
                _heroView.transform);
            healTextPrefab.GetComponent<DamageText>().SetText("+" + heal_value, Color.green);
        }
    }
}