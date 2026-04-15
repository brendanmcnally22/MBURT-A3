using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI")]
    public TextMeshProUGUI cheeseText;
    public TextMeshProUGUI caughtText;

    int cheeseCount = 0;

    void Awake()
    {
        // simple singleton setup because i only want one manager talking to the ui.
        instance = this;
    }

    void Start()
    {
        UpdateCheeseUI();

        if (caughtText != null)
            caughtText.gameObject.SetActive(false);
    }

    public void AddCheese(int amount)
    {
        cheeseCount += amount;
        UpdateCheeseUI();
    }

    void UpdateCheeseUI()
    {
        if (cheeseText != null)
            cheeseText.text = "cheese: " + cheeseCount;
    }

    public void ShowCaught()
    {
        if (caughtText != null)
            caughtText.gameObject.SetActive(true);

        // freezing time here makes the fail state really obvious and stops extra weirdness after getting caught.
        Time.timeScale = 0f;
    }
}