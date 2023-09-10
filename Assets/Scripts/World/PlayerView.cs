﻿using UnityEngine;

public sealed class PlayerView : MonoBehaviour
{
    public Configuration config;
    
    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);

        Gizmos.color = transparentGreen;

        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - config.groundedOffset, transform.position.z),
            config.groundedRadius);
    }
}