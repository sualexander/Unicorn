using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

using System.Linq;
using System.Reflection;

[InitializeOnLoad]
public class LevelEditor
{
    static LevelEditor()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorSceneManager.sceneOpened -= OnSceneOpened;
        EditorSceneManager.sceneOpened += OnSceneOpened;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;

        InitEditor();
    }

    static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        var type = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.LogEntries");
        type.GetMethod("Clear").Invoke(null, null);

        InitEditor();
    }

    static void OnPlayModeChanged(PlayModeStateChange type)
    {
        if (type == PlayModeStateChange.ExitingPlayMode) InitEditor();
    }

    static void OnObjectChange(ref ObjectChangeEventStream stream)
    {
        int justSpawned = -1;

        for (int i = 0; i < stream.length; ++i) {
            ObjectChangeKind kind = stream.GetEventType(i);
            switch (kind) {
            case ObjectChangeKind.ChangeGameObjectOrComponentProperties:
                stream.GetChangeGameObjectOrComponentPropertiesEvent(i, out var changed);
                Object obj = EditorUtility.InstanceIDToObject(changed.instanceId);
                if (obj is GameObject) {
                    GameObject gameObject = obj as GameObject;
                    Doodad doodad = gameObject.GetComponent<Doodad>();
                    if (doodad && !doodad.isFuture) {
                        Object.DestroyImmediate(doodad.complement);
                        CreateFuture(gameObject);
                        Debug.Log("Updated " + gameObject.name);
                    }
                } else if (obj is Transform) {
                    Transform baseTransform = obj as Transform;

                    Transform[] transforms = baseTransform.GetComponentsInChildren<Transform>(true);
                    foreach (Transform transform in transforms) 
                    {
                        Doodad doodad = transform.gameObject.GetComponent<Doodad>();
                        if (transform.position.y > 0) {
                            if (doodad && !doodad.isFuture) {
                                foreach (Transform child in doodad.complement.transform)
                                {
                                    child.SetParent(null);
                                }
                                Object.DestroyImmediate(doodad.complement);
                                Object.DestroyImmediate(doodad);
                            }
                            continue;
                        }
                        if (!doodad) {
                            CreateFuture(transform.gameObject);
                        } else if (!doodad.isFuture) {
                            Transform futureTransform = doodad.complement.transform;
                            futureTransform.position = new Vector3(transform.position.x, transform.position.y * -1, transform.position.z);
                            futureTransform.rotation = transform.rotation;
                            futureTransform.localScale = transform.localScale * -1;
                        }
                    }
                } else if (obj is LevelSettings) {
                    LevelSettings settings = obj as LevelSettings;
                    UnicornEditor.Update(settings.isMirrored);
                    ToggleMirrorInternal(settings.isMirrored);
                }
                break;
            case ObjectChangeKind.CreateGameObjectHierarchy:
                stream.GetCreateGameObjectHierarchyEvent(i, out var created);
                if (created.instanceId != justSpawned) {
                    justSpawned = created.instanceId;
                    GameObject newObject = EditorUtility.InstanceIDToObject(created.instanceId) as GameObject;
                    if (newObject.transform.position.y < 0) {
                        CreateFuture(newObject);
                        Debug.Log("New " + newObject.name + " spawned");
                    }
                }
                break;
            case ObjectChangeKind.ChangeGameObjectParent:
                stream.GetChangeGameObjectParentEvent(i, out var parentEvent);
                Object maybeParent = EditorUtility.InstanceIDToObject(parentEvent.newParentInstanceId);
                GameObject temp = EditorUtility.InstanceIDToObject(parentEvent.instanceId) as GameObject;
                Doodad maybeDoodad = temp.GetComponent<Doodad>();
                if (!maybeDoodad) continue;

                Transform futureChild = maybeDoodad.complement.transform;
                if (maybeParent) {
                    GameObject parent = maybeParent as GameObject;
                    futureChild.SetParent(parent.GetComponent<Doodad>().complement.transform);
                    Debug.Log("Parented " + futureChild.name);
                } else {
                    futureChild.transform.SetParent(null);
                    Debug.Log("Unparented " + futureChild.name);
                }
                break;
            case ObjectChangeKind.DestroyGameObjectHierarchy:
                stream.GetDestroyGameObjectHierarchyEvent(i, out var destroyed);
                Doodad[] singles = Object.FindObjectsByType<Doodad>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(doodad => !doodad.complement)
                .ToArray();
                foreach (Doodad single in singles)
                {
                    if (single.isFuture) {
                        Object.DestroyImmediate(single.gameObject);
                    } else {
                        Debug.Log("don't do that man");
                    }
                }
                break;
            }
        }
    }

    static LevelSettings Settings;

    static void InitEditor()
    {
        Debug.Log("Level Editor initialized");

        Settings = Object.FindFirstObjectByType<LevelSettings>();
        if (!Settings) {
            GameObject empty = new GameObject("Settings");
            Settings = empty.AddComponent<LevelSettings>();
        } 
        UnicornEditor.Update(Settings.isMirrored);
        if (!Settings.isMirrored) return;

        ObjectChangeEvents.changesPublished -= OnObjectChange;
        ObjectChangeEvents.changesPublished += OnObjectChange;

        GameObject futureFolder = GameObject.Find("Future");
        if (!futureFolder) futureFolder = new GameObject("Future");

        GameObject[] newPasts = Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None)
        .Where(sprite => !sprite.GetComponent<Doodad>())
        .Select(sprite => sprite.gameObject)
        .Where(gameObject => gameObject.transform.position.y < 0)
        .ToArray();

        foreach(GameObject gameObject in newPasts)
        {
            CreateFuture(gameObject);
            Debug.Log("Added new past and future: " + gameObject.name);
        }
    }

    static void CreateFuture(GameObject past)
    {
        Doodad doodad = null;
        if (past.transform.parent) doodad = past.transform.parent.GetComponent<Doodad>();

        GameObject future = Object.Instantiate(past);
        foreach (Transform child in future.transform)
        {
            Object.DestroyImmediate(child.gameObject);
        }
        future.name = "future_" + past.name;
        future.transform.SetParent(doodad ? doodad.complement.transform : GameObject.Find("Future").transform, false);

        doodad = future.GetComponent<Doodad>();
        if (!doodad) doodad = future.AddComponent<Doodad>();
        doodad.isFuture = true;
        doodad.complement = past;

        doodad =  past.GetComponent<Doodad>();
        if (!doodad) doodad = past.AddComponent<Doodad>();
        doodad.complement = future;

        future.transform.position = new Vector3(future.transform.position.x, future.transform.position.y * -1, future.transform.position.z);
        future.transform.localScale *= -1;

        Color color = new Color(0.6f, 0.7f, 1);
        future.GetComponent<SpriteRenderer>().color = color;
    }

    public static void ToggleMirror(bool isMirrored)
    {
        ToggleMirrorInternal(isMirrored);
        Settings.isMirrored = isMirrored;
    }
    static void ToggleMirrorInternal(bool isMirrored)
    {
        if (isMirrored) {
            InitEditor();
        } else {
            ObjectChangeEvents.changesPublished -= OnObjectChange;

            Doodad[] doodads = Object.FindObjectsByType<Doodad>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Doodad doodad in doodads)
            {
                if (!doodad) continue;
                if (doodad.isFuture) Object.DestroyImmediate(doodad.gameObject);
                else Object.DestroyImmediate(doodad);
            }
            Object.DestroyImmediate(GameObject.Find("Future"));
        }
    }
}

public class LevelSettings : MonoBehaviour
{
    public bool isMirrored = true;
}

public class UnicornEditor : EditorWindow
{
    static UnicornEditor Window;
    static bool isToggled = false;

    [MenuItem("Tools/Unicorn Editor")]
    public static void ShowWindow()
    {
        Window = GetWindow<UnicornEditor>("Unicorn Editor");
    }

    void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        isToggled = EditorGUILayout.Toggle("Mirror Editing", isToggled);
        if (EditorGUI.EndChangeCheck()) LevelEditor.ToggleMirror(isToggled);

        if (GUILayout.Button("Create spawnpoint"))
        {
            //TODO
        }
    }

    public static void Update(bool inToggled)
    {
        isToggled = inToggled;
        if (!HasOpenInstances<UnicornEditor>()) {
            Window = GetWindow<UnicornEditor>("Unicorn Editor");
            Debug.Log("Creating Unicorn Editor window");
        }
        else if (!Window) Window = Resources.FindObjectsOfTypeAll<UnicornEditor>()[0];
        Window.Repaint();
    }
}
