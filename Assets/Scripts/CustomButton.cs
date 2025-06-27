using UnityEngine;
using UnityEngine.Events;

public class CustomButton : MonoBehaviour
{
    public UnityEvent eventOnClick;
    
    private void OnMouseDown()
    {
        eventOnClick?.Invoke();
    }
}
