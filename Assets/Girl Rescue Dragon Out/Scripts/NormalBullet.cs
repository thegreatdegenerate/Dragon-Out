using System.Collections;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

public class NormalBullet : MonoBehaviour, IBullet
{
    [SerializeField] private int colorId;
    [SerializeField] private float damage = 50f;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float hitDistance = 0.5f;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float selfDestructTime = 5f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    private IHitable target;
    private bool isTracking = false;
    
    #region IBullet Implementation
    public int ColorId 
    { 
        get => colorId; 
        set => colorId = value; 
    }
    public float Damage 
    { 
        get => damage; 
        set => damage = value; 
    }
    public float Speed 
    { 
        get => speed; 
        set => speed = value; 
    }
    public float HitDistance 
    { 
        get => hitDistance; 
        set => hitDistance = value; 
    }
    
    public void Fire(IHitable target, Vector3 startPosition)
    {
        this.target = target;
        transform.position = startPosition;
        isTracking = true;
        
        target.SetAsTargeted();
        
        StartCoroutine(TrackTargetCoroutine());
        
        Invoke(nameof(DestroyBullet), selfDestructTime);
    }
    public void DestroyBullet()
    {
        StopAllCoroutines();
        CancelInvoke();
        if (target != null && target.IsAlive)
        {
            target.ClearTargeted();
        }
        
        Destroy(gameObject);
    }
    public void SetColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }
    #endregion
    
    private IEnumerator TrackTargetCoroutine()
    {
        while (isTracking && target != null && target.IsAlive)
        {
            Vector3 direction = (target.Position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
            
            if (direction != Vector3.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
            
            float distanceToTarget = Vector3.Distance(transform.position, target.Position);
            if (distanceToTarget <= hitDistance)
            {
                target.TakeDamage(damage);
                
                if (hitEffectPrefab != null)
                {
                    Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                }
                
                DestroyBullet();
                yield break;
            }
            
            yield return null;
        }
        
        DestroyBullet();
    }
    
    #region Editor Functions
    [Button("Bắn Thử"), PropertyOrder(100)]
    private void TestFire()
    {
        var targets = FindObjectsOfType<BodySegment>()
            .Where(s => s.ColorId == colorId && s.IsAlive)
            .Cast<IHitable>()
            .ToArray();
        
        if (targets.Length > 0)
        {
            var target = targets[0];
            Fire(target, transform.position);
            Debug.Log($"Đã bắn vào mục tiêu với ID {colorId} tại vị trí {target.Position}");
        }
        else
        {
            Debug.LogWarning($"Không tìm thấy mục tiêu nào với ID {colorId}");
        }
    }
    #endregion
} 