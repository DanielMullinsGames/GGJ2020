using UnityEngine;
using System.Collections;

public class MakeStaticOnNotPlaying : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer spriteRendererToChange;

    [SerializeField]
    private Sprite activeSprite;

    [SerializeField]
    private Sprite staticSprite;
    
    void Update()
    {
        if (!GameStateManager.Instance.Playing)
        {
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            SetSpriteStatic(true);
        }
        else
        {
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            SetSpriteStatic(false);
        }
    }

    void SetSpriteStatic(bool isStatic)
    {
        if (spriteRendererToChange != null)
        {
            spriteRendererToChange.sprite = isStatic ? staticSprite : activeSprite;
        }
    }
}
