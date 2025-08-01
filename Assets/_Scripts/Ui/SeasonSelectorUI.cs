using UnityEngine;
using UnityEngine.UI;

public class SeasonSelectorUI : MonoBehaviour
{
    public Button btnSpring;
    public Button btnSummer;
    public Button btnAutumn;
    public Button btnWinter;

    private void Start()
    {
        btnSpring.onClick.AddListener(() => SeasonManager.Instance.TryChangeSeason(Season.Spring));
        btnSummer.onClick.AddListener(() => SeasonManager.Instance.TryChangeSeason(Season.Summer));
        btnAutumn.onClick.AddListener(() => SeasonManager.Instance.TryChangeSeason(Season.Autumn));
        btnWinter.onClick.AddListener(() => SeasonManager.Instance.TryChangeSeason(Season.Winter));
    }
}
