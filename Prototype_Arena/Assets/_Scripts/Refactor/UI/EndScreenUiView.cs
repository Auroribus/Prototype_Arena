using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Refactor.UI
{
    public class EndScreenUiView : MonoBehaviour

    {
    [SerializeField] private Text _winnersNameText;

    public Text WinnersNameText
    {
        get { return _winnersNameText; }
    }
    }
}