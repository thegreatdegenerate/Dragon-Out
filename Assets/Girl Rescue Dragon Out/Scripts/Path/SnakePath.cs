using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

public class SnakePath : MonoBehaviour
{
    [Header("Path Settings")]
    [Tooltip("Controls the overall shape of the snake path")]
    [SerializeField] private PathType pathType = PathType.Sinusoidal;
    
    [Tooltip("Number of points in the path")]
    [SerializeField, Range(100, 200)] private int pointCount = 100;
    
    [Tooltip("Size of the path")]
    [SerializeField] private Vector2 pathSize = new Vector2(10f, 5f);
    
    [Tooltip("Number of oscillations/curves in the path")]
    [SerializeField, Range(1, 20)] private int oscillationCount = 5;
    
    [Tooltip("Path randomization factor")]
    [SerializeField, Range(0f, 1f)] private float randomizationFactor = 0.2f;
    
    [Tooltip("Path center point")]
    [SerializeField] private Vector2 pathCenter = Vector2.zero;
    
    [Header("Path Connection")]
    [Tooltip("Snake controller to connect with")]
    [SerializeField] private SnakeController targetSnakeController;
    
    [Tooltip("External transform to create path points under")]
    [SerializeField] private Transform externalPathParent;
    
    [Header("Gizmos Visualization")]
    [SerializeField] private Color pathColor = Color.green;
    [SerializeField] private float pointSize = 0.1f;
    [SerializeField] private bool showPoints = true;
    [SerializeField] private bool showLines = true;
    
    // Path points
    [ListDrawerSettings(ShowIndexLabels = true, ShowItemCount = true)]
    [PropertyOrder(100)]
    [TabGroup("Path Data")]
    public List<Vector2> PathPoints { get; private set; } = new List<Vector2>();
    
    // Path type options
    public enum PathType
    {
        Sinusoidal,
        Spiral,
        ZigZag,
        Random,
        Manual
    }
    
    // Transforms for path points that can be used by other components
    [HideInInspector] public List<Transform> PathTransforms = new List<Transform>();
    
    // Parent object for created transforms
    private Transform pathPointsParent;
    
    // Initialize on awake
    private void Awake()
    {
        if (PathPoints.Count == 0 && Application.isPlaying)
        {
            GeneratePath();
        }
        
        CreateTransforms();
        
        // Connect to target snake controller if assigned
        if (targetSnakeController != null)
        {
            SendPathToSnakeController();
        }
    }
    
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        
        if (pathType != PathType.Manual)
        {
            GeneratePath();
        }
    }
    
    [Button("Tạo Đường Đi", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1.0f)]
    [PropertyOrder(50)]
    public void GeneratePath()
    {
        PathPoints.Clear();
        
        switch (pathType)
        {
            case PathType.Sinusoidal:
                GenerateSinusoidalPath();
                break;
            case PathType.Spiral:
                GenerateSpiralPath();
                break;
            case PathType.ZigZag:
                GenerateZigZagPath();
                break;
            case PathType.Random:
                GenerateRandomPath();
                break;
            case PathType.Manual:
                // Don't generate in manual mode
                break;
        }
        
        if (Application.isPlaying)
        {
            CreateTransforms();
            
            // Update snake controller if assigned
            if (targetSnakeController != null)
            {
                SendPathToSnakeController();
            }
        }
    }
    
    private void GenerateSinusoidalPath()
    {
        for (int i = 0; i < pointCount; i++)
        {
            float progress = i / (float)(pointCount - 1);
            float xPos = Mathf.Lerp(-pathSize.x/2, pathSize.x/2, progress);
            
            // Create a sinus wave with oscillations
            float yPos = Mathf.Sin(progress * oscillationCount * Mathf.PI * 2) * pathSize.y/2;
            
            // Add randomization
            if (randomizationFactor > 0)
            {
                yPos += Random.Range(-pathSize.y * randomizationFactor, pathSize.y * randomizationFactor);
            }
            
            Vector2 point = new Vector2(xPos, yPos) + pathCenter;
            PathPoints.Add(point);
        }
    }
    
    private void GenerateSpiralPath()
    {
        float angleStep = 360f * oscillationCount / pointCount;
        float radiusStep = pathSize.x / pointCount;
        
        for (int i = 0; i < pointCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float radius = i * radiusStep;
            
            float xPos = Mathf.Cos(angle) * radius;
            float yPos = Mathf.Sin(angle) * radius;
            
            // Add randomization
            if (randomizationFactor > 0)
            {
                float randomOffset = radius * randomizationFactor;
                xPos += Random.Range(-randomOffset, randomOffset);
                yPos += Random.Range(-randomOffset, randomOffset);
            }
            
            Vector2 point = new Vector2(xPos, yPos) + pathCenter;
            PathPoints.Add(point);
        }
    }
    
    private void GenerateZigZagPath()
    {
        float segmentLength = pathSize.x / (oscillationCount * 2);
        float amplitude = pathSize.y / 2;
        
        for (int i = 0; i < pointCount; i++)
        {
            float progress = i / (float)(pointCount - 1);
            float x = Mathf.Lerp(-pathSize.x/2, pathSize.x/2, progress);
            
            // Calculate zig-zag pattern
            float normalizedX = (x + pathSize.x/2) / pathSize.x * oscillationCount * 2;
            float zigzagValue = (normalizedX % 2) < 1 ? normalizedX % 1 : 1 - (normalizedX % 1);
            float y = (zigzagValue * 2 - 1) * amplitude;
            
            // Add randomization
            if (randomizationFactor > 0)
            {
                y += Random.Range(-amplitude * randomizationFactor, amplitude * randomizationFactor);
            }
            
            Vector2 point = new Vector2(x, y) + pathCenter;
            PathPoints.Add(point);
        }
    }
    
    private void GenerateRandomPath()
    {
        // Start with a straight line
        for (int i = 0; i < pointCount; i++)
        {
            float progress = i / (float)(pointCount - 1);
            float xPos = Mathf.Lerp(-pathSize.x/2, pathSize.x/2, progress);
            float yPos = 0;
            
            Vector2 point = new Vector2(xPos, yPos) + pathCenter;
            PathPoints.Add(point);
        }
        
        // Apply random deformations
        for (int iteration = 0; iteration < 3; iteration++)
        {
            List<Vector2> newPoints = new List<Vector2>(PathPoints);
            
            for (int i = 1; i < PathPoints.Count - 1; i++)
            {
                float deformY = Random.Range(-pathSize.y/4, pathSize.y/4) * randomizationFactor;
                newPoints[i] = new Vector2(PathPoints[i].x, PathPoints[i].y + deformY);
            }
            
            // Smooth the path
            for (int i = 1; i < PathPoints.Count - 1; i++)
            {
                Vector2 prev = newPoints[i - 1];
                Vector2 curr = newPoints[i];
                Vector2 next = newPoints[i + 1];
                
                PathPoints[i] = Vector2.Lerp(curr, (prev + next) / 2, 0.3f);
            }
        }
    }
    
    [Button("Xóa Đường Đi", ButtonSizes.Medium), GUIColor(1.0f, 0.4f, 0.4f)]
    [PropertyOrder(51)]
    public void ClearPath()
    {
        PathPoints.Clear();
        DestroyTransforms();
    }
    
    [Button("Thêm Điểm", ButtonSizes.Small)]
    [TabGroup("Path Data")]
    public void AddPoint()
    {
        Vector2 newPoint = Vector2.zero;
        
        if (PathPoints.Count > 0)
        {
            newPoint = PathPoints[PathPoints.Count - 1] + Vector2.right;
        }
        
        PathPoints.Add(newPoint);
    }
    
    // Create Transform objects for each path point (used by other components)
    private void CreateTransforms()
    {
        DestroyTransforms();
        
        if (PathPoints.Count == 0) return;
        
        // Create parent object for points
        if (pathPointsParent == null)
        {
            // Use external parent if specified, otherwise create a new one
            if (externalPathParent != null)
            {
                pathPointsParent = externalPathParent;
            }
            else
            {
                GameObject parent = new GameObject("Path Points");
                parent.transform.SetParent(transform);
                parent.transform.localPosition = Vector3.zero;
                pathPointsParent = parent.transform;
            }
        }
        
        PathTransforms.Clear();
        
        for (int i = 0; i < PathPoints.Count; i++)
        {
            GameObject point = new GameObject($"Point_{i}");
            point.transform.SetParent(pathPointsParent);
            point.transform.position = new Vector3(PathPoints[i].x, PathPoints[i].y, 0);
            
            PathTransforms.Add(point.transform);
        }
    }
    
    private void DestroyTransforms()
    {
        PathTransforms.Clear();
        
        if (pathPointsParent != null)
        {
            while (pathPointsParent.childCount > 0)
            {
                DestroyImmediate(pathPointsParent.GetChild(0).gameObject);
            }
        }
    }
    
    [Button("Tạo Lại Transform", ButtonSizes.Medium)]
    [PropertyOrder(52)]
    public void RebuildTransforms()
    {
        if (Application.isPlaying)
        {
            CreateTransforms();
        }
    }
    
    // Display path in the Scene view
    private void OnDrawGizmos()
    {
        if (PathPoints == null || PathPoints.Count == 0) return;
        
        Gizmos.color = pathColor;
        
        // Draw lines between points
        if (showLines)
        {
            for (int i = 0; i < PathPoints.Count - 1; i++)
            {
                Vector3 start = new Vector3(PathPoints[i].x, PathPoints[i].y, 0);
                Vector3 end = new Vector3(PathPoints[i + 1].x, PathPoints[i + 1].y, 0);
                Gizmos.DrawLine(start, end);
            }
        }
        
        // Draw points
        if (showPoints)
        {
            for (int i = 0; i < PathPoints.Count; i++)
            {
                Vector3 position = new Vector3(PathPoints[i].x, PathPoints[i].y, 0);
                Gizmos.DrawSphere(position, pointSize);
            }
        }
    }
    
    // Get transforms for use with SnakeController
    public Transform[] GetPathTransforms()
    {
        if (PathTransforms.Count == 0)
        {
            CreateTransforms();
        }
        
        return PathTransforms.ToArray();
    }
    
    // Helper method to smooth a list of points using Catmull-Rom spline
    [Button("Làm Mịn Đường Đi", ButtonSizes.Medium)]
    [TabGroup("Path Data")]
    public void SmoothPath()
    {
        if (PathPoints.Count < 4) return;
        
        List<Vector2> smoothedPoints = new List<Vector2>();
        
        // Double first and last points to maintain endpoints
        Vector2 firstPoint = PathPoints[0];
        Vector2 lastPoint = PathPoints[PathPoints.Count - 1];
        
        List<Vector2> tempPoints = new List<Vector2> { firstPoint };
        tempPoints.AddRange(PathPoints);
        tempPoints.Add(lastPoint);
        
        int subdivisions = 3; // Number of points to add between each original point
        
        for (int i = 0; i < tempPoints.Count - 3; i++)
        {
            Vector2 p0 = tempPoints[i];
            Vector2 p1 = tempPoints[i + 1];
            Vector2 p2 = tempPoints[i + 2];
            Vector2 p3 = tempPoints[i + 3];
            
            if (i == 0) smoothedPoints.Add(p1);
            
            for (int j = 1; j <= subdivisions; j++)
            {
                float t = j / (float)(subdivisions + 1);
                Vector2 newPoint = CatmullRomPoint(p0, p1, p2, p3, t);
                smoothedPoints.Add(newPoint);
            }
            
            smoothedPoints.Add(p2);
        }
        
        PathPoints = smoothedPoints;
        
        if (Application.isPlaying)
        {
            CreateTransforms();
        }
    }
    
    private Vector2 CatmullRomPoint(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        
        float b0 = 0.5f * (-t3 + 2f * t2 - t);
        float b1 = 0.5f * (3f * t3 - 5f * t2 + 2f);
        float b2 = 0.5f * (-3f * t3 + 4f * t2 + t);
        float b3 = 0.5f * (t3 - t2);
        
        return b0 * p0 + b1 * p1 + b2 * p2 + b3 * p3;
    }
    
    [Button("Gửi Path Tới Snake Controller", ButtonSizes.Medium)]
    [PropertyOrder(53)]
    public void SendPathToSnakeController()
    {
        if (targetSnakeController == null)
        {
            Debug.LogWarning("No target Snake Controller assigned!");
            return;
        }
        
        // Send the path data directly to the snake controller
        targetSnakeController.SetPathData(PathPoints.ToArray());
    }
    
    [Button("Tạo Path Tại Transform Khác", ButtonSizes.Medium)]
    [PropertyOrder(54)]
    public void CreatePathAtTransform(Transform targetTransform)
    {
        if (targetTransform == null) return;
        
        // Store the current path parent
        Transform originalParent = pathPointsParent;
        
        // Set the external parent temporarily
        externalPathParent = targetTransform;
        
        // Recreate the transforms at the new parent
        CreateTransforms();
        
        // Reset the external parent
        externalPathParent = null;
        
        // Restore original parent
        pathPointsParent = originalParent;
    }
    
    // Get path data as Vector2 array
    public Vector2[] GetPathData()
    {
        return PathPoints.ToArray();
    }
} 