using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Storyboard
{
    [System.Serializable]
    public class ScenarioHeader : MonoBehaviour
    {
        [SerializeField] public TextMeshProUGUI title;
        [SerializeField] public ScenarioBoard scenario;

        public void ShowScenario()
        {
            StoryboardManager.Instance.ShowScenario(scenario);
        }
        public void DestroyScenario()
        {
            StoryboardManager.Instance.RemoveScenario(this);
        }
    }
}
