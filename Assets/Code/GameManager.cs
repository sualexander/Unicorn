using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Collections;
#endif

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject cameraPrefab;

    GameObject player, playerCamera;

    void Start()
    {
#if UNITY_EDITOR
        IEnumerator WaitForSceneView()
        {
            yield return null;
            if (SceneView.lastActiveSceneView) InitScene(SceneView.lastActiveSceneView.camera.transform.position.x);
            else {
                Debug.Log("uh oh");
                InitScene(0);
            }
        }
        StartCoroutine(WaitForSceneView());
#else
        //spawn point object
#endif
    }

    void InitScene(float spawnpoint)
    {
        player = Instantiate(playerPrefab, new Vector3(spawnpoint, 0, 0), Quaternion.identity);

        PlayerCamera old = FindFirstObjectByType<PlayerCamera>();
        if (old) Destroy(old.gameObject);
        
        playerCamera = Instantiate(cameraPrefab, new Vector3(spawnpoint, 0, -8), Quaternion.identity);
        playerCamera.GetComponent<PlayerCamera>().Init(player.transform);
    }

    void Update()
    {}
}
