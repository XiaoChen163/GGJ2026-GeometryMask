using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Bullet Pool")]
    public GameObject bulletPrefab;
    public int poolSize = 200;
    public int maxActiveBullets = 100;
    public float bulletLifeTime = 0.2f;
    public bool expandPool = true; // allow Instantiate when pool empty

    [Header("AI Spawning")]
    public GameObject aiPrefab;
    public GameObject playerPrefab;
    public GameObject VirtualCamera;
    public bool enableAISpawning = true;
    public float aiSpawnInterval = 2f;
    public float safeDistance = 6f; // ai won't spawn within this distance of the camera center
    public int maxAICount = 20;
    public int maxSpawnAttempts = 30;

    [Header("AI Value")]
    public float _DetectionRange = 6f;
    public float _MoveSpeed = 3f;
    public float _SpotTimeMin = 0.5f;
    public float _SpotTimeMax = 1.5f;
    public float _BulletSpeed = 20f;
    public float _FireInterval = 1f;


    private Queue<GameObject> bulletPool = new Queue<GameObject>();
    private int activeBulletCount = 0;

    private List<GameObject> spawnedAIs = new List<GameObject>();
    private GameObject ActivePlayer;

    private bool isStarted = false;
    private Coroutine aiSpawnCoroutine;
    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitPool();
    }

    public void Start()
    {
        UIManager.Instance.OpenPanel("MainMenu");
        BgmPlayer.Instance.playRandomMusic();
    }

    public void GameStart()
    {
        if (isStarted) return;

        isStarted = true;

        ActivePlayer = Instantiate(playerPrefab, new Vector2(0, 0), Quaternion.identity);

        CinemachineVirtualCamera cvc = VirtualCamera.GetComponent<CinemachineVirtualCamera>();
        cvc.Follow = ActivePlayer.transform;


        if (enableAISpawning && aiPrefab != null)
        {
            aiSpawnCoroutine = StartCoroutine(AISpawnLoop());
        }
    }

    public void ChangeAiValue(float detectionRange, float moveSpeed, float spotTimeMin, float spotTimeMax, float bulletSpeed, float fireInterval)
    {
        _DetectionRange = detectionRange;
        _MoveSpeed = moveSpeed;
        _SpotTimeMin = spotTimeMin;
        _SpotTimeMax = spotTimeMax;
        _BulletSpeed = bulletSpeed;
        _FireInterval = fireInterval;
    }

    public void GameReStart()
    {
        if (aiSpawnCoroutine != null)
        {
            StopCoroutine(aiSpawnCoroutine);
            aiSpawnCoroutine = null;
        }

        foreach (var ai in spawnedAIs)
        {
            if (ai != null)
                Destroy(ai);
        }
        spawnedAIs.Clear();

        ActivePlayer.SetActive(true);
        ActivePlayer.GetComponent<PlayerController>().Reset();

        
        if (enableAISpawning && aiPrefab != null)
        {
            aiSpawnCoroutine = StartCoroutine(AISpawnLoop());
        }

    }

    private void InitPool()
    {
        if (bulletPrefab == null) return;
        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = Instantiate(bulletPrefab);
            go.SetActive(false);
            bulletPool.Enqueue(go);
        }
    }

    private IEnumerator AISpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(aiSpawnInterval);
            // cleanup dead AIs
            spawnedAIs.RemoveAll(item => item == null);

            if (spawnedAIs.Count >= maxAICount) continue;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) continue;

            Camera cam = Camera.main;

            bool spawned = false;
            for (int attempt = 0; attempt < maxSpawnAttempts && !spawned; attempt++)
            {
                float xpos = Random.Range(-17.2f, 17.2f);
                float ypos = Random.Range(-9.5f, 9.5f);
                Vector3 pos = new Vector3(xpos, ypos, 0f);

                if (Vector2.Distance(pos,cam.gameObject.transform.position) < safeDistance) continue;

                GameObject ai = Instantiate(aiPrefab, pos, Quaternion.identity);
                ai.GetComponent<AIController>().InitSpawnValue(_DetectionRange,_MoveSpeed,_SpotTimeMin,_SpotTimeMax,_BulletSpeed,_FireInterval);
                spawnedAIs.Add(ai);
                spawned = true;
            }
        }
    }

    private GameObject GetBulletFromPool()
    {
        if (bulletPool.Count > 0)
        {
            GameObject go = bulletPool.Dequeue();
            go.SetActive(true);
            return go;
        }

        if (expandPool)
        {
            GameObject go = Instantiate(bulletPrefab);
            go.SetActive(true);
            return go;
        }

        return null;
    }

    // owner is optional; passed to Bullet.Shoot so bullets can ignore collisions with owner if desired
    public bool TrySpawnBullet(Vector3 position, Vector2 velocity, GameObject owner = null)
    {
        if (bulletPrefab == null) return false;
        if (activeBulletCount >= maxActiveBullets) return false;

        GameObject go = GetBulletFromPool();
        if (go == null) return false;

        go.transform.position = position;
        go.transform.rotation = Quaternion.identity;

        Bullet bulletComp = go.GetComponent<Bullet>();
        activeBulletCount++;

        System.Action<GameObject> returnToPool = ReturnBullet;

        if (bulletComp != null)
        {
            bulletComp.Shoot(velocity, bulletLifeTime, owner, returnToPool);
        }
        else
        {
            Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                go.SetActive(true);
                rb.velocity = velocity;
                StartCoroutine(ReturnAfterLifetime(go, bulletLifeTime));
            }
            else
            {
                // nothing to move the bullet with; return it
                ReturnBullet(go);
                return false;
            }
        }
        return true;
    }

    private void ReturnBullet(GameObject go)
    {
        go.SetActive(false);
        bulletPool.Enqueue(go);
        activeBulletCount = Mathf.Max(0, activeBulletCount - 1);
    }

    private IEnumerator ReturnAfterLifetime(GameObject go, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        ReturnBullet(go);
    }
}
