using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private ShapeWithMask entity;

    private SpriteRenderer maskRenderer;
    private SpriteRenderer bodyRenderer;
    private Rigidbody2D playerBody;
    private AudioSource soundPlayer;

    private float moveX;
    private float moveY;

    public int Hp { get; private set; }
    public int Score { get; private set; }

    public float moveSpeed;

    public float bulletSpeed = 10f;
    public Transform firePoint; // optional: where bullets spawn; if null, use player position
    public float spawnOffset = 2f; // offset in front of player to spawn bullets to avoid hitting player

    public int initialHp = 5;
    public int initialScore = 0;
    public float timer = 0;

    public AudioClip maskBreak;
    public AudioClip killed;
    public AudioClip fire;

    public Action<int> OnScoreChanged;
    public Action<int> OnHpChanged;

    private void Awake()
    {
        bodyRenderer = gameObject.GetComponent<SpriteRenderer>();
        maskRenderer = gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
        playerBody = gameObject.GetComponent<Rigidbody2D>();
        soundPlayer = gameObject.GetComponent<AudioSource>();
    }


    void Start()
    {
        entity = new ShapeWithMask();
        entity.RandomInit();
        Hp = initialHp;
        RenderBody();
    }

    void Update()
    {
        moveX = Input.GetAxis("Horizontal");
        moveY = Input.GetAxis("Vertical");

        Vector2 dir = new Vector2(moveX, moveY).normalized;

        playerBody.velocity = dir * moveSpeed;

        timer += Time.deltaTime;

        // Fire bullet toward mouse click
        if (Input.GetMouseButtonDown(0) && GameManager.Instance != null && GameManager.Instance.bulletPrefab != null)
        {
            Camera cam = Camera.main;
            if (cam == null) 
            {
                Debug.Log("cam is null"); 
                return; 
            }

            Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;

            Vector3 baseSpawn = (firePoint != null) ? firePoint.position : transform.position;
            Vector2 toTarget = (mouseWorld - baseSpawn).normalized;
            Vector3 spawnPos = baseSpawn + (Vector3)toTarget * spawnOffset;

            Vector2 velocity = toTarget * bulletSpeed + dir;

            soundPlayer.PlayOneShot(fire,0.4f);
            GameManager.Instance.TrySpawnBullet(spawnPos, velocity, gameObject);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            if(Score >= 5)
            {
                int trueShapeValue = (int)entity.GetTrueShape();
                int randomValue = UnityEngine.Random.Range(0, 2);
                int newShapeValue = (randomValue >= trueShapeValue) ? randomValue + 1 : randomValue;

                Shape newMaskShape = (Shape)newShapeValue;
                entity.SetMask(newMaskShape);
                RefreshMaskStatus();

                Score -= 5;
                OnScoreChanged(Score);
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            entity.RemoveMask();
            RefreshMaskStatus();
        }

        if(Input.GetKeyDown(KeyCode.H))
        {
            if(Score > 0 && Hp < 10)
            {
                RecoverHp(1);
            }
        }

        if(Hp <= 0 || Score <= -10)
        {
            GameOver();
        }

        if(Score >= 100)
        {
            GameWon();
        }
    }

    private void RenderBody()
    {
        switch (entity.GetTrueShape())
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


    private void RefreshMaskStatus()
    {
        if (entity.hasMask)
        {
            switch (entity.GetMaskedShape())
            {
                case Shape.Circle:
                    maskRenderer.sprite = Resources.Load<Sprite>("Image/Circle");
                    maskRenderer.color = Color.red;
                    break;
                case Shape.Square:
                    maskRenderer.sprite = Resources.Load<Sprite>("Image/Square");
                    maskRenderer.color = Color.blue;
                    break;
                case Shape.Triangle:
                    maskRenderer.sprite = Resources.Load<Sprite>("Image/Triangle");
                    maskRenderer.color = Color.green;
                    break;
            }
            Color c = maskRenderer.color;
            c.a = 0.5f;
            maskRenderer.color = c;
        }
        else
        {
            maskRenderer.sprite = null;
        }
    }

    private void RecoverHp(int HpToRecover)
    {
        if(Score >= HpToRecover)
        {
            Score -= HpToRecover;
            OnScoreChanged(Score);
            Hp += HpToRecover;
            OnHpChanged(Hp);
        }
    }

    public void AddScore(int ScoreToAdd)
    {
        soundPlayer.PlayOneShot(killed, 0.5f);
        Score += ScoreToAdd;
        OnScoreChanged(Score);
    }

    // Allow external callers to break player's mask
    public void BreakMask()
    {
        if (entity.hasMask)
        {
            entity.RemoveMask();
            soundPlayer.PlayOneShot(maskBreak, 0.5f);
        }
        RefreshMaskStatus();
    }

    public Shape GetShape()
    {
        return entity.GetMaskedShape();
    }

    public Shape GetTrueShape()
    {
        return entity.GetTrueShape();
    }

    public bool HasMask()
    {
        return entity.hasMask;
    }

    private void GameOver()
    {
        bodyRenderer.sprite = null;
        maskRenderer.sprite = null;
        UIManager.Instance.OpenPanel("GameOverMenu");

        this.gameObject.SetActive(false);
        //Destroy(this);
    }

    private void GameWon()
    {
        bodyRenderer.sprite = null;
        maskRenderer.sprite = null;
        YouWonMenu ywm = (YouWonMenu)UIManager.Instance.OpenPanel("YouWonMenu");

        ywm.SetTimeTaken(timer);

        this.gameObject.SetActive(false);
    }

    public void Reset()
    {
        transform.position = Vector2.zero;
        entity.RandomInit();
        timer = 0;
        Hp = initialHp;
        OnHpChanged(Hp);
        RenderBody();
        Score = 0;
        OnScoreChanged(Score);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision != null && collision.gameObject.CompareTag("Bullet"))
        {
            Hp--;
            OnHpChanged(Hp);
            var ai = collision.gameObject.GetComponent<AIController>();
            if (ai != null) ai.BreakMask();
        }
    }
}
