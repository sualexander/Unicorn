using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    Transform target;
    public void Init(Transform player)
    {
        target = player;
        this.enabled = true;
        GetComponentInChildren<SpriteRenderer>().enabled = true;
    }

    void LateUpdate()
    {
        transform.position = new Vector3(target.position.x, transform.position.y, -10);
    }
}
