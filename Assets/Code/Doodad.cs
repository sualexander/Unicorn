using UnityEngine;
using TMPro;

using System.Collections;

public class Doodad : MonoBehaviour
{
    public bool isFuture;
    public GameObject complement;

    public int durability = -1;
    TextMeshPro text;
    SpriteRenderer sprite;
    GameObject backgroundHolder;
    static Sprite backgroundSquare;

    void Start()
    {
        if (durability == -1 || isFuture) return;

        SpawnText();
        Doodad future = complement.GetComponent<Doodad>();
        future.durability = durability;
        future.SpawnText();
    }

    void OnCollisionExit2D(Collision2D other)
    {
        if (durability == -1) return;
        if (other.gameObject.GetComponent<Player>()) {
            UpdateDurability(durability - 1);
            if (!isFuture) {
                Doodad future = complement.GetComponent<Doodad>();
                future.UpdateDurability(durability);
            }
        }
    }
    
    void SpawnText()
    {
        GameObject textHolder = new GameObject(gameObject.name + "_text");
        textHolder.transform.SetParent(gameObject.transform);
        textHolder.transform.localPosition = Vector3.zero;

        text = textHolder.AddComponent<TextMeshPro>();
        text.text = durability.ToString();
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        sprite = GetComponent<SpriteRenderer>();
        textHolder.GetComponent<RectTransform>().sizeDelta = sprite.bounds.size;
        text.enableAutoSizing = true;
        text.fontSizeMax = Mathf.Min(sprite.bounds.size.x, sprite.bounds.size.y) * 8.5f;

        backgroundHolder = new GameObject(textHolder.name + "_background");
        backgroundHolder.transform.SetParent(textHolder.transform);
        backgroundHolder.transform.localPosition = Vector3.zero;

        SpriteRenderer background = backgroundHolder.AddComponent<SpriteRenderer>();
        if (!backgroundSquare) {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.black);
            texture.Apply();
            backgroundSquare = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        }
        background.sprite = backgroundSquare;

        background.sortingOrder = sprite.sortingOrder + 1;
        text.sortingOrder = sprite.sortingOrder + 2;

        StartCoroutine(UpdateBackgroundSize());
    }

    void UpdateDurability(int inDurability)
    {
        durability = inDurability;
        GetComponent<Collider2D>().enabled = durability != 0;
        backgroundHolder.SetActive(durability != 0);
        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, durability == 0 ? 0.2f : 1);
        text.text = durability.ToString();
        StartCoroutine(UpdateBackgroundSize());
    }

    IEnumerator UpdateBackgroundSize()
    {
        yield return null;
        Vector2 textSize = text.GetRenderedValues(false);
        backgroundHolder.transform.localScale = new Vector3(textSize.x * 1.2f, textSize.y * 0.8f, 1);
    }
}
