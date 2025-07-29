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
        btnSpring.onClick.AddListener(() => SelectSeason(Season.Spring));
        btnSummer.onClick.AddListener(() => SelectSeason(Season.Summer));
        btnAutumn.onClick.AddListener(() => SelectSeason(Season.Autumn));
        btnWinter.onClick.AddListener(() => SelectSeason(Season.Winter));
    }

    private void SelectSeason(Season season)
    {
        SeasonManager.Instance.SetSeasonManually(season);
        Debug.Log("Estação escolhida: " + season);
    }
}
