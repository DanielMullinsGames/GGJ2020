using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct HealthEntry
{
    public int Value;
    public List<GameObject> On;
    public List<GameObject> Off;
}

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance;

    public int Health;
    public int MaxHealth;
    public List<HealthEntry> Entries;

    [SerializeField]
    private HealthBarUI healthBarUI;

    private void Awake()
    {
        Instance = this;
        UpdateAppearance(showUI: false);
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void ChangeHealth(int change)
    {
        Health = Mathf.Clamp(Health + change, 0, MaxHealth);
        UpdateAppearance();
    }

    private void UpdateAppearance(bool showUI = true)
    {
        foreach (var entry in Entries)
        {
            if (entry.Value == Health)
            {
                foreach (var on in entry.On)
                    on.SetActive(true);

                foreach (var off in entry.Off)
                    off.SetActive(false);
            }
        }

        if (showUI)
        {
            healthBarUI.gameObject.SetActive(true);
            healthBarUI.UpdateHealth(Health);
        }
    }
}
