using UnityEngine;
using Sirenix.OdinInspector;

public class BodySegment : MonoBehaviour, IHitable
{
    [SerializeField] 
    private int colorId;
    [SerializeField, ProgressBar(0, "maxHealth", ColorGetter = "GetHealthBarColor")]
    private float health = 100f;
    [SerializeField] 
    private float maxHealth = 100f;
    [SerializeField]
    private GameObject destroyEffectPrefab;
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    
    private bool hasTarget = false;
    
    public System.Action<BodySegment> OnDestroyed;

    #region IHitable Implementation
    public int ColorId => colorId;
    public Vector3 Position => transform.position;
    public bool IsAlive => health > 0;
    public bool HasTarget => hasTarget;
    
    public void SetAsTargeted()
    {
        hasTarget = true;
    }
    public void ClearTargeted()
    {
        hasTarget = false;
    }
    public void TakeDamage(float damage)
    {
        if (!IsAlive) return;
        
        health = Mathf.Max(0, health - damage);
        
        if (!IsAlive)
        {
            DestroySegment();
        }
    }
    public void SetColor(Color color)
    {
        spriteRenderer ??= GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }
    #endregion
    private void DestroySegment()
    {
        OnDestroyed?.Invoke(this);
        
        if (destroyEffectPrefab != null)
        {
            Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
        }
        
        Destroy(gameObject);
    }
    
    #region Editor Functions
    private Color GetHealthBarColor()
    {
        return Color.Lerp(Color.red, Color.green, health / maxHealth);
    }
    [Button("Nhận Sát Thương Thử"), PropertyOrder(100)]
    private void TestTakeDamage()
    {
        TakeDamage(20f);
    }
    [Button("Phá Hủy Ngay"), PropertyOrder(100)]
    private void TestDestroy()
    {
        TakeDamage(health);
    }
    #endregion
} 