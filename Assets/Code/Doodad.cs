using UnityEngine;
using TMPro;

public class Doodad : MonoBehaviour
{
    public bool isFuture;
    public GameObject complement;

    public int durability = -1;
    TextMeshPro text;
    SpriteRenderer sprite;

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
        text.color = Color.black;

        sprite = GetComponent<SpriteRenderer>();
        textHolder.GetComponent<RectTransform>().sizeDelta = sprite.bounds.size;
        text.enableAutoSizing = true;
        text.fontSizeMax = sprite.bounds.size.y * 8;
    }

    void UpdateDurability(int inDurability)
    {
        durability = inDurability;
        GetComponent<Collider2D>().enabled = durability == 0 ? false : true;
        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, durability == 0 ? 0.2f : 1);
        text.text = durability.ToString();
    }
}
