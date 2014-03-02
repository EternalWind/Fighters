using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float HP = 100.0f;
    public float Speed = 4.0f;
    public GameObject Bullet;
    public GameObject Explosion;
    public Texture HPIndicator;

    private Sprite m_Sprite;
    private Sprite m_BulletSprite;
    private Bullet m_Bullet;
    private float m_RemainCoolDownTime = 0.0f;
    private float m_CoolDownTime;
    private Animator m_Animator;
    private float m_CurrentHP;

    // Use this for initialization
    void Start()
    {
        m_Sprite = GetComponent<SpriteRenderer>().sprite;
        m_BulletSprite = Bullet.GetComponent<SpriteRenderer>().sprite;
        m_CoolDownTime = Bullet.GetComponent<Bullet>().CoolDownTime;
        m_Animator = GetComponent<Animator>();
        m_Bullet = Bullet.GetComponent<Bullet>();
        m_CurrentHP = HP;
    }

    void OnGUI()
    {
        var length = 50.0f;
        var height = 5.0f;
        var pos = transform.position;
        pos.y += m_Sprite.bounds.extents.y;

        pos = Camera.main.WorldToScreenPoint(pos);
        pos.y = Screen.height - pos.y;

        pos.x -= length / 2.0f;
        pos.y -= height + 1.0f;

        GUI.DrawTexture(new Rect(pos.x, pos.y, length * m_CurrentHP / HP, height), HPIndicator, ScaleMode.StretchToFill);
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

    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        var sync_pos = new Vector3();
        var sync_rot = new Quaternion();
        var sync_hp = 0.0f;

        if (stream.isReading)
        {
            stream.Serialize(ref sync_pos);
            stream.Serialize(ref sync_rot);
            stream.Serialize(ref sync_hp);

            transform.position = sync_pos;
            transform.rotation = sync_rot;
            m_CurrentHP = sync_hp;
        }
        else
        {
            sync_pos = transform.position;
            sync_rot = transform.rotation;
            sync_hp = m_CurrentHP;

            stream.Serialize(ref sync_pos);
            stream.Serialize(ref sync_rot);
            stream.Serialize(ref sync_hp);
        }
    }

    [RPC]
    void Hit()
    {
        m_CurrentHP -= m_Bullet.Damage;

        if (m_CurrentHP <= 0.0f)
        {
            var exp_pos = new Vector3(transform.position.x, transform.position.y, -3.0f);

            Network.Instantiate(Explosion, exp_pos, Quaternion.identity, 0);
            Network.Destroy(gameObject);
        }
    }
}
