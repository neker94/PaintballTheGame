using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public enum WeaponState
{
    Idle,
    Shooting,
    StartReload,
    Reloading,
    EndReload
}

[RequireComponent(typeof(PlayerInfo))]
[RequireComponent(typeof(PlayerMovement))]
public class WeaponScript : NetworkBehaviour {

    public GameObject weaponSocket;

    [SyncVar]
    public string weaponName;

    public PlayerAnimations animations;

    public LayerMask raycastMask;

    private PlayerInfo _playerInfo;
    //private PlayerMovement _playerMovement;

    private GameObject _camera;
    private GameObject _weapon;
    private WeaponStats _weaponInfo;

    private Vector3 _avgPosition;
    private bool _cameraRayHit;
    private Vector3 _cameraImpactPoint;
    private bool _weaponRayHit;
    private Vector3 _weaponImpactPoint;

    private Vector3 _shootDirection;

    private bool _canShoot = true;

    private WeaponState _state;

    private int _bulletsInClip;
    private int _totalBullets;

    /*-------------TIMERS-------------*/
    private float _fireRateTimer = 0.0f;

    private float _startReload = 0.5f;
    private float _startReloadTimer = 0.0f;
    private float _reloadTimer = 0.0f;
    private float _endReload = 0.5f;
    private float _endReloadTimer = 0.0f;

    // Use this for initialization
    public override void OnStartLocalPlayer()
    {
        _playerInfo = GetComponent<PlayerInfo>();
        _camera = _playerInfo.GameCamera;
        
        weaponName = ClientInfo.GetClientInfo().WeaponEquipped;
        CmdInitWeapon(weaponName);
    }

    //Sync local variables here
    public override void OnStartClient()
    {
        base.OnStartClient();


    }

    [Command]
    public void CmdInitWeapon(string weapon)
    {
        RpcInitWeapon(weapon);

        InitWeapon(weapon);
    }


    public void SetNetworkWeapon()
    {
        if (isLocalPlayer)
            return;

        SetWeapon(weaponName);

        _avgPosition = new Vector3(0, 0, 0);
        _cameraRayHit = false;
        _cameraImpactPoint = new Vector3(0, 0, 0);
        _weaponRayHit = false;
        _weaponImpactPoint = new Vector3(0, 0, 0);

        if (!_weaponInfo)
            return;

        _fireRateTimer = _weaponInfo.fireRate;
        _bulletsInClip = _weaponInfo.clipSize;
        _totalBullets = _weaponInfo.maxBullets;

        _startReloadTimer = 0.0f;
        _reloadTimer = 0.0f;
        _endReloadTimer = 0.0f;

        _state = WeaponState.Idle;
    }
    
    public void GetWeaponNet()
    {

        SetWeapon(weaponName);

        _avgPosition = new Vector3(0, 0, 0);
        _cameraRayHit = false;
        _cameraImpactPoint = new Vector3(0, 0, 0);
        _weaponRayHit = false;
        _weaponImpactPoint = new Vector3(0, 0, 0);

        if (!_weaponInfo)
            return;

        _fireRateTimer = _weaponInfo.fireRate;
        _bulletsInClip = _weaponInfo.clipSize;
        _totalBullets = _weaponInfo.maxBullets;

        _startReloadTimer = 0.0f;
        _reloadTimer = 0.0f;
        _endReloadTimer = 0.0f;

        _state = WeaponState.Idle;
    }



    public void InitWeapon(string weapon)
    {
        SetWeapon(weapon);

        _avgPosition = new Vector3(0, 0, 0);
        _cameraRayHit = false;
        _cameraImpactPoint = new Vector3(0, 0, 0);
        _weaponRayHit = false;
        _weaponImpactPoint = new Vector3(0, 0, 0);

        if (!_weaponInfo)
            return;

        RestartWeapon();
    }

    [ClientRpc]
    public void RpcInitWeapon(string weapon)
    {
        InitWeapon(weapon);
    } 
	
	// Update is called once per frame
	void Update ()
    {

        if(_weapon)
            animations.LookAt = _weapon.transform.position + (_weapon.transform.forward * 2.0f);

        if(isLocalPlayer)
        {
            if (!_weaponInfo)
                return;

            UpdateRotation();
            UpdateImpactPoints();

            UpdateState();            
        }
        else
        {
            if (!_weapon)
                GetWeaponNet();
            if (!_playerInfo)
                _playerInfo = GetComponent<PlayerInfo>();
        }
	}

    void OnDrawGizmos()
    {
        if (!_weaponInfo)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_avgPosition, 0.2f);

        if(_camera)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(MathUtils.GetPointInDistanceFromPos(_camera.transform.position, _camera.transform.forward.normalized, _weaponInfo.bestMinDistance), 0.2f);
            Gizmos.DrawWireSphere(MathUtils.GetPointInDistanceFromPos(_camera.transform.position, _camera.transform.forward.normalized, _weaponInfo.bestMaxDistance), 0.2f);
        }


        if (_cameraRayHit)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(_camera.transform.position, _cameraImpactPoint);
            Gizmos.DrawWireSphere(_cameraImpactPoint, 0.2f);
        }

        if (_weaponRayHit)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_weaponImpactPoint, 0.2f);
            Gizmos.DrawLine(weaponSocket.transform.position, _weaponImpactPoint);
        }
    }

    private void UpdateState()
    {
        bool canShoot = CanShoot();

        switch (_state)
        {
            case WeaponState.Idle:
                UpdateFireRate();

                if (canShoot && Input.GetButton("Fire1"))
                    _state = WeaponState.Shooting;

                if (Input.GetButtonDown("Fire2") && _bulletsInClip < _weaponInfo.clipSize && _totalBullets > 0)
                    _state = WeaponState.StartReload;

                break;

            case WeaponState.Shooting:
                UpdateFiring();

                if (!Input.GetButton("Fire1") || !canShoot)
                    _state = WeaponState.Idle;
                break;

            case WeaponState.StartReload:
                if(_weaponInfo)
                {
                    float animProgress = _startReloadTimer / _startReload;
                    animProgress = 1f - Mathf.Cos(animProgress * Mathf.PI * 0.5f); //Ease in function

                    _weaponInfo.clipSocket.transform.localRotation = Quaternion.Slerp(_weaponInfo.ClipStartQuat, _weaponInfo.reloadingClipPosition.localRotation, animProgress);
                }


                if ( _startReloadTimer >= _startReload)
                {
                    _state = WeaponState.Reloading;
                    _startReloadTimer = 0.0f;
                }
                else
                    _startReloadTimer += Time.deltaTime;

                break;

            case WeaponState.Reloading:

                if (_reloadTimer >= _weaponInfo.reloadTime)
                {
                    _reloadTimer = 0.0f;
                    _bulletsInClip++;
                    _totalBullets--;
                }
                else
                    _reloadTimer += Time.deltaTime;

                if (!Input.GetButton("Fire2") ||  _bulletsInClip >= _weaponInfo.clipSize || _totalBullets <= 0)
                    _state = WeaponState.EndReload;

                break;

            case WeaponState.EndReload:
                if (_weaponInfo)
                {
                    float animProgress = _endReloadTimer / _endReload;
                    animProgress = Mathf.Sin(animProgress * Mathf.PI * 0.5f); //Ease out function

                    _weaponInfo.clipSocket.transform.localRotation = Quaternion.Slerp(_weaponInfo.reloadingClipPosition.localRotation, _weaponInfo.ClipStartQuat, animProgress);
                }

                if (_endReloadTimer >= _endReload)
                {
                    _state = WeaponState.Idle;
                    _endReloadTimer = 0.0f;
                }
                else
                    _endReloadTimer += Time.deltaTime;
                break;

            default:
                break;
        }
    }

    private void UpdateRotation()
    {
        if (!_camera || !_weaponInfo)
            return;

        float avgDistance = (_weaponInfo.bestMinDistance + _weaponInfo.bestMaxDistance) / 2.0f;
        _avgPosition = MathUtils.GetPointInDistanceFromPos(_camera.transform.position, _camera.transform.forward, avgDistance);


        weaponSocket.transform.LookAt(_avgPosition);
    }

    private void UpdateImpactPoints()
    {

        if (!_camera || !_weaponInfo)
            return;

        Ray cameraRay = new Ray(_camera.transform.position, _camera.transform.forward);
        RaycastHit cameraHit = new RaycastHit();
        if(Physics.Raycast(cameraRay, out cameraHit, Mathf.Infinity, raycastMask))
        {
            _cameraRayHit = true;
            _cameraImpactPoint = cameraHit.point;
        }
        else
        {
            _cameraRayHit = false;
        }

        Ray weaponRay;
        RaycastHit weaponHit = new RaycastHit();
        float distanceToCameraImpact = Vector3.Distance(_cameraImpactPoint, _camera.transform.position);
        if (distanceToCameraImpact < _weaponInfo.bestMinDistance)
        {
            weaponRay = new Ray(
                weaponSocket.transform.position,
                MathUtils.GetPointInDistanceFromPos(_camera.transform.position, _camera.transform.forward, _weaponInfo.bestMinDistance) - weaponSocket.transform.position);
        }
        else if (distanceToCameraImpact > _weaponInfo.bestMaxDistance || !_cameraRayHit)
        {

            weaponRay = new Ray(
                weaponSocket.transform.position,
                MathUtils.GetPointInDistanceFromPos(_camera.transform.position, _camera.transform.forward, _weaponInfo.bestMaxDistance) - weaponSocket.transform.position);
        }
        else
            weaponRay = new Ray(weaponSocket.transform.position, cameraHit.point - weaponSocket.transform.position);
        

        if (Physics.Raycast(weaponRay, out weaponHit, Mathf.Infinity, raycastMask))
        {
            _weaponRayHit = true;
            _weaponImpactPoint = weaponHit.point;
        }
        else
        {
            _weaponRayHit = false;
            _weaponImpactPoint = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
        }

    }
    
    private bool CanShoot()
    {
        if (!_canShoot)
            return _canShoot;
        

        Ray ray = new Ray(transform.position, _weaponInfo.barrelMouth.transform.position - transform.position);
        RaycastHit hit = new RaycastHit();
        if(_weaponRayHit && _weapon)
        {

            if (Vector3.Distance(_weapon.transform.position, _weaponImpactPoint) < Vector3.Distance(_weapon.transform.position, _weaponInfo.barrelMouth.transform.position))
                return false;
            else
                return true;
        }
        else
            return false; 
        /*
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, raycastMask))
        {
            if (Vector3.Distance(transform.position, hit.point) < Vector3.Distance(transform.position, _weaponInfo.barrelMouth.transform.position))
                return false;
            else
                return true;
        }
        else
            return true;*/
    }
    
    private void UpdateFiring()
    {
        if (_fireRateTimer >= _weaponInfo.fireRate && _bulletsInClip > 0)
        {
            _bulletsInClip--;

            if(_weaponRayHit)
            {
                Fire(_weaponInfo.barrelMouth.transform.position, _weaponImpactPoint - _weaponInfo.barrelMouth.transform.position);
            }
            else
            {
                Fire(_weaponInfo.barrelMouth.transform.position,  ((_camera.transform.position + _camera.transform.forward * _weaponInfo.bestMaxDistance)- _weaponInfo.barrelMouth.transform.position).normalized);
            }

            _fireRateTimer -= Time.deltaTime;
        }
        else if (_fireRateTimer < _weaponInfo.fireRate)
        {
            if (_fireRateTimer <= 0.0f)
                _fireRateTimer = _weaponInfo.fireRate;
            else
                _fireRateTimer -= Time.deltaTime;
        }
    }

    private void UpdateFireRate()
    {
        if (!_weaponInfo)
            return;

        if (_fireRateTimer <= 0.0f)
        {
            _fireRateTimer = _weaponInfo.fireRate;
        }
        else if (_fireRateTimer < _weaponInfo.fireRate)
            _fireRateTimer -= Time.deltaTime;
    }

    public void Fire(Vector3 position, Vector3 direction)
    {
        if (!_canShoot)
            return;

        GameObject bulletObject = Instantiate(_weaponInfo.bulletObject, position, _weaponInfo.barrelMouth.transform.rotation) as GameObject;
        if (!bulletObject)
            return;

        BulletMovement bulletMovement = bulletObject.GetComponent<BulletMovement>();
        if (!bulletMovement)
            return;

        bulletMovement.InitBullet(_playerInfo, direction);

        CmdFire(position, direction);
    }

    [Command]
    public void CmdFire(Vector3 position, Vector3 direction)
    {   
        RpcFire(position, direction);
    }

    [ClientRpc]
    public void RpcFire(Vector3 position, Vector3 direction)
    {
        if (isLocalPlayer)
            return;
        
        GameObject bulletObject = Instantiate(_weaponInfo.bulletObject, position, _weaponInfo.barrelMouth.transform.rotation) as GameObject;
        if (!bulletObject)
            return;

        BulletMovement bulletMovement = bulletObject.GetComponent<BulletMovement>();
        if (!bulletMovement)
            return;

        bulletMovement.InitBullet(_playerInfo, direction);

    }

    public void SetWeapon(string weaponName)
    {
        this.weaponName = weaponName;
        InstantiateWeapon();
    }

    private void InstantiateWeapon()
    {
        if (_weapon)
            Destroy(_weapon);

        GameObject newWeapon = Resources.Load("Weapons/" + weaponName) as GameObject;

        if(!newWeapon)
        {
            Debug.Log("Couldn't find weapon");
            return;
        }

        _weapon = Instantiate(newWeapon, weaponSocket.transform.position, weaponSocket.transform.rotation) as GameObject;

        _weapon.transform.parent = weaponSocket.transform;
        _weaponInfo = _weapon.GetComponent<WeaponStats>();
        if(_weaponInfo)
        {
            animations.SetHandsIKSockets(_weaponInfo.leftHand, _weaponInfo.rightHand);
        }
        
    }

    public void RestartWeapon()
    {

        if (!_weaponInfo)
            return;

        _fireRateTimer = _weaponInfo.fireRate;
        _bulletsInClip = _weaponInfo.clipSize;
        _totalBullets = _weaponInfo.maxBullets;

        _startReloadTimer = 0.0f;
        _reloadTimer = 0.0f;
        _endReloadTimer = 0.0f;

        _state = WeaponState.Idle;
    }
    
    public int Ammo
    {
        get { return _bulletsInClip; }
    }

    public int MaxAmmo
    {
        get { return _totalBullets; }
    }
    
    public Vector3 BulletImpactPredicted
    {
        get { return _weaponImpactPoint; }
    }

    public bool CanShootWeapon
    {
        get { return _canShoot; }
        set { _canShoot = value; }
    }

}
