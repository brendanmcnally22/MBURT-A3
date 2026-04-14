using UnityEngine;

public class Cheese : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.instance.AddCheese(1);
            gameObject.SetActive(false);
        }
    }
}