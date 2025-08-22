using UnityEngine;
using System.Collections;

public class AttackEffect : MonoBehaviour
{
    public GameObject slashEffectPrefab;
    private Transform attackPointTransform; // Lưu trữ transform của attackPoint sau khi tìm thấy

    void Start()
    {
        // Tìm AttackPoint khi script này được khởi chạy (sau khi tướng có thể đã được tạo)
        Transform foundTransform = transform.Find("attackPoint");
        if (foundTransform != null)
        {
            attackPointTransform = foundTransform;
        }
        else
        {
            Debug.LogError("Không tìm thấy GameObject con tên 'AttackPoint'!");
            enabled = false; // Vô hiệu hóa script nếu không tìm thấy điểm tấn công
        }
    }

    public void TriggerSlash(Transform target)
    {
        if (slashEffectPrefab != null && attackPointTransform != null && target != null)
        {
            GameObject slashInstance = Instantiate(slashEffectPrefab, target.position, Quaternion.identity);

            Vector3 direction = (target.position - attackPointTransform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            slashInstance.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            StartCoroutine(FadeOut(slashInstance.GetComponent<SpriteRenderer>(), 0.3f));
        }
    }

    IEnumerator FadeOut(SpriteRenderer renderer, float duration)
    {
        Color startColor = renderer.color;
        float timer = 0f;
        while (timer < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, timer / duration);
            renderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            timer += Time.deltaTime;
            yield return null;
        }
        Destroy(renderer.gameObject);
    }
}