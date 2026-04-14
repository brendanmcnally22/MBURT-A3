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
            cheeseText.text = "Cheese: " + cheeseCount;
    }

    public void ShowCaught()
    {
        if (caughtText != null)
            caughtText.gameObject.SetActive(true);

        Time.timeScale = 0f;
    }
}