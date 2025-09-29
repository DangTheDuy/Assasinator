using System;
using UnityEngine;

public class RadarEffect : MonoBehaviour
{
    public float duration = 1.5f;     
    public float rotationSpeed = 240f;  
    private Transform cone;
    public Action onFinished;

    void Start()
    {
        cone = transform.Find("RadarCone");
        Invoke(nameof(Finish), duration);
    }

    void Update()
    {
        if (cone != null)
            cone.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }

    private void Finish()
    {
        onFinished?.Invoke();  
        Destroy(gameObject);  
    }
}
