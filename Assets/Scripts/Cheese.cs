using UnityEngine;

public class Cheese : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // if the player touches cheese, add score and hide the object.
        // i'm using setactive false here because it's cheap and easy.
        if (other.CompareTag("Player"))
        {
            GameManager.instance.AddCheese(1);
            gameObject.SetActive(false);
        }
    }
}