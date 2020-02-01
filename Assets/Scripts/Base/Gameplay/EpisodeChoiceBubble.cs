using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class EpisodeChoiceBubble : MonoBehaviour, IPointerClickHandler
{
    public EpisodeChoice Choice;
    public TextMeshPro Text;

    public void OnPointerClick(PointerEventData eventData)
    {
        GameStateManager.Instance.SelectChoice(Choice);
    }

    public void SetChoice(EpisodeChoice choice)
    {
        Choice = choice;
        Text.text = Choice.Text;
    }
}
