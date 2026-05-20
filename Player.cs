using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public Text coinText;
    public int currentCoin = 0;

    public int maxHealth = 3;
    public Text health;

    private Rigidbody2D rb;
    private Animator animator;

    public float jumpHeight = 10f;
    private bool isGround = true;

    private float movement;
    public float moveSpeed = 10f;

    private bool facingRight = true;
    private bool isDead = false;

    public Transform attackPoint;
    public float attackRadius = 1f;
    public LayerMask attackLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (coinText != null)
            coinText.text = currentCoin.ToString();

        if (health != null)
            health.text = maxHealth.ToString();
    }

    void Update()
    {
        if (isDead) return;

        if (coinText != null)
            coinText.text = currentCoin.ToString();

        if (health != null)
            health.text = maxHealth.ToString();

        movement = Input.GetAxis("Horizontal");

        if (movement < 0f && facingRight)
        {
            transform.eulerAngles = new Vector3(0f, -180f, 0f);
            facingRight = false;
        }
        else if (movement > 0f && !facingRight)
        {
            transform.eulerAngles = new Vector3(0f, 0f, 0f);
            facingRight = true;
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            Jump();
            isGround = false;

            if (animator != null)
                animator.SetBool("Jump", true);
        }

        if (animator != null)
            animator.SetFloat("Run", Mathf.Abs(movement));

        if (Input.GetMouseButtonDown(0))
        {
            if (animator != null)
                animator.SetTrigger("Attack");
        }
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        transform.position += new Vector3(movement, 0f, 0f) * Time.fixedDeltaTime * moveSpeed;
    }

    void Jump()
    {
        Vector2 vel = rb.velocity;
        vel.y = jumpHeight;
        rb.velocity = vel;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGround = true;

            if (animator != null)
                animator.SetBool("Jump", false);
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(1);
        }
    }

    public void Attack()
    {
        Collider2D collInfo = Physics2D.OverlapCircle(
            attackPoint.position,
            attackRadius,
            attackLayer
        );

        if (collInfo != null)
        {
            PatrolEnemy enemy = collInfo.GetComponent<PatrolEnemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(1);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        maxHealth -= damage;

        if (maxHealth <= 0)
        {
            Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Coin"))
        {
            currentCoin++;

            Collider2D coinCollider = other.GetComponent<Collider2D>();
            if (coinCollider != null)
                coinCollider.enabled = false;

            if (other.transform.childCount > 0)
            {
                Animator coinAnimator = other.transform.GetChild(0).GetComponent<Animator>();

                if (coinAnimator != null)
                    coinAnimator.SetTrigger("Collected");
            }

            Destroy(other.gameObject, 1f);
        }

        if (other.CompareTag("VictoryPoint"))
        {
            Debug.Log("Victory");

            GameManager gameManager = FindObjectOfType<GameManager>();

            if (gameManager != null)
            {
                gameManager.isGameActive = false;
            }
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log("Player Died");

        GameManager gameManager = FindObjectOfType<GameManager>();

        if (gameManager != null)
        {
            gameManager.isGameActive = false;
        }

        if (animator != null)
            animator.SetTrigger("Dead");

        moveSpeed = 0f;
        rb.velocity = Vector2.zero;

        StartCoroutine(RestartAfterDeath());
    }

    IEnumerator RestartAfterDeath()
    {
        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}