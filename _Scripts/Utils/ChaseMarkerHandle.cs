using UnityEngine;
using UnityEngine.UI;

public class ChaseMarkerHandler : MonoBehaviour
{
    public GameObject marker; 
    public Color markerColor = Color.red;

    void Start()
    {
        if(marker != null)
            marker.SetActive(false);
    }

    public void ShowMarker()
    {
        if(marker != null)
        {
            marker.SetActive(true);
            var sr = marker.GetComponent<Image>();
            if(sr != null)
                sr.color = markerColor;
        }
    }

    public void HideMarker()
    {
        if(marker != null)
            marker.SetActive(false);
    }
}
