using Photon.Pun;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public float health = 100;
    public float maxHealth = 100;

    public Transform shootPoint;
    public Transform orientation;
    public GameObject bullet;

    public float BulletSpeed = 100f;

    public PlayerTyp PlayerTyp;

    public PlayerCam PlayerCameraController;

    private ParticleSystem _particleSystem;

    private bool canShoot = true;

    void Awake()
    {
        _particleSystem = shootPoint.gameObject.GetComponentInChildren<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            WeaponHandler();
        }
    }

    void WeaponHandler()
    {
        if (Input.GetKey(KeyCode.Mouse0) && canShoot && PlayerTyp == PlayerTyp.TEACHER)
        {
            photonView.RPC(nameof(Weapon_Shoot), RpcTarget.All);
        }
    }

    [PunRPC]
    void Weapon_Shoot()
    {
        if (_particleSystem != null) _particleSystem.Play();
        
        if (photonView.IsMine)
        {
            if (!canShoot) return;

            canShoot = false;

            Ray ray = PlayerCameraController.camObject.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;

            Vector3 targetPoint;
            if (Physics.Raycast(ray, out hit))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = ray.GetPoint(75);
            }

            Vector3 direction = targetPoint - shootPoint.position;

            Debug.DrawRay(shootPoint.position, direction, Color.red);

            GameObject bulletObject = PhotonNetwork.Instantiate("Prefabs/Projectile/" + bullet.name,
                shootPoint.position, Quaternion.identity);

            bulletObject.GetComponent<Rigidbody>().AddForce(direction.normalized * BulletSpeed, ForceMode.Impulse);
            bulletObject.GetComponent<Rigidbody>().AddForce(direction.normalized * BulletSpeed, ForceMode.Impulse);

            Invoke("Weapon_Shoot_Reset", 0.25f);
        }
    }

    void Weapon_Shoot_Reset()
    {
        canShoot = true;
    }


    [PunRPC]
    public void Player_Heal(float heal)
    {
        health = health + heal > maxHealth ? maxHealth : health + heal;
    }
    
    [PunRPC]
    public void Player_Damage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            health = 0;
            
            if (photonView.IsMine)
            {
                photonView.RPC(nameof(Player_Death), RpcTarget.All);
            }
        }
    }
    
    [PunRPC]
    void Player_Death()
    {
        Debug.Log(photonView.Owner.NickName + " died!");
        if (photonView.IsMine)
        {
            PhotonNetwork.Instantiate("Prefabs/Player/Player_Corpse", gameObject.transform.position,
                Quaternion.identity);
                transform.position += new Vector3(0, 5, 0);
                photonView.RPC(nameof(Player_Heal), RpcTarget.All, 99999f);
        }
    }
}