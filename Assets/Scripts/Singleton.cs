using UnityEngine;
using UnityEngine.Serialization;

public class Singleton : MonoBehaviour
{
    public static Singleton Instance { get; private set; }

    public PlayerInputManager playerInputManager;
    public FloaterManager floaterManager;
    public AudioManager audioManager;
    public PrefabReferences prefabReferences;
    public ItemManager itemManager;
    
    private void OnEnable()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        //Application.targetFrameRate = 165;
        Application.targetFrameRate = 999;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Break();
        }
    }
}
