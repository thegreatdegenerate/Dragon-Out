using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class SnakeController : MonoBehaviour
{
    [Header("Snake Components")]
    [SerializeField] private Transform snakeHead;
    [SerializeField] private BodySegment bodySegmentPrefab;
    [SerializeField] private int initialSegmentCount = 5;
    [SerializeField] private float segmentSpacing = 0.5f;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Transform[] pathPoints;
    [SerializeField] private bool loopPath = true;
    
    [Header("Path Data")]
    [SerializeField, ReadOnly] private Vector2[] pathData;
    [SerializeField] private bool usePathData = false;
    
    [Header("Visual Settings")]
    [SerializeField] private Color[] segmentColors;
    
    private List<BodySegment> bodySegments = new List<BodySegment>();
    private float distanceTraveled = 0f;
    private int currentPathIndex = 0;
    private float waypointDistanceThreshold = 0.1f;
    private List<Vector2> pathPositions = new List<Vector2>();
    private float totalPathLength = 0f;
    private List<float> pathSegmentLengths = new List<float>();
    
    // Singleton pattern for easy access
    public static SnakeController Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    private void Start()
    {
        // Calculate path positions and lengths
        CalculatePathData();
        
        // Initialize the snake
        InitializeSnake();
        
        // Set the initial position of the snake head to the first path point
        if (pathPositions.Count > 0)
        {
            snakeHead.position = pathPositions[0];
        }
    }
    
    private void Update()
    {
        // Move the snake head along the path
        MoveSnakeHead();
        
        // Update body segments position
        UpdateBodySegments();
    }
    
    private void CalculatePathData()
    {
        pathPositions.Clear();
        pathSegmentLengths.Clear();
        totalPathLength = 0f;
        
        // Use path data if available and enabled, otherwise use transform points
        if (usePathData && pathData != null && pathData.Length >= 2)
        {
            // Use direct Vector2 path data
            for (int i = 0; i < pathData.Length; i++)
            {
                pathPositions.Add(pathData[i]);
            }
        }
        else if (pathPoints != null && pathPoints.Length >= 2)
        {
            // Use transform path points
            for (int i = 0; i < pathPoints.Length; i++)
            {
                pathPositions.Add(pathPoints[i].position);
            }
        }
        else
        {
            // No valid path data
            return;
        }
        
        // Calculate the length of each path segment
        for (int i = 0; i < pathPositions.Count - 1; i++)
        {
            float segmentLength = Vector2.Distance(pathPositions[i], pathPositions[i + 1]);
            pathSegmentLengths.Add(segmentLength);
            totalPathLength += segmentLength;
        }
        
        // If path is looped, add the distance from last to first point
        if (loopPath && pathPositions.Count > 0)
        {
            float segmentLength = Vector2.Distance(pathPositions[pathPositions.Count - 1], pathPositions[0]);
            pathSegmentLengths.Add(segmentLength);
            totalPathLength += segmentLength;
        }
    }
    
    // Public method to set path data directly
    public void SetPathData(Vector2[] newPathData)
    {
        pathData = newPathData;
        usePathData = true;
        
        // Recalculate the path with new data
        CalculatePathData();
        
        // Reset the snake position
        if (Application.isPlaying && pathPositions.Count > 0)
        {
            snakeHead.position = pathPositions[0];
            distanceTraveled = 0f;
            currentPathIndex = 0;
            UpdateBodySegments();
        }
    }
    
    private void InitializeSnake()
    {
        // Clear existing segments if any
        foreach (var segment in bodySegments)
        {
            if (segment != null)
            {
                Destroy(segment.gameObject);
            }
        }
        
        bodySegments.Clear();
        distanceTraveled = 0f;
        
        // Create initial body segments
        for (int i = 0; i < initialSegmentCount; i++)
        {
            AddBodySegment();
        }
        
        // Position body segments along the path
        UpdateBodySegments();
    }
    
    private void MoveSnakeHead()
    {
        if (pathPositions.Count == 0) return;
        
        // Get current target waypoint
        Vector3 targetPosition;
        if (currentPathIndex < pathPositions.Count)
        {
            targetPosition = pathPositions[currentPathIndex];
        }
        else
        {
            return; // No valid target
        }
        
        // Calculate previous position before moving
        Vector3 previousPosition = snakeHead.position;
        
        // Move towards the target
        Vector3 moveDirection = (targetPosition - snakeHead.position).normalized;
        snakeHead.position += moveDirection * moveSpeed * Time.deltaTime;
        
        // Update distance traveled
        distanceTraveled += Vector3.Distance(previousPosition, snakeHead.position);
        
        // Wrap around total path length if needed
        if (loopPath && distanceTraveled > totalPathLength)
        {
            distanceTraveled -= totalPathLength;
        }
        
        // Rotate towards movement direction
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, moveDirection);
            snakeHead.rotation = Quaternion.Slerp(snakeHead.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // Check if we reached the waypoint
        if (Vector3.Distance(snakeHead.position, targetPosition) < waypointDistanceThreshold)
        {
            // Move to next waypoint
            currentPathIndex++;
            
            // Loop back if needed
            if (currentPathIndex >= pathPositions.Count)
            {
                if (loopPath)
                {
                    currentPathIndex = 0;
                }
                else
                {
                    currentPathIndex = pathPositions.Count - 1;
                }
            }
        }
    }
    
    private void UpdateBodySegments()
    {
        if (pathPositions.Count < 2 || bodySegments.Count == 0) return;
        
        for (int i = 0; i < bodySegments.Count; i++)
        {
            if (bodySegments[i] == null) continue;
            
            // Calculate the distance along the path for this segment
            float segmentDistance = distanceTraveled - (i + 1) * segmentSpacing;
            
            // Handle negative distances (wrap around for looped path)
            if (loopPath)
            {
                while (segmentDistance < 0)
                {
                    segmentDistance += totalPathLength;
                }
            }
            else
            {
                // Clamp to start of path if not looped
                segmentDistance = Mathf.Max(0, segmentDistance);
            }
            
            // Find the position on the path at this distance
            Vector2 position = GetPositionAlongPath(segmentDistance);
            bodySegments[i].transform.position = position;
            
            // Calculate direction for rotation
            float lookAheadDistance = segmentDistance + 0.1f;
            if (loopPath)
            {
                while (lookAheadDistance > totalPathLength)
                {
                    lookAheadDistance -= totalPathLength;
                }
            }
            else
            {
                lookAheadDistance = Mathf.Min(lookAheadDistance, totalPathLength);
            }
            
            Vector2 lookAheadPosition = GetPositionAlongPath(lookAheadDistance);
            Vector2 direction = (lookAheadPosition - position).normalized;
            
            if (direction != Vector2.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, direction);
                bodySegments[i].transform.rotation = Quaternion.Slerp(
                    bodySegments[i].transform.rotation, 
                    targetRotation, 
                    rotationSpeed * Time.deltaTime);
            }
        }
    }
    
    private Vector2 GetPositionAlongPath(float distance)
    {
        if (distance <= 0)
            return pathPositions[0];
            
        float accumulatedDistance = 0;
        
        // Find the path segment that contains this distance
        for (int i = 0; i < pathSegmentLengths.Count; i++)
        {
            int startIndex = i;
            int endIndex = (i + 1) % pathPositions.Count;
            
            float segmentLength = pathSegmentLengths[i];
            
            if (accumulatedDistance + segmentLength >= distance)
            {
                // Found the right segment, now interpolate along it
                float t = (distance - accumulatedDistance) / segmentLength;
                return Vector2.Lerp(pathPositions[startIndex], pathPositions[endIndex], t);
            }
            
            accumulatedDistance += segmentLength;
        }
        
        // If we get here, return the last point
        return pathPositions[pathPositions.Count - 1];
    }
    
    public BodySegment AddBodySegment()
    {
        // Calculate initial position for the new segment
        Vector3 spawnPosition = snakeHead.position;
        if (bodySegments.Count > 0 && bodySegments[bodySegments.Count - 1] != null)
        {
            spawnPosition = bodySegments[bodySegments.Count - 1].transform.position;
        }
        
        // Instantiate new segment
        BodySegment newSegment = Instantiate(bodySegmentPrefab, spawnPosition, Quaternion.identity, transform);
        
        // Set color based on pattern
        if (segmentColors.Length > 0)
        {
            int colorIndex = bodySegments.Count % segmentColors.Length;
            newSegment.SetColor(segmentColors[colorIndex]);
        }
        
        // Subscribe to destruction event
        newSegment.OnDestroyed += HandleSegmentDestroyed;
        
        // Add to list
        bodySegments.Add(newSegment);
        
        return newSegment;
    }
    
    private void HandleSegmentDestroyed(BodySegment destroyedSegment)
    {
        if (bodySegments.Contains(destroyedSegment))
        {
            // Unsubscribe from event
            destroyedSegment.OnDestroyed -= HandleSegmentDestroyed;
            
            // Find the destroyed segment index
            int destroyedIndex = bodySegments.IndexOf(destroyedSegment);
            
            // Remove the segment from our list
            bodySegments.RemoveAt(destroyedIndex);
            
            // Reattach segments after the destroyed one
            RearrangeSegmentsAfterDestruction(destroyedIndex);
        }
    }
    
    private void RearrangeSegmentsAfterDestruction(int destroyedIndex)
    {
        // Move all segments after the destroyed one to the end of the snake
        if (destroyedIndex < bodySegments.Count)
        {
            List<BodySegment> segmentsToMove = new List<BodySegment>();
            
            // Collect segments to move
            for (int i = destroyedIndex; i < bodySegments.Count; i++)
            {
                segmentsToMove.Add(bodySegments[i]);
            }
            
            // Remove them from the current list
            bodySegments.RemoveRange(destroyedIndex, segmentsToMove.Count);
            
            // Add them back at the end
            bodySegments.AddRange(segmentsToMove);
        }
    }
    
    [Button("Thêm Đoạn Thân")]
    private void AddSegmentEditor()
    {
        if (Application.isPlaying)
        {
            AddBodySegment();
        }
    }
    
    [Button("Khởi Tạo Lại Snake + Path")]
    private void ResetSnakeAndPath()
    {
        if (Application.isPlaying)
        {
            CalculatePathData();
            InitializeSnake();
        }
    }
}
