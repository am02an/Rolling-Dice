using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    public enum EnemyType { Melee, Ambusher, PatrolGuard, CastleSeeker }
    public enum State { Idle, Hide, Patrol, Chase, Attack, SeekCastle, Cooldown }

    [Header("Setup")]
    public EnemyType type = EnemyType.Melee;
    public Transform player;
    public Transform castle;
    private Rigidbody rb;
    public Animator anim;

    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float rotationSpeed = 10f;

    [Header("Combat")]
    public float attackRange = 1.8f;
    public float attackDamage = 15f;
    public float attackCooldown = 1.2f;

    [Header("Vision & Hearing")]
    public float viewRange = 10f;
    public float viewAngle = 120f;
    public float hearRange = 5f;
    public LayerMask losMask = ~0;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    private int patrolIndex = 0;
    public float patrolWaitTime = 2f;

    [Header("Ambush")]
    public string hideSpotTag = "HideSpot";
    public float ambushTriggerRange = 4f;

    private float cooldownEnd;
    private float stateTimer;
    public State state = State.Idle;

    // Cached hash IDs for animation
    private int hashSpeed = Animator.StringToHash("Speed");

    private Vector3 moveTarget;
    private bool hasTarget = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation; // prevent physics flipping enemy

        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
        if (!castle)
        {
            var c = GameObject.FindGameObjectWithTag("Castle");
            if (c) castle = c.transform;
        }

        // Initial state
        switch (type)
        {
            case EnemyType.Melee: state = State.Chase; break;
            case EnemyType.Ambusher: state = State.Hide; break;
            case EnemyType.PatrolGuard: state = patrolPoints.Length > 0 ? State.Patrol : State.Chase; break;
            case EnemyType.CastleSeeker: state = State.SeekCastle; break;
        }
    }

    private void Update()
    {
        stateTimer += Time.deltaTime;

        switch (state)
        {
            case State.Idle: HandleIdle(); break;
            case State.Hide: HandleHide(); break;
            case State.Patrol: HandlePatrol(); break;
            case State.Chase: HandleChase(); break;
            case State.Attack: HandleAttack(); break;
            case State.SeekCastle: HandleSeekCastle(); break;
            case State.Cooldown: HandleCooldown(); break;
        }

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (hasTarget)
        {
            Vector3 dir = (moveTarget - transform.position);
            dir.y = 0f;

            if (dir.magnitude > 0.1f)
            {
                // Move enemy
                Vector3 newPos = rb.position + dir.normalized * moveSpeed * Time.fixedDeltaTime;
                rb.MovePosition(newPos);

                // Rotate smoothly
                Quaternion targetRot = Quaternion.LookRotation(dir);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
            }
        }
    }

    // -----------------------------------------------------
    // STATES
    // -----------------------------------------------------
    void HandleIdle()
    {
        StopMoving();

        if (CanSeePlayer() || CanHearPlayer())
            ChangeState(State.Chase);
        else if (type == EnemyType.CastleSeeker && castle)
            ChangeState(State.SeekCastle);
    }

    void HandleHide()
    {
        if (stateTimer < 0.1f)
        {
            Transform spot = FindNearestHideSpot();
            if (spot) SetDestination(spot.position);
        }

        if (player && Vector3.Distance(transform.position, player.position) <= ambushTriggerRange)
            ChangeState(State.Chase);
    }

    void HandlePatrol()
    {
        if (CanSeePlayer() || CanHearPlayer())
        {
            ChangeState(State.Chase);
            return;
        }

        if (!hasTarget)
        {
            SetDestination(patrolPoints[patrolIndex].position);
        }
        else if (Vector3.Distance(transform.position, patrolPoints[patrolIndex].position) < 0.5f)
        {
            if (stateTimer >= patrolWaitTime)
            {
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                SetDestination(patrolPoints[patrolIndex].position);
                stateTimer = 0f;
            }
        }
    }

    void HandleChase()
    {
        if (!player) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            StopMoving();
            PerformAttack(player);
            return;
        }

        SetDestination(player.position);

        if (!CanSeePlayer() && !CanHearPlayer())
        {
            if (type == EnemyType.PatrolGuard && patrolPoints.Length > 0)
                ChangeState(State.Patrol);
            else if (type == EnemyType.CastleSeeker && castle)
                ChangeState(State.SeekCastle);
        }
    }

    void HandleAttack()
    {
        StopMoving();

        if (player) FaceTarget(player.position);

        if (Time.time >= cooldownEnd)
        {
            float dist = player ? Vector3.Distance(transform.position, player.position) : Mathf.Infinity;
            if (dist <= attackRange)
                PerformAttack(player);
            else
                ChangeState(State.Chase);
        }
    }

    void HandleSeekCastle()
    {
        if (player && (CanSeePlayer() || CanHearPlayer()))
        {
            ChangeState(State.Chase);
            return;
        }

        if (!castle) return;

        if (TryAttackTarget(castle)) return;

        SetDestination(castle.position);
    }

    void HandleCooldown()
    {
        StopMoving();
        if (Time.time >= cooldownEnd)
            ChangeState(State.Chase);
    }

    // -----------------------------------------------------
    // ANIMATION
    // -----------------------------------------------------
    void UpdateAnimation()
    {
        if (!anim) return;

        float speed = hasTarget ? moveSpeed : 0f;
        anim.SetFloat(hashSpeed, speed);
    }

    // -----------------------------------------------------
    // ATTACK
    // -----------------------------------------------------
    bool TryAttackTarget(Transform target)
    {
        if (Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            FaceTarget(target.position);
            PerformAttack(target);
            return true;
        }
        return false;
    }

    void PerformAttack(Transform target)
    {
        if (!anim) return;

        int randomAttack = Random.Range(0, 4);
        anim.SetInteger("AttackIndex", randomAttack);
        anim.SetTrigger("Attack");

        Debug.Log($"{name} performs Attack {randomAttack + 1} on {target.name}");

        cooldownEnd = Time.time + attackCooldown;
        ChangeState(State.Attack);
    }

    public void DealDamage() // Animation Event
    {
        if (player && Vector3.Distance(transform.position, player.position) <= attackRange)
            Debug.Log($"{name} hit {player.name} for {attackDamage} damage!");

        if (castle && Vector3.Distance(transform.position, castle.position) <= attackRange)
            Debug.Log($"{name} hit the castle for {attackDamage} damage!");
    }

    // -----------------------------------------------------
    // UTILITIES
    // -----------------------------------------------------
    bool CanSeePlayer()
    {
        if (!player) return false;
        Vector3 dir = (player.position - transform.position);
        if (dir.sqrMagnitude > viewRange * viewRange) return false;
        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > viewAngle * 0.5f) return false;

        if (Physics.Raycast(transform.position + Vector3.up, dir.normalized, out RaycastHit hit, viewRange, losMask))
            return hit.transform == player || hit.transform.IsChildOf(player);

        return false;
    }

    bool CanHearPlayer()
    {
        if (!player) return false;
        return Vector3.Distance(transform.position, player.position) <= hearRange;
    }

    Transform FindNearestHideSpot()
    {
        GameObject[] spots = GameObject.FindGameObjectsWithTag(hideSpotTag);
        Transform nearest = null;
        float minDist = Mathf.Infinity;
        foreach (var s in spots)
        {
            float d = Vector3.SqrMagnitude(s.transform.position - transform.position);
            if (d < minDist) { minDist = d; nearest = s.transform; }
        }
        return nearest;
    }

    void FaceTarget(Vector3 pos)
    {
        Vector3 dir = (pos - transform.position); dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion look = Quaternion.LookRotation(dir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, look, Time.deltaTime * rotationSpeed));
        }
    }

    void SetDestination(Vector3 target)
    {
        moveTarget = target;
        hasTarget = true;
    }

    void StopMoving()
    {
        hasTarget = false;
    }

    void ChangeState(State next)
    {
        state = next;
        stateTimer = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
