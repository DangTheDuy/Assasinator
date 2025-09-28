using System;
using UnityEngine;

public class RadarEffect : MonoBehaviour
{
    public float duration = 1.5f;        // thời gian tồn tại
    public float rotationSpeed = 240f; // độ/quay mỗi giây = 360/ duration
    private Transform cone;
    public static event Action<RadarEffect> OnRadarFinished;

    void Start()
    {
        cone = transform.Find("RadarCone");
        Invoke(nameof(Finish), duration);
    }

    void Update()
    {
        if (cone != null)
        {
            cone.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }
    }
    
    private void Finish()
    {
        OnRadarFinished?.Invoke(this); 
        Destroy(gameObject);
    }
}
