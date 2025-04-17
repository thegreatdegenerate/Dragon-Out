using System;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;

public class Cannon : MonoBehaviour, IFireable
{
    [SerializeField] private int colorId;
    [SerializeField, Min(0)] private int bulletCount = 5;
    [SerializeField, Min(0.1f)] private float fireRate = 1f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject fireEffectPrefab;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private TMP_Text textBulletCount;
    
    private float nextFireTime = 0f;
    private IHitable currentTarget;
   
    #region IFireable Implementation
    public int ColorId => colorId;
    public int RemainingBullets => bulletCount;
    public Vector3 FirePosition => firePoint != null ? firePoint.position : transform.position;
    public float FireRate => fireRate;
    public float NextFireTime 
    { 
        get => nextFireTime; 
        set => nextFireTime = value; 
    }
    
    public event Action<IFireable> OnOutOfBullets;
    public bool CanFire()
    {
        return bulletCount > 0 && Time.time >= nextFireTime;
    }
    public void Fire(IHitable target)
    {
        if (!CanFire() || target == null || bulletPrefab == null) return;
        
        currentTarget = target;
        RotateTowardsTarget(target.Position);
        
        if (!IsAimingAtTarget(target.Position))
        {
            return;
        }
        
        nextFireTime = Time.time + fireRate;
        bulletCount--;
        
        if (fireEffectPrefab != null)
        {
            Instantiate(fireEffectPrefab, FirePosition, Quaternion.identity);
        }
        
        GameObject bulletGO = Instantiate(bulletPrefab, FirePosition, transform.rotation);
        
        IBullet bullet = bulletGO.GetComponent<IBullet>();
        if (bullet != null)
        {
            bullet.ColorId = colorId;
            bullet.Fire(target, FirePosition);
        }
        
        if (bulletCount <= 0)
        {
            OnOutOfBullets?.Invoke(this);
        }
        
        UpdateTextBullet();
    }
    #endregion
    
    private void Update()
    {
        if (currentTarget != null && currentTarget.IsAlive)
        {
            RotateTowardsTarget(currentTarget.Position);
        }
    }
    
    private void RotateTowardsTarget(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        
        float currentAngle = transform.eulerAngles.z;
        if (currentAngle > 180) currentAngle -= 360;
        
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
        
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, 
            targetRotation, 
            rotationSpeed * Time.deltaTime
        );
    }
    private bool IsAimingAtTarget(Vector3 targetPosition, float angleTolerance = 5f)
    {
        Vector3 direction = targetPosition - transform.position;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        float currentAngle = transform.eulerAngles.z;
        
        if (currentAngle > 180) currentAngle -= 360;
        if (targetAngle > 180) targetAngle -= 360;
        
        float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);
        
        return Mathf.Abs(angleDifference) <= angleTolerance;
    }
    public void AddBullets(int amount)
    {
        bulletCount += Mathf.Max(0, amount);
        UpdateTextBullet();
    }
    public void SetColorId(int id)
    {
        colorId = id;
        UpdateCannonVisuals();
    }
    private void UpdateCannonVisuals()
    {
        Color color = GetColorFromId(colorId);
        
        var renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var renderer in renderers)
        {
            renderer.color = color;
        }
    }
    private Color GetColorFromId(int id)
    {
        switch (id)
        {
            case 0: return Color.red;
            case 1: return Color.green;
            case 2: return Color.blue;
            case 3: return Color.yellow;
            case 4: return Color.cyan;
            case 5: return Color.magenta;
            default: return Color.white;
        }
    }
    private void UpdateTextBullet()
    {
        if (textBulletCount != null)
        {
            textBulletCount.text = bulletCount.ToString();
        }
    }
    
    #region Editor Functions
    [Button("Bắn Vào Mục Tiêu"), PropertyOrder(100)]
    private void TestFire()
    {
        var targets = FindObjectsOfType<BodySegment>()
            .Where(s => s.ColorId == colorId && s.IsAlive)
            .Cast<IHitable>()
            .ToArray();
        
        if (targets.Length > 0)
        {
            var target = targets[0];
            Fire(target);
            Debug.Log($"Đã bắn vào mục tiêu với ID {colorId}, đạn còn lại: {bulletCount}");
        }
        else
        {
            Debug.LogWarning($"Không tìm thấy mục tiêu nào với ID {colorId}");
        }
    }
    [Button("Thêm Đạn"), PropertyOrder(101)]
    private void TestAddBullets(int amount = 5)
    {
        AddBullets(amount);
        Debug.Log($"Đã thêm {amount} đạn, tổng số đạn: {bulletCount}");
    }
    [Button("Đổi Mã Màu"), PropertyOrder(102)]
    private void TestChangeColor(int newColorId)
    {
        SetColorId(newColorId);
        Debug.Log($"Đã đổi mã màu thành {newColorId}");
    }
    #endregion
} 