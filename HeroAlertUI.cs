using UnityEngine;

public class HeroAlertUI : MonoBehaviour
{
    public static HeroAlertUI Instance;

    [Header("Backgrounds")]
    [SerializeField] private GameObject bgGreen;
    [SerializeField] private GameObject bgRed;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetDetected(false); 
    }

    public void SetDetected(bool detected)
    {
        if (bgGreen == null || bgRed == null) return;

        bgGreen.SetActive(!detected); 
        bgRed.SetActive(detected);    
    }
}
