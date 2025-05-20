using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Connects SnakePath and SnakeController components.
/// Handles integration between the path creation and the snake movement.
/// </summary>
public class SnakePathIntegrator : MonoBehaviour
{
    [SerializeField, Required] 
    private SnakePath snakePath;
    
    [SerializeField, Required] 
    private SnakeController snakeController;
    
    [Button("Kết Nối Path Với Snake", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 0.4f)]
    public void ConnectPathToSnake()
    {
        if (snakePath == null)
        {
            Debug.LogError("SnakePath component is missing!");
            return;
        }
        
        if (snakeController == null)
        {
            Debug.LogError("SnakeController component is missing!");
            return;
        }
        
        // Get the path transforms from the snake path
        Transform[] pathTransforms = snakePath.GetPathTransforms();
        
        // Set the path points in the snake controller using reflection
        var pathPointsField = snakeController.GetType().GetField("pathPoints", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        
        if (pathPointsField != null)
        {
            pathPointsField.SetValue(snakeController, pathTransforms);
            Debug.Log("Successfully connected path with snake!");
        }
        else
        {
            Debug.LogError("Could not find 'pathPoints' field in SnakeController!");
        }
    }
    
    [Button("Khởi Tạo Path Và Snake", ButtonSizes.Medium)]
    public void InitializePathAndSnake()
    {
        if (snakePath == null || snakeController == null)
        {
            Debug.LogError("Missing required components!");
            return;
        }
        
        // Generate the path if it doesn't exist
        if (snakePath.PathPoints.Count == 0)
        {
            snakePath.GeneratePath();
        }
        
        // Connect the path to the snake
        ConnectPathToSnake();
    }
    
    private void Start()
    {
        if (Application.isPlaying)
        {
            InitializePathAndSnake();
        }
    }
    
    [Button("Tìm Components", ButtonSizes.Small)]
    private void FindComponents()
    {
        if (snakePath == null)
        {
            snakePath = GetComponent<SnakePath>();
            if (snakePath == null)
            {
                snakePath = FindObjectOfType<SnakePath>();
            }
        }
        
        if (snakeController == null)
        {
            snakeController = GetComponent<SnakeController>();
            if (snakeController == null)
            {
                snakeController = FindObjectOfType<SnakeController>();
            }
        }
    }
} 