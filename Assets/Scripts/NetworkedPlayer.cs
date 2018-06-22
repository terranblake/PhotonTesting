using UnityEngine;

public class NetworkedPlayer : Photon.MonoBehaviour, IPunObservable
{
    public static GameObject LocalPlayerInstance;
    public NetworkActions networkActions;
    public GameObject BulletPrefab;

    private GameObject _mainCamera;
    private GameObject _player;
    private GameObject _head;
    private Vector3 _bodyPosition = Vector3.zero; // We lerp towards this
    private Quaternion _bodyRotation = Quaternion.identity; // We lerp towards this

    private Vector3 _headPosition = Vector3.zero;
    private Quaternion _headRotation = Quaternion.identity;
    private PhotonView _view;
    private Rigidbody _parentRb;
    private Vector3 _remoteVelocity;
    private float _lag;
    private PhotonView _inventoryView;
    private int _inventoryIndex;

    void Awake()
    {
        _inventoryView = transform.Find("Inventory").GetComponent<PhotonView>();

        if (photonView.isMine)
        {
            LocalPlayerInstance = gameObject;
        }

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        _player = GameObject.Find("Player");
        _head = GameObject.Find("Head");
        _mainCamera = GameObject.Find("MainCamera");

        transform.parent = _player.transform;
        _parentRb = transform.parent.GetComponent<Rigidbody>();

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        _head.transform.position = _mainCamera.transform.position;
        _head.transform.rotation = _mainCamera.transform.rotation;
    }

    void Update()
    {
        if (!photonView.isMine)
        {
            // Update other player's position and rotation to match last grabbed value
            transform.position = Vector3.Lerp(transform.position, this._bodyPosition, Time.deltaTime * 10);
            transform.rotation = Quaternion.Lerp(transform.rotation, this._bodyRotation, Time.deltaTime * 10);
        }
        else
        {
            // If this is our photonView, then lock the _head to the camera rotation
            //  and position without making the _head a child of the camera
            _head.transform.localRotation = _mainCamera.transform.localRotation;
            _head.transform.position = _mainCamera.transform.position;

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            ProcessInputs();
        }
    }

    void ProcessInputs()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Vector3 pos = _head.transform.position;

            Debug.Log("Firing Projectile");
            networkActions.Fire(BulletPrefab.name, pos, _head.transform.forward, 100f, 1f);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            RaycastHit hit;
            Physics.Raycast(_head.transform.position, _head.transform.forward, out hit, 100f);
            {
                if (hit.transform != null)
                {
                    string itemName = hit.transform.name;

                    if (_inventoryView != null && hit.transform.gameObject.layer == LayerMask.NameToLayer("Interactable"))
                    {
                        Debug.Log("Acquiring Item", this);
                        networkActions.OwnershipTransfer(itemName, _inventoryView.viewID, -1);
                    }
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            if (_inventoryView != null && _inventoryView.transform.childCount > 0)
            {
                Transform item = _inventoryView.transform.GetChild(0);

                Debug.Log("Dropping Item", this);
                networkActions.OwnershipTransfer(null, _inventoryView.viewID, 0);
            }
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            string cube = "Gun";

            Debug.Log("Creating Item", this);
            networkActions.Creation(cube, new Vector3(1, 1, 1), Quaternion.identity);
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            RaycastHit hit;
            Physics.Raycast(_head.transform.position, _head.transform.forward, out hit, 100f);
            {
                if (hit.transform != null)
                {
                    string itemName = hit.transform.name;

                    if (hit.transform.parent == null && hit.transform.gameObject.layer == LayerMask.NameToLayer("Interactable"))
                    {
                        Debug.Log("Destroying Item", this);
                        networkActions.Destruction(itemName);
                    }
                }
            }
        }
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // We own this player: send the others our data
        if (stream.isWriting)
        {
            // Send our position and rotation
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        // Network player, receive data
        else
        {
            // Lag compensation
            //float _lag = Mathf.Abs((float)(PhotonNetwork.time - info.timestamp));

            // Receive other players position and rotation
            this._bodyPosition = (Vector3)stream.ReceiveNext();
            this._bodyRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}