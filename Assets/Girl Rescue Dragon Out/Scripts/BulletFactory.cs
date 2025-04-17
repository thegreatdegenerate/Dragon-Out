using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

public class BulletFactory : MonoBehaviour
{
    [Tooltip("Prefab của đạn thường")]
    [SerializeField] private GameObject normalBulletPrefab;
    [Tooltip("Bộ quản lý màu sắc")]
    [SerializeField] private ColorManager colorManager;
    [Tooltip("Số lượng đạn khởi tạo ban đầu cho mỗi màu")]
    [SerializeField, Min(0)] private int initialPoolSize = 10;
    [Tooltip("Số lượng tối đa của mỗi loại đạn trong pool")]
    [SerializeField, Min(0)] private int maxPoolSize = 30;
    
    private Dictionary<int, Queue<GameObject>> bulletPools = new Dictionary<int, Queue<GameObject>>();
    
    private void Awake()
    {
        if (colorManager == null)
        {
            colorManager = Resources.Load<ColorManager>("ColorManager");
        }
        
        InitializeBulletPools();
    }
    private void InitializeBulletPools()
    {
        if (colorManager == null || normalBulletPrefab == null) return;
        
        var colorIds = colorManager.GetAllColorIds();
        
        foreach (var colorId in colorIds)
        {
            if (!bulletPools.ContainsKey(colorId))
            {
                bulletPools[colorId] = new Queue<GameObject>();
                
                // Khởi tạo số lượng đạn ban đầu
                for (int i = 0; i < initialPoolSize; i++)
                {
                    CreateBulletForPool(colorId);
                }
            }
        }
    }
    private GameObject CreateBulletForPool(int colorId)
    {
        GameObject bulletGO = Instantiate(normalBulletPrefab);
        
        IBullet bullet = bulletGO.GetComponent<IBullet>();
        if (bullet != null)
        {
            bullet.ColorId = colorId;
            
        }
        
        bulletGO.SetActive(false);
        bulletGO.transform.SetParent(transform);
        
        return bulletGO;
    }
    public GameObject GetBullet(int colorId)
    {
        if (!bulletPools.ContainsKey(colorId))
        {
            bulletPools[colorId] = new Queue<GameObject>();
        }
        
        GameObject bulletGO = null;
        Queue<GameObject> pool = bulletPools[colorId];
        
        while (pool.Count > 0 && bulletGO == null)
        {
            bulletGO = pool.Dequeue();
            
            if (bulletGO == null)
            {
                continue;
            }
        }
        
        if (bulletGO == null)
        {
            bulletGO = CreateBulletForPool(colorId);
        }
        
        bulletGO.SetActive(true);
        bulletGO.GetComponent<NormalBullet>().SetColor(colorManager.GetColor(colorId));
        
        return bulletGO;
    }
    public void ReturnBullet(GameObject bulletGO, int colorId)
    {
        if (bulletGO == null) return;
        
        if (!bulletPools.ContainsKey(colorId))
        {
            bulletPools[colorId] = new Queue<GameObject>();
        }
        
        if (bulletPools[colorId].Count < maxPoolSize)
        {
            bulletGO.SetActive(false);
            bulletGO.transform.SetParent(transform);
            bulletPools[colorId].Enqueue(bulletGO);
        }
        else
        {
            Destroy(bulletGO);
        }
    }
    public void FireBulletAtTarget(int colorId, IHitable target, Vector3 startPosition, float damage)
    {
        if (target == null) return;
        
        GameObject bulletGO = GetBullet(colorId);
        
        IBullet bullet = bulletGO.GetComponent<IBullet>();
        if (bullet != null)
        {
            bullet.ColorId = colorId;
            bullet.Damage = damage;
            bullet.Fire(target, startPosition);
            bulletGO.GetComponent<NormalBullet>().SetColor(colorManager.GetColor(colorId));
        }
    }
    
    #region Editor Functions
    [Button("Thử Bắn Đạn"), PropertyOrder(100)]
    private void TestFireBullet(int colorId, float damage = 50f)
    {
        var targets = FindObjectsOfType<BodySegment>()
            .Where(s => s.ColorId == colorId && s.IsAlive)
            .Cast<IHitable>()
            .ToArray();
        
        if (targets.Length > 0)
        {
            var target = targets[0];
            FireBulletAtTarget(colorId, target, transform.position, damage);
            Debug.Log($"Đã bắn đạn với ID {colorId} vào mục tiêu tại vị trí {target.Position}");
        }
        else
        {
            Debug.LogWarning($"Không tìm thấy mục tiêu nào với ID {colorId}");
        }
    }
    [Button("Hiển Thị Thông Tin Pool"), PropertyOrder(101)]
    private void ShowPoolInfo()
    {
        foreach (var pair in bulletPools)
        {
            Debug.Log($"Pool mã màu {pair.Key}: {pair.Value.Count} đạn");
        }
    }
    #endregion
} 