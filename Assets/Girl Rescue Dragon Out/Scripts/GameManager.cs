using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Quản lý trò chơi tổng thể
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Tooltip("Controller quản lý kẻ địch")]
    [SerializeField] private EnemyController enemyController;
    [Tooltip("Controller quản lý pháo")]
    [SerializeField] private CannonController cannonController;
    [Tooltip("Factory tạo đạn")]
    [SerializeField] private BulletFactory bulletFactory;
    [Tooltip("Quản lý màu sắc")]
    [SerializeField] private ColorManager colorManager;
    
    private bool isGameRunning = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        FindRequiredComponents();
    }
    private void Start()
    {
        InitializeGame();
    }
    
    private void FindRequiredComponents()
    {
        if (enemyController == null)
        {
            enemyController = FindObjectOfType<EnemyController>();
        }
        
        if (cannonController == null)
        {
            cannonController = FindObjectOfType<CannonController>();
        }
        
        if (bulletFactory == null)
        {
            bulletFactory = FindObjectOfType<BulletFactory>();
        }
        
        if (colorManager == null)
        {
            colorManager = Resources.Load<ColorManager>("ColorManager");
        }
    }
    private void InitializeGame()
    {
        isGameRunning = true;
    }
    
    public void StartGame()
    {
        if (isGameRunning) return;
        
        InitializeGame();
        isGameRunning = true;
    }
    public void PauseGame()
    {
        if (!isGameRunning) return;
        
        isGameRunning = false;
        Time.timeScale = 0f;
    }
    public void ResumeGame()
    {
        if (isGameRunning) return;
        
        isGameRunning = true;
        Time.timeScale = 1f;
    }
    public void EndGame()
    {
        isGameRunning = false;
        Time.timeScale = 1f;
    }
    public void CreateCannon(int colorId, int bulletCount)
    {
        if (cannonController == null) return;
        cannonController.CreateCannon(colorId, bulletCount);
    }
    public void CreateCannons(Dictionary<int, int> cannonData)
    {
        if (cannonController == null) return;
        cannonController.CreateCannons(cannonData);
    }
    
    #region Editor Functions
    [Button("Bắt Đầu Trò Chơi"), PropertyOrder(100)]
    private void TestStartGame()
    {
        StartGame();
    }
    [Button("Tạm Dừng/Tiếp Tục"), PropertyOrder(101)]
    private void TestTogglePause()
    {
        if (isGameRunning)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }
    [Button("Kết Thúc Trò Chơi"), PropertyOrder(102)]
    private void TestEndGame()
    {
        EndGame();
    }
    [Button("Tạo Pháo Thử"), PropertyOrder(103)]
    private void TestCreateCannon(int colorId, int bulletCount)
    {
        CreateCannon(colorId, bulletCount);
    }
    [Button("Tạo Nhiều Pháo Thử"), PropertyOrder(104)]
    private void TestCreateMultipleCannons()
    {
        Dictionary<int, int> cannonData = new Dictionary<int, int>
        {
            { 0, 10 }, // Pháo đỏ với 10 đạn
            { 1, 5 },  // Pháo xanh lá với 5 đạn
            { 2, 8 }   // Pháo xanh dương với 8 đạn
        };
        
        CreateCannons(cannonData);
    }
    #endregion
} 