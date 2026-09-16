using UnityEngine;

namespace MazeGame.Generation
{
    public class BallSimController : MonoBehaviour
    {
        [SerializeField] Transform mazeRoot;
        [SerializeField] MazeConfig cfg;
        [SerializeField] float floorY = 0.15f; // ball rest height above the floor
        [SerializeField] int substeps = 3;     // guards against tunneling through thin walls

        public System.Action OnFellInHole;

        MazeCollisionGrid collisionGrid;
        Vector2 localPos;
        Vector2 velocity;
        Vector2 holeLocalPos;
        Quaternion ballRotation = Quaternion.identity;
        bool active;

        public void Init(MazeCollisionGrid grid, Vector2 startLocalPos, Vector2 holePos)
        {
            collisionGrid = grid;
            localPos = startLocalPos;
            transform.position = new Vector3(localPos.x, floorY, localPos.y);
            ballRotation = Quaternion.identity;
            transform.rotation = ballRotation;
            velocity = Vector2.zero;
            holeLocalPos = holePos;
            active = true;
        }

        // void LateUpdate()
        // {
        //     if (!active || collisionGrid == null) return;

        //     Vector3 localGravityDir = mazeRoot.InverseTransformDirection(Vector3.down);
        //     Vector2 accel = new Vector2(localGravityDir.x, localGravityDir.z) * cfg.gravityScale;

        //     velocity += accel * Time.deltaTime;
        //     velocity *= Mathf.Clamp01(1f - cfg.friction * Time.deltaTime);
        //     velocity = Vector2.ClampMagnitude(velocity, cfg.maxBallSpeed);

        //     Vector2 step = velocity * Time.deltaTime / substeps;
        //     for (int i = 0; i < substeps; i++)
        //     {
        //         localPos += step;
        //         ResolveCollisions();
        //     }

        //     transform.position = mazeRoot.TransformPoint(new Vector3(localPos.x, floorY, localPos.y));

        //     if (Vector2.Distance(localPos, holeLocalPos) < cfg.holeRadius)
        //     {
        //         active = false;
        //         OnFellInHole?.Invoke();
        //     }
        // }

        // void ResolveCollisions()
        // {
        //     foreach (var wall in collisionGrid.GetNearbyWalls(localPos))
        //     {
        //         Vector2 closest = new Vector2(
        //             Mathf.Clamp(localPos.x, wall.min.x, wall.max.x),
        //             Mathf.Clamp(localPos.y, wall.min.y, wall.max.y));

        //         Vector2 delta = localPos - closest;
        //         float dist = delta.magnitude;

        //         if (dist < cfg.ballRadius)
        //         {
        //             Vector2 normal = dist > 0.0001f ? delta / dist : Vector2.up;
        //             localPos += normal * (cfg.ballRadius - dist);

        //             Vector3 v3 = new Vector3(velocity.x, 0, velocity.y);
        //             Vector3 reflected = Vector3.Reflect(v3, new Vector3(normal.x, 0, normal.y));
        //             velocity = new Vector2(reflected.x, reflected.z) * cfg.restitution;
        //         }
        //     }
        // }
        void LateUpdate()
        {
            if (!active || collisionGrid == null)
                return;

            float dt = Time.deltaTime;

            Vector2 previousPos = localPos;

            // Gravity in maze-local coordinates.
            Vector3 gravityLocal = mazeRoot.InverseTransformDirection(Vector3.down);
            Vector2 accel = new Vector2(gravityLocal.x, gravityLocal.z) * cfg.gravityScale;

            // Integrate velocity.
            velocity += accel * dt;

            // Air/rolling resistance.
            velocity *= Mathf.Exp(-cfg.friction * dt);

            // Clamp speed.
            velocity = Vector2.ClampMagnitude(velocity, cfg.maxBallSpeed);

            float stepDt = dt / substeps;

            for (int s = 0; s < substeps; s++)
            {
                localPos += velocity * stepDt;

                // A few iterations stabilizes corners.
                for (int i = 0; i < 3; i++)
                    ResolveCollisions();
            }

            Vector2 movement = localPos - previousPos;

            if (movement.sqrMagnitude > 0.000001f)
            {
                Vector3 worldMove = mazeRoot.TransformVector(
                    new Vector3(movement.x, 0f, movement.y));

                float distance = worldMove.magnitude;

                if (distance > 0.00001f)
                {
                    Vector3 moveDir = worldMove / distance;

                    // Normal of the maze surface.
                    Vector3 floorNormal = mazeRoot.up;

                    // Axis the ball should roll around.
                    Vector3 axis = Vector3.Cross(floorNormal, moveDir);

                    float angle = (distance / cfg.ballRadius) * Mathf.Rad2Deg;

                    ballRotation = Quaternion.AngleAxis(angle, axis) * ballRotation;
                }
            }

            transform.SetPositionAndRotation(
                mazeRoot.TransformPoint(new Vector3(localPos.x, floorY, localPos.y)),
                ballRotation);

            if (Vector2.Distance(localPos, holeLocalPos) < cfg.holeRadius)
            {
                active = false;
                OnFellInHole?.Invoke();
            }
        }
        void ResolveCollisions()
        {
            foreach (var wall in collisionGrid.GetNearbyWalls(localPos))
            {
                Vector2 closest = new Vector2(
                    Mathf.Clamp(localPos.x, wall.min.x, wall.max.x),
                    Mathf.Clamp(localPos.y, wall.min.y, wall.max.y));

                Vector2 delta = localPos - closest;
                float sqrDist = delta.sqrMagnitude;

                if (sqrDist >= cfg.ballRadius * cfg.ballRadius)
                    continue;

                float dist = Mathf.Sqrt(sqrDist);

                Vector2 normal;

                if (dist > 0.00001f)
                {
                    normal = delta / dist;
                }
                else
                {
                    // Ball center exactly inside wall.
                    // Pick the shallowest push direction.
                    float left   = Mathf.Abs(localPos.x - wall.min.x);
                    float right  = Mathf.Abs(wall.max.x - localPos.x);
                    float bottom = Mathf.Abs(localPos.y - wall.min.y);
                    float top    = Mathf.Abs(wall.max.y - localPos.y);

                    float min = Mathf.Min(left, right, bottom, top);

                    if (min == left)
                        normal = Vector2.left;
                    else if (min == right)
                        normal = Vector2.right;
                    else if (min == bottom)
                        normal = Vector2.down;
                    else
                        normal = Vector2.up;

                    dist = 0f;
                }

                // Push outside wall.
                float penetration = cfg.ballRadius - dist;
                localPos += normal * penetration;

                // Remove velocity INTO the wall.
                float vn = Vector2.Dot(velocity, normal);

                if (vn < 0f)
                {
                    velocity -= normal * vn;

                    // Optional: tiny energy loss when scraping walls.
                    velocity *= (1f - cfg.restitution * 0.1f);
                }
            }
        }
    }
}
