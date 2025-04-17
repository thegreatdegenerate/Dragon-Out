using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

public class EnemyController : MonoBehaviour
{
    [Tooltip("Biên để kiểm tra đối tượng có nằm trong màn hình không (0-1)")]
    [SerializeField, Range(0f, 0.2f)] 
    private float screenBoundary = 0.1f;
    [SerializeField] 
    private ColorManager colorManager;
    
    private List<IHitable> hitableObjects = new List<IHitable>();
    private Camera mainCamera;
    private void Awake()
    {
        mainCamera = Camera.main;
    }
    private void OnEnable()
    {
        var bodySegments = FindObjectsOfType<BodySegment>();
        foreach (var segment in bodySegments)
        {
            RegisterHitableObject(segment);
            segment.SetColor(colorManager.GetColor(segment.ColorId));
            segment.OnDestroyed += OnSegmentDestroyed;
        }
    }
    private void OnDisable()
    {
        var bodySegments = FindObjectsOfType<BodySegment>();
        foreach (var segment in bodySegments)
        {
            segment.OnDestroyed -= OnSegmentDestroyed;
        }
    }
    
    public void RegisterHitableObject(IHitable hitable)
    {
        if (!hitableObjects.Contains(hitable))
        {
            hitableObjects.Add(hitable);
        }
    }
    public void UnregisterHitableObject(IHitable hitable)
    {
        hitableObjects.Remove(hitable);
    }
    private void OnSegmentDestroyed(BodySegment segment)
    {
        UnregisterHitableObject(segment);
        segment.OnDestroyed -= OnSegmentDestroyed;
    }
    public IHitable FindVisibleHitableWithColorId(int colorId)
    {
        return hitableObjects
            .Where(h => h.IsAlive && h.ColorId == colorId && IsVisibleOnScreen(h.Position) && !h.HasTarget)
            .OrderBy(h => Vector3.Distance(h.Position, Vector3.zero)) // Ưu tiên đối tượng gần nhất
            .FirstOrDefault();
    }
    public IHitable FindNearestVisibleHitableWithColorId(int colorId, Vector3 cannonPosition)
    {
        return hitableObjects
            .Where(h => h.IsAlive && h.ColorId == colorId && IsVisibleOnScreen(h.Position) && !h.HasTarget)
            .OrderBy(h => Vector3.Distance(h.Position, cannonPosition)) // Ưu tiên đối tượng gần nhất với pháo
            .FirstOrDefault();
    }
    private bool IsVisibleOnScreen(Vector3 worldPosition)
    {
        if (mainCamera == null) return false;
        
        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(worldPosition);
        
        return viewportPoint.z > 0 && 
               viewportPoint.x >= screenBoundary && 
               viewportPoint.x <= 1 - screenBoundary && 
               viewportPoint.y >= screenBoundary && 
               viewportPoint.y <= 1 - screenBoundary;
    }
    
    #region Editor Functions
    [Button("Tìm Đối Tượng Có Thể Bắn Theo ID"), PropertyOrder(100)]
    private void TestFindHitable(int colorId)
    {
        var hitable = FindVisibleHitableWithColorId(colorId);
        if (hitable != null)
        {
            Debug.Log($"Tìm thấy đối tượng với ID {colorId} tại vị trí {hitable.Position}");
        }
        else
        {
            Debug.Log($"Không tìm thấy đối tượng nào với ID {colorId} trong tầm nhìn");
        }
    }
    [Button("Tìm Đối Tượng Gần Nhất Với Vị Trí"), PropertyOrder(100)]
    private void TestFindNearestHitable(int colorId, Vector3 position)
    {
        var hitable = FindNearestVisibleHitableWithColorId(colorId, position);
        if (hitable != null)
        {
            Debug.Log($"Tìm thấy đối tượng gần nhất với ID {colorId} tại vị trí {hitable.Position}, " + 
                       $"cách {Vector3.Distance(position, hitable.Position):F2} đơn vị");
        }
        else
        {
            Debug.Log($"Không tìm thấy đối tượng nào với ID {colorId} trong tầm nhìn");
        }
    }
    [Button("Hiển Thị Số Lượng Đối Tượng"), PropertyOrder(100)]
    private void ShowHitableCount()
    {
        Debug.Log($"Số lượng đối tượng có thể bắn: {hitableObjects.Count}");
        
        // Đếm số lượng theo mã màu
        var countByColor = hitableObjects.GroupBy(h => h.ColorId)
            .Select(g => new { ColorId = g.Key, Count = g.Count() });
        
        foreach (var group in countByColor)
        {
            Debug.Log($"Mã màu {group.ColorId}: {group.Count} đối tượng");
        }
    }
    #endregion
} 