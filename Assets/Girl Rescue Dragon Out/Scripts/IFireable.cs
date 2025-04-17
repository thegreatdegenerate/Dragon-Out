using System;
using UnityEngine;

public interface IFireable
{
    int ColorId { get; }
    int RemainingBullets { get; }
    Vector3 FirePosition { get; }
    float FireRate { get; }
    float NextFireTime { get; set; }
    void Fire(IHitable target);
    bool CanFire();
    
    event Action<IFireable> OnOutOfBullets;
} 
