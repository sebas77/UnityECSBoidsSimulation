﻿using UnityEngine;
using System.Collections.Generic;

namespace Boid.OOP
{

public class Boid : MonoBehaviour
{
    public Simulation simulation { get; set; }
    public Param param { get; set; }
    public Vector3 pos { get; private set; }
    public Vector3 velocity { get; private set; }
    Vector3 accel = Vector3.zero;
    List<Boid> neighbors = new List<Boid>();

    void Start()
    {
        pos = transform.position;
        velocity = transform.forward * param.initSpeed;
    }

    void Update()
    {
        UpdateNeighbors();
        UpdateWalls();
        if (neighbors.Count > 0)
        {
            UpdateSeparation();
            UpdateAlignment();
            UpdateCohesion();
        }
        UpdateMove();
    }

    void UpdateNeighbors()
    {
        neighbors.Clear();

        if (!simulation) return;

        var prodThresh = Mathf.Cos(param.neighborFov * Mathf.Deg2Rad);
        var distThresh = param.neighborDistance;

        foreach (var other in simulation.boids)
        {
            if (other == this) continue;

            var to = other.pos - pos;
            var dist = to.magnitude;
            if (dist < distThresh)
            {
                var dir = to.normalized;
                var fwd = velocity.normalized;
                var prod = Vector3.Dot(fwd, dir);
                if (prod > prodThresh)
                {
                    neighbors.Add(other);
                }
            }
        }
    }

    void UpdateSeparation()
    {
        if (neighbors.Count == 0) return;

        Vector3 force = Vector3.zero;
        foreach (var neighbor in neighbors)
        {
            force += (pos - neighbor.pos).normalized;
        }
        force /= neighbors.Count;

        accel += force * param.separationWeight;
    }

    void UpdateWalls()
    {
        if (!simulation) return;

        var scale = param.wallScale * 0.5f;
        accel +=
            GetAccelAgainstWall(-scale - pos.x, Vector3.right) +
            GetAccelAgainstWall(-scale - pos.y, Vector3.up) +
            GetAccelAgainstWall(-scale - pos.z, Vector3.forward) +
            GetAccelAgainstWall(+scale - pos.x, Vector3.left) +
            GetAccelAgainstWall(+scale - pos.y, Vector3.down) +
            GetAccelAgainstWall(+scale - pos.z, Vector3.back);
    }

    Vector3 GetAccelAgainstWall(float distance, Vector3 dir)
    {
        if (distance < param.wallDistance)
        {
            return dir * (param.wallWeight / Mathf.Abs(distance / param.wallDistance));
        }
        return Vector3.zero;
    }

    void UpdateAlignment()
    {
        var averageVelocity = Vector3.zero;
        foreach (var neighbor in neighbors)
        {
            averageVelocity += neighbor.velocity;
        }
        averageVelocity /= neighbors.Count;

        accel += (averageVelocity - velocity) * param.alignmentWeight;
    }

    void UpdateCohesion()
    {
        var averagePos = Vector3.zero;
        foreach (var neighbor in neighbors)
        {
            averagePos += neighbor.pos;
        }
        averagePos /= neighbors.Count;

        accel += (averagePos - pos) * param.cohesionWeight;
    }

    void UpdateMove()
    {
        var dt = Time.deltaTime;

        velocity += accel * dt;
        var dir = velocity.normalized;
        var speed = velocity.magnitude;
        velocity = Mathf.Clamp(speed, param.minSpeed, param.maxSpeed) * dir;
        pos += velocity * dt;

        var rot = Quaternion.LookRotation(velocity);
        transform.SetPositionAndRotation(pos, rot);

        accel = Vector3.zero;
    }
}

}