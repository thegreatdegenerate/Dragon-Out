using UnityEngine;

public interface IHitable
{
    int ColorId { get; }
    bool IsAlive { get; }
    bool HasTarget { get; }
    Vector3 Position { get; }
    void TakeDamage(float damage);
    void SetAsTargeted();
    void ClearTargeted();
} 