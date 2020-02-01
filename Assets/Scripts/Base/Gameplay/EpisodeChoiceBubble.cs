using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class EpisodeChoiceBubble : MonoBehaviour, IPointerClickHandler
{
    public static List<EpisodeChoiceBubble> CurrentBubbles = new List<EpisodeChoiceBubble>();

    public EpisodeChoice Choice;
    public List<TextMeshPro> Texts;
    public bool IsDark;
    public GameObject NormalObject;
    public GameObject DarkObject;

    private Vector2 vel;

    private void Awake()
    {
        CurrentBubbles.Add(this);
    }

    private void OnDestroy()
    {
        CurrentBubbles.Remove(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameStateManager.Instance.SelectChoice(Choice);
    }

    public void SetChoice(EpisodeChoice choice)
    {
        Choice = choice;

        foreach(var text in Texts)
            text.text = Choice.Text;
    }

    public void SelectChoice()
    {
        GameStateManager.Instance.SelectChoice(Choice);
    }

    private void Update()
    {
        GetComponent<Rigidbody2D>().velocity = vel;

        if (!GameStateManager.Instance.Playing)
            Destroy(gameObject);
    }

    public void SetTargetVelocity(Vector2 velToUse)
    {
        vel = velToUse;
    }

    public void SetDark(EpisodeChoice choice)
    {
        if (IsDark)
            return;

        SetChoice(choice);
        IsDark = true;
        NormalObject.SetActive(false);
        DarkObject.SetActive(true);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.GetComponent<Bullet>() != null)
        {
            collision.collider.gameObject.GetComponent<Bullet>().TriggerHit();
            GameStateManager.Instance.SelectChoice(Choice);
            Destroy(gameObject);
        }
        if (collision.collider.gameObject.GetComponent<Gremlin>() != null)
        {
            SetDark(collision.collider.gameObject.GetComponent<Gremlin>().DarkChoice);
            collision.collider.gameObject.GetComponent<Gremlin>().TriggerHit();
        }
    }
}
