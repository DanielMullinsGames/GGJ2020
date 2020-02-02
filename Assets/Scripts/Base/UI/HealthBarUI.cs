using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField]
    private TMPro.TextMeshPro text;

    [SerializeField]
    private TMPro.TextMeshPro shadow;

    public AudioSource HealthSound;

    private int health;

    private void Awake()
    {
        health = HealthManager.Instance.Health;
    }

    public void UpdateHealth(int health)
    {
        if (this.health != health)
        {
            this.health = health;
            gameObject.SetActive(true);
            GetComponent<Animator>().Play("show_health", 0, 0f);
            HealthSound.Play();
        }
        else
        {
            text.text = shadow.text = health.ToString();
            gameObject.SetActive(false);
        }
    }

    private void UpdateTextKeyframe()
    {
        text.text = shadow.text = health.ToString();
    }
}
