using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Pixelplacement;

public class EpisodeChoiceBubble : MonoBehaviour, IPointerClickHandler
{
    public static List<EpisodeChoiceBubble> CurrentBubbles = new List<EpisodeChoiceBubble>();

    public EpisodeChoice Choice;
    public List<TextMeshPro> Texts;
    public bool IsDark;
    public GameObject NormalObject;
    public GameObject DarkObject;

    private Vector2 vel;

    [SerializeField]
    private SpriteRenderer leftGlow;

    [SerializeField]
    private SpriteRenderer rightGlow;

    [SerializeField]
    private SpriteRenderer bubble;

    [SerializeField]
    private Sprite brainySprite;

    [SerializeField]
    private Color brainyTextColor;

    private bool isSelectedChoice;

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
        SelectChoice();
    }

    public void SetChoice(EpisodeChoice choice)
    {
        Choice = choice;

        foreach(var text in Texts)
            text.text = Choice.Text;

        ShowColors(choice);
    }

    private void ShowColors(EpisodeChoice choice)
    {
        if (choice.Scores.Count == 1)
        {
            leftGlow.enabled = rightGlow.enabled = true;
            leftGlow.color = rightGlow.color = choice.Scores[0].Type.Color;
        }
        else if (choice.Scores.Count == 2)
        {
            leftGlow.enabled = rightGlow.enabled = true;
            leftGlow.color = choice.Scores[0].Type.Color;
            rightGlow.color = choice.Scores[1].Type.Color;
        }
        else
        {
            leftGlow.enabled = rightGlow.enabled = false;
        }
    }

    public void SelectChoice()
    {
        GameStateManager.Instance.SelectChoice(this);
    }

    private void Update()
    {
        GetComponent<Rigidbody2D>().velocity = vel;

        if (!GameStateManager.Instance.Playing && !isSelectedChoice)
            CleanUp();
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
    
    public void SetBrainy()
    {
        bubble.sprite = brainySprite;
        bubble.sortingLayerName = "UI";
        foreach (TextMeshPro text in Texts)
        {
            text.sortingLayerID = bubble.sortingLayerID;
            text.color = brainyTextColor;
        }
    }

    public void CleanUp()
    {
        enabled = false;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        Tween.LocalScale(transform, Vector3.zero, 0.2f, 0f, Tween.EaseIn);
        Destroy(gameObject, 0.2f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.GetComponent<Bullet>() != null)
        {
            collision.collider.gameObject.GetComponent<Bullet>().TriggerHit();

            enabled = false;
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

            SelectChoice();
        }
        if (collision.collider.gameObject.GetComponent<Gremlin>() != null)
        {
            SetDark(collision.collider.gameObject.GetComponent<Gremlin>().DarkChoice);
            collision.collider.gameObject.GetComponent<Gremlin>().TriggerHit();
        }
    }
}
