using UnityEngine;

public interface IBullet
{
    int ColorId { get; set; }
    float Damage { get; set; }
    float Speed { get; set; }
    float HitDistance { get; set; }
    void Fire(IHitable target, Vector3 startPosition);
    void DestroyBullet();
} 