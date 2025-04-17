using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "ColorManager", menuName = "Game/Color Manager")]
public class ColorManager : ScriptableObject
{
    [System.Serializable]
    public class ColorData
    {
        [Tooltip("Mã màu")]
        public int colorId;
        
        [Tooltip("Tên màu")]
        public string colorName;
        
        [Tooltip("Màu sắc")]
        public Color color;
    }
    
    [Tooltip("Danh sách màu sắc")]
    [SerializeField] private List<ColorData> colorData = new List<ColorData>();
    
    private Dictionary<int, Color> colorMap;
    private Dictionary<int, string> colorNameMap;
    
    private void OnEnable()
    {
        InitializeColorMaps();
    }
    
    private void InitializeColorMaps()
    {
        colorMap = new Dictionary<int, Color>();
        colorNameMap = new Dictionary<int, string>();
        
        foreach (var data in colorData)
        {
            colorMap[data.colorId] = data.color;
            colorNameMap[data.colorId] = data.colorName;
        }
    }
    public Color GetColor(int colorId)
    {
        if (colorMap == null)
        {
            InitializeColorMaps();
        }
        
        if (colorMap.TryGetValue(colorId, out Color color))
        {
            return color;
        }
        
        return Color.white;
    }
    public List<int> GetAllColorIds()
    {
        if (colorMap == null)
        {
            InitializeColorMaps();
        }
        
        return new List<int>(colorMap.Keys);
    }
    
    #region Editor Functions
    [Button("Tạo Màu Cơ Bản"), PropertyOrder(100)]
    private void CreateBasicColors()
    {
        colorData.Clear();
        
        colorData.Add(new ColorData { colorId = 0, colorName = "Đỏ", color = Color.red });
        colorData.Add(new ColorData { colorId = 1, colorName = "Xanh Lá", color = Color.green });
        colorData.Add(new ColorData { colorId = 2, colorName = "Xanh Dương", color = Color.blue });
        colorData.Add(new ColorData { colorId = 3, colorName = "Vàng", color = Color.yellow });
        colorData.Add(new ColorData { colorId = 4, colorName = "Hồng", color = Color.magenta });
        colorData.Add(new ColorData { colorId = 5, colorName = "Lam", color = Color.cyan });
        
        InitializeColorMaps();
    }
    #endregion
} 