using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    private Rigidbody2D rb;
    private float lifeTime =0.2f;
    public GameObject Owner { get; set; }

    private System.Action<GameObject> returnCallback;
    private Coroutine lifeCoroutine;

    private void Awake()
    {
       rb = GetComponent<Rigidbody2D>();
    }

     public void Shoot(Vector2 velocity, float lifetime, GameObject owner, System.Action<GameObject> returnToPool)
     {
        lifeTime = lifetime;
        returnCallback = returnToPool;

        gameObject.SetActive(true);
        rb.velocity = velocity;

        Owner = owner;

        if (lifeCoroutine != null)
        StopCoroutine(lifeCoroutine);
        lifeCoroutine = StartCoroutine(LifeRoutine());
     }

     private IEnumerator LifeRoutine()
     {
        yield return new WaitForSeconds(lifeTime);
        ReturnToPool();
     }

     private void OnCollisionEnter2D(Collision2D collision)
     {
        // Return to pool on any collision
        ReturnToPool();
     }

    private void ReturnToPool()
    {
        rb.velocity = Vector2.zero;
        if (lifeCoroutine != null)
        {
            StopCoroutine(lifeCoroutine);
            lifeCoroutine = null;
        }
        returnCallback?.Invoke(gameObject);
    }
}
