using UnityEngine;

public class GameObjectDeactivator : MonoBehaviour
{
    public GameObject objectToDeactivate;

    public void DeactivateGameobject()
    {
        objectToDeactivate.SetActive(false);
    }
}
