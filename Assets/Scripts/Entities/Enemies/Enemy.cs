using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    protected Transform player;

    public virtual void Initialize(Transform playerTransform)
    {
        // all enemies must be initialized with a reference to the player
        player = playerTransform;
    }
}