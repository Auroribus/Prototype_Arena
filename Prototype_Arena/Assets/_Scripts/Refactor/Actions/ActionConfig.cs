using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Scripts.Refactor.Actions
{
    [CreateAssetMenu(fileName = "ActionConfig", menuName = "ScriptableObject/Action/ActionConfig")]
    public class ActionConfig : ScriptableObject
    {
        [Serializable]
        public class ActionIconSetting
        {
            public ActionType ActionType;
            public Sprite ActionSprite;
        }

        [SerializeField] private List<ActionIconSetting> _actionIconSettings;
        
        public Sprite GetActionSprite(ActionType actionType)
        {
            return _actionIconSettings.First(setting => 
                setting.ActionType == actionType).ActionSprite;
        }
    }
}