using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    public float Speed = 6.0f;
    public float CoolDownTime = 0.5f;
    public float KeepAliveTime = 5.0f;
    public float Damage = 25.0f;

    private Quaternion m_RotCompensation;

    // Use this for initialization
    void Start()
    {
        m_RotCompensation = Quaternion.FromToRotation(new Vector3(0.0f, 1.0f), new Vector3(-1.0f, 0.0f));

        if (networkView.isMine)
            StartCoroutine(DestroySelf());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (networkView.isMine)
        {
            var pos = transform.position + transform.rotation * m_RotCompensation * (new Vector3(0.0f, Speed) * Time.deltaTime);
            transform.position = pos;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && networkView.isMine)
        {
            other.networkView.RPC("Hit", other.networkView.owner);
            Network.Destroy(gameObject);
        }
    }

    IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(KeepAliveTime);
        Network.Destroy(gameObject);
    }
}
