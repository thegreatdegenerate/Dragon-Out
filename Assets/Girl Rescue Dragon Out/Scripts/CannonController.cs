using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

public class CannonController : MonoBehaviour
{
    [SerializeField] private GameObject cannonPrefab;
    [SerializeField] private Transform[] cannonSpawnPoints;
    [SerializeField, Min(0.1f)] private float checkFireInterval = 0.2f;
    [SerializeField] private EnemyController enemyController;
    
    private List<IFireable> cannons = new List<IFireable>();
    private Dictionary<int, GameObject> cannonPrefabsByColorId = new Dictionary<int, GameObject>();
    
    private void Start()
    {
        if (enemyController == null)
        {
            enemyController = FindObjectOfType<EnemyController>();
        }
        
        InvokeRepeating(nameof(TryFireCannons), 1f, checkFireInterval);
    }
    private void OnDestroy()
    {
        CancelInvoke();
    }
    
    public void RegisterCannon(IFireable cannon)
    {
        if (cannon != null && !cannons.Contains(cannon))
        {
            cannons.Add(cannon);
            cannon.OnOutOfBullets += HandleOutOfBullets;
        }
    }
    public void UnregisterCannon(IFireable cannon)
    {
        if (cannon != null)
        {
            cannons.Remove(cannon);
            cannon.OnOutOfBullets -= HandleOutOfBullets;
        }
    }
    private void HandleOutOfBullets(IFireable cannon)
    {
        UnregisterCannon(cannon);
        
        if (cannon is MonoBehaviour monoBehaviour)
        {
            Destroy(monoBehaviour.gameObject);
        }
    }
    public void CreateCannons(Dictionary<int, int> cannonData)
    {
        foreach (var pair in cannonData)
        {
            int colorId = pair.Key;
            int bulletCount = pair.Value;
            
            CreateCannon(colorId, bulletCount);
        }
    }
    public void CreateCannon(int colorId, int bulletCount)
    {
        if (cannonSpawnPoints == null || cannonSpawnPoints.Length == 0)
        {
            Debug.LogWarning("Không có vị trí tạo pháo khả dụng");
            return;
        }
        
        Transform spawnPoint = cannonSpawnPoints[cannons.Count % cannonSpawnPoints.Length];
        GameObject cannonGO = Instantiate(cannonPrefab, spawnPoint.position, spawnPoint.rotation);
        
        Cannon cannon = cannonGO.GetComponent<Cannon>();
        if (cannon != null)
        {
            cannon.SetColorId(colorId);
            cannon.AddBullets(bulletCount);
            RegisterCannon(cannon);
        }
        else
        {
            Debug.LogError("Prefab pháo không chứa thành phần Cannon");
            Destroy(cannonGO);
        }
    }
    private void TryFireCannons()
    {
        if (enemyController == null || cannons.Count == 0) return;
        
        foreach (var cannon in cannons.ToList())
        {
            while (cannon.CanFire())
            {
                Vector3 cannonPosition = Vector3.zero;
                if (cannon is MonoBehaviour monoBehaviour)
                {
                    cannonPosition = monoBehaviour.transform.position;
                }
                
                IHitable target = enemyController.FindNearestVisibleHitableWithColorId(cannon.ColorId, cannonPosition);
                
                if (target != null)
                {
                    cannon.Fire(target);
                }
                else
                {
                    break;
                }
            }
        }
    }
    
    #region Editor Functions
    [Button("Tạo Pháo Thử"), PropertyOrder(100)]
    private void TestCreateCannon(int colorId, int bulletCount)
    {
        CreateCannon(colorId, bulletCount);
        Debug.Log($"Đã tạo pháo mới với mã màu {colorId} và {bulletCount} đạn");
    }
    [Button("Tạo Nhiều Pháo"), PropertyOrder(101)]
    private void TestCreateMultipleCannons()
    {
        Dictionary<int, int> cannonData = new Dictionary<int, int>
        {
            { 0, 10 },
            { 1, 5 },
            { 2, 8 }
        };
        
        CreateCannons(cannonData);
        Debug.Log("Đã tạo nhiều pháo với các mã màu và số đạn khác nhau");
    }
    [Button("Hiển Thị Số Lượng Pháo"), PropertyOrder(102)]
    private void ShowCannonCount()
    {
        Debug.Log($"Số lượng pháo: {cannons.Count}");
        
        var countByColor = cannons.GroupBy(c => c.ColorId)
            .Select(g => new { ColorId = g.Key, Count = g.Count() });
        
        foreach (var group in countByColor)
        {
            Debug.Log($"Mã màu {group.ColorId}: {group.Count} pháo");
        }
    }
    [Button("Thử Bắn Tất Cả Pháo"), PropertyOrder(103)]
    private void TestFireAllCannons()
    {
        TryFireCannons();
        Debug.Log("Đã thử bắn tất cả pháo");
    }
    #endregion
} 