using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float Speed = 4.0f;
    public GameObject Bullet;
    public GameObject Explosion;

    private Sprite m_Sprite;
    private Sprite m_BulletSprite;
    private float m_RemainCoolDownTime = 0.0f;
    private float m_CoolDownTime;
    private Animator m_Animator;

    // Use this for initialization
    void Start()
    {
        m_Sprite = GetComponent<SpriteRenderer>().sprite;
        m_BulletSprite = Bullet.GetComponent<SpriteRenderer>().sprite;
        m_CoolDownTime = Bullet.GetComponent<Bullet>().CoolDownTime;
        m_Animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (networkView.isMine)
        {
            var self_pos = transform.position;
            var mouse_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var direction = mouse_pos - self_pos;
            direction.z = 0.0f;

            var rot = transform.rotation;
            rot.SetFromToRotation(new Vector3(0.0f, 1.0f), direction);
            transform.rotation = rot;

            var move_h = Input.GetAxis("Horizontal");
            var move_v = Input.GetAxis("Vertical");
            var move = new Vector2(move_h, move_v);
            move.Normalize();

            rigidbody2D.velocity = move * Speed;

            if (Input.GetButton("Fire1") && m_RemainCoolDownTime <= 0.0f)
            {
                m_RemainCoolDownTime = m_CoolDownTime;

                var self_rot = transform.rotation;
                var self_ext = m_Sprite.bounds.extents;
                var bullet_ext = m_BulletSprite.bounds.extents;
                var bullet_pos = new Vector3(0.0f, self_ext.y + bullet_ext.y + 0.1f);
                bullet_pos = self_rot * bullet_pos + self_pos;

                Network.Instantiate(Bullet, bullet_pos, Bullet.transform.rotation * self_rot, 0);
            }

            if (m_RemainCoolDownTime > 0.0f)
                m_RemainCoolDownTime -= Time.deltaTime;

            m_Animator.speed = move.magnitude;
        }
    }

    void Hit()
    {
        var exp_pos = new Vector3(transform.position.x, transform.position.y, -3.0f);

        Instantiate(Explosion, exp_pos, Quaternion.identity);
        Network.Destroy(gameObject);
    }
}
