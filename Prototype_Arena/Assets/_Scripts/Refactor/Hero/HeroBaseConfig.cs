using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Refactor.Hero
{
    [CreateAssetMenu(fileName = "HeroBaseConfig", menuName = "ScriptableObject/Hero/HeroBaseConfig")]
    public class HeroBaseConfig : ScriptableObject
    {
        [Serializable]
        public class HeroBase
        {
            public HeroName HeroName;
            public int HealthPoints;
            public int Damage;
            public int Initiative;
            public MainClass MainClass;
            public SubClass SubClass;
            public Sprite DraftSprite;
            public Sprite MainSprite;
        }
        
        [SerializeField] private List<HeroBase> _heroBases;
        public List<HeroBase> HeroBases
        {
            get { return _heroBases; }
        }
    }
}