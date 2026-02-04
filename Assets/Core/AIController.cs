using UnityEngine;
using System.Collections.Generic;
using System;

// Simple AI controller with a finite state machine: Idle and Attack
// - Idle: wander randomly around start position and check for player within detectionRange
// - Attack: chase the player (but stop when within stopDistance) and fire bullets at intervals
public class AIController : MonoBehaviour
{
    private enum State { Idle, Attack }
    private State state = State.Idle;

    [Header("Sensing")]
    public float detectionRange =8f;

    [Header("Movement")]
    public float moveSpeed =2.5f;
    public float wanderRadius =9f;
    public float wanderTimer =3f; // time between wander target changes
    private float wanderCounter;
    private Vector3 wanderTarget;
    private Vector3 startPosition;

    [Header("Attack")]
    public float stopDistance =2f; // distance to stop approaching player
    public float fireInterval =0.8f;
    public float spotTimeMax = 1f;
    public float spotTimeMin = 0.1f;
    private float spotTime;
    private float spotCounter;
    private float fireCounter;

    [Header("Bullet")]
    public float bulletSpeed =10f;
    public float spawnOffset =1.5f; // spawn bullet slightly ahead of AI to avoid self-collision

    [Header("Sound")]
    public AudioClip killed;
    public AudioClip fire;
    public AudioClip maskBreak;

    private ShapeWithMask entity;
    private Transform target;
    private SpriteRenderer bodyRenderer;
    private AudioSource soundPlayer;

    // vision tracking (maintained by trigger enter/exit)
    private List<GameObject> visibleEntities = new List<GameObject>();

    private CircleCollider2D visionCollider;

    private void Awake()
    {
        entity = new ShapeWithMask();
        bodyRenderer = GetComponent<SpriteRenderer>();
        soundPlayer = GetComponent<AudioSource>();
        entity.RandomInit();

        // Give AI a mask once at start, but only if the random mask is different from its true shape
        Shape maybeMask = (Shape)UnityEngine.Random.Range(0,3);
        if (maybeMask != entity.GetTrueShape())
        {
            entity.SetMask(maybeMask);
        }

        RenderBody();

    }

    private void Start()
    {
        SetState(State.Idle);
    }

    private void Update()
    {
        // keep trigger radius synced in case detectionRange changed in inspector at runtime
        if (visionCollider != null)
        {
            visionCollider.radius = detectionRange;
        }

        switch (state)
        {
            case State.Idle:
               UpdateIdle();
            break;
            case State.Attack:
               UpdateAttack();
            break;
        }
    }

    public void InitSpawnValue(float _DetectionRange,float _MoveSpeed, float _SpotTimeMin, float _SpotTimeMax, float _BulletSpeed, float _FireInterval)
    {
        detectionRange = _DetectionRange;
        moveSpeed = _MoveSpeed;
        spotTimeMin = _SpotTimeMin;
        spotTimeMax = _SpotTimeMax;
        bulletSpeed = _BulletSpeed;

        visionCollider = GetComponent<CircleCollider2D>();
        if (visionCollider == null)
        {
            visionCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        visionCollider.isTrigger = true;
        visionCollider.radius = detectionRange;

        

        startPosition = transform.position;
        wanderCounter = 0f; // force immediate pick
        fireCounter = 0f;
        spotTime = UnityEngine.Random.Range(spotTimeMin, spotTimeMax);
        spotCounter = spotTime;
    }

    private void SetState(State newState)
    {
        state = newState;
        if (state == State.Idle)
        {
            // pick a wander target immediately
            PickNewWanderTarget();
        }
        else if (state == State.Attack)
        {
            fireCounter = 0f;
        }
    }

     private void UpdateIdle()
    {
       // wander around
        wanderCounter -= Time.deltaTime;
        if (wanderCounter <=0f)
        {
            PickNewWanderTarget();
        }

        // each time we pick a new wander target, attempt to select an attack target from vision
        spotCounter -= Time.deltaTime;
        if (spotCounter <= 0f)
        {
            if (TryPickTargetFromVision())
            {
                return; // switched to attack
            }
            spotCounter = spotTime;
        }
        // move towards wander target
        transform.position = Vector3.MoveTowards(transform.position, wanderTarget, moveSpeed * Time.deltaTime);

        // additional check: if target somehow assigned and within detectionRange, switch
        if (target != null)
        {
            float d = Vector2.Distance(transform.position, target.position);
            if (d <= detectionRange)
            {
               SetState(State.Attack);
            }
        }
    }

    // Try to pick an attack target from visibleEntities. Prioritize player if present and valid. Return true if a target was chosen.
    private bool TryPickTargetFromVision()
    {
        // cleanup
        visibleEntities.RemoveAll(x => x == null);

        // prioritize player
        GameObject player = visibleEntities.Find(go => go.CompareTag("Player"));
        if (player != null)
        {
            var pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                // visible shape to this AI
                if (pc.GetShape() != entity.GetTrueShape())
                {
                    target = player.transform;
                    SetState(State.Attack);
                    return true;
                }
            }
        }

        // otherwise pick a random visible entity whose masked shape differs from our true shape
        var candidates = new List<GameObject>();
        foreach (var go in visibleEntities)
        {
            if (go == null || go == this.gameObject) continue;
            var otherAI = go.GetComponent<AIController>();
            if (otherAI != null)
            {
                if (otherAI.GetShape() != entity.GetTrueShape()) candidates.Add(go);
                continue;
            }
            var otherPlayer = go.GetComponent<PlayerController>();
            if (otherPlayer != null)
            {
                if (otherPlayer.GetShape() != entity.GetTrueShape()) candidates.Add(go);
            }
        }

        if (candidates.Count ==0) return false;

        int idx = UnityEngine.Random.Range(0, candidates.Count);
        target = candidates[idx].transform;
        SetState(State.Attack);
        return true;
    }

    private void UpdateAttack()
    {
        if (target == null)
        {
            SetState(State.Idle);
            return;
        }

        float dist = Vector2.Distance(transform.position, target.position);

        // If player moved out of detection range, return to idle
        if (dist > detectionRange *1.2f)
        {
            // also clear target
            target = null;
            SetState(State.Idle);
            return;
        }

        if (dist > stopDistance)
        {
             transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        }

        fireCounter -= Time.deltaTime;
        if (fireCounter <= 0f)
        {
            fireCounter = fireInterval;
            Vector2 direction = (target.position - transform.position).normalized;
            Vector3 spawnPos = transform.position + (Vector3)direction * spawnOffset;
            Vector2 velocity = direction * bulletSpeed;

            if (GameManager.Instance != null)
            {
                soundPlayer.PlayOneShot(fire, 0.5f);
                GameManager.Instance.TrySpawnBullet(spawnPos, velocity, gameObject);
            }
        }
    }

    private void PickNewWanderTarget()
    {
        Vector2 rnd = UnityEngine.Random.insideUnitCircle * wanderRadius;
        wanderTarget = startPosition + (Vector3)rnd;
        wanderCounter = wanderTimer;
    }

    private void RenderBody()
    {
        switch (entity.GetMaskedShape())
        {
            case Shape.Circle:
                bodyRenderer.sprite = Resources.Load<Sprite>("Image/Circle");
                bodyRenderer.color = Color.red;
                break;
            case Shape.Square:
                bodyRenderer.sprite = Resources.Load<Sprite>("Image/Square");
                bodyRenderer.color = Color.blue;
                break;
            case Shape.Triangle:
                bodyRenderer.sprite = Resources.Load<Sprite>("Image/Triangle");
                bodyRenderer.color = Color.green;
                break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Bullet")) return;

        var bullet = collision.gameObject.GetComponent<Bullet>();
        if (bullet == null)
        {
            // unknown bullet, just destroy
            Destroy(gameObject);
            return;
        }

        GameObject owner = bullet.Owner;

        // perform exposure check before dying
        if (owner != null)
        {
            if (killed == null) Debug.Log("killed is null");
            if (soundPlayer == null) Debug.Log("soundPlayer is null");
            soundPlayer.PlayOneShot(killed, 0.5f);
            HandleKilledBy(owner);
        }
        Destroy(gameObject);
    }

    // Called when this AI is killed by 'owner'. If any other entity in this AI's vision has the same masked shape as owner, then owner's mask breaks.
    private void HandleKilledBy(GameObject owner)
    {
        if (owner == null) return;
        
        // get owner's visible mask (what owner shows to others)
        Shape ownerMask;
        Shape ownerShape;
        var ownerAI = owner.GetComponent<AIController>();
        var ownerPlayer = owner.GetComponent<PlayerController>();
        if (ownerAI != null)
        {
            ownerMask = ownerAI.GetShape();
        }
        else if (ownerPlayer != null)
        {
            ownerMask = ownerPlayer.GetShape();
            ownerShape = ownerPlayer.GetTrueShape();
            if(entity.GetTrueShape() == ownerShape)
            {
                ownerPlayer.AddScore(-1);
            }
            else if(ownerPlayer.HasMask())
            {
                ownerPlayer.AddScore(2);
            }
            else
            {
                ownerPlayer.AddScore(1);
            }
        }
        else
        {
            return;
        }

        // check this AI's vision list for any other entity (besides owner) whose visible mask equals owner's visible mask
        foreach (var go in visibleEntities)
        {
            if (go == null) continue;
            if (go == owner) continue;
            if (go == this.gameObject) continue;

            var ai = go.GetComponent<AIController>();
            if (ai != null)
            {
                if (ai.GetShape() == ownerMask)
                {
                    // owner mask breaks
                    if (ownerAI != null) ownerAI.BreakMask();
                    if (ownerPlayer != null) ownerPlayer.BreakMask();
                    return;
                }
                continue;
            }

            var player = go.GetComponent<PlayerController>();
            if (player != null)
            {
                if (player.GetShape() == ownerMask)
                {
                    if (ownerAI != null) ownerAI.BreakMask();
                    if (ownerPlayer != null) ownerPlayer.BreakMask();
                    return;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null || other.gameObject == null) return;
        var go = other.gameObject;
        if (go == this.gameObject) return;
        if (go.GetComponent<AIController>() != null || go.GetComponent<PlayerController>() != null)
        {
            if (!visibleEntities.Contains(go)) visibleEntities.Add(go);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == null || other.gameObject == null) return;
        var go = other.gameObject;
        if (visibleEntities.Contains(go)) visibleEntities.Remove(go);

        // if the current target left vision, clear target and go idle
        if (target != null && go == target.gameObject)
        {
            target = null;
            SetState(State.Idle);
        }
    }

    public void BreakMask()
    {
        if (entity.hasMask)
        {
            entity.RemoveMask();
            soundPlayer.PlayOneShot(maskBreak, 0.2f);
        }
        RenderBody();
    }

    public Shape GetShape()
    {
        return entity.GetMaskedShape();
    }
}
