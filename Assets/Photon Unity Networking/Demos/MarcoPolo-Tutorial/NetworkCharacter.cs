using UnityEngine;

public class NetworkCharacter : Photon.MonoBehaviour, IPunObservable
{
    private GameObject _mainCamera;
    private GameObject _player;
    private GameObject _head;
    private Vector3 _bodyPosition = Vector3.zero; // We lerp towards this
    private Quaternion _bodyRotation = Quaternion.identity; // We lerp towards this

    private Vector3 _headPosition = Vector3.zero;
    private Quaternion _headRotation = Quaternion.identity;
    private PhotonView _view;

    void Start()
    {
        _player = GameObject.Find("Player");
        _head = GameObject.Find("Head");
        _mainCamera = GameObject.Find("MainCamera");

        _view = GetComponent<PhotonView>();

        //transform.parent = _player.transform;

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

            // Same for the player's _head
            _head.transform.rotation = Quaternion.Lerp(_head.transform.rotation, this._headRotation, Time.deltaTime * 10);
        }
        else {
            // If this is our photonView, then lock the _head to the camera rotation 
            //  and position without making the _head a child of the camera
            _head.transform.localRotation = _mainCamera.transform.localRotation;
            _head.transform.position = _mainCamera.transform.position;
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

            // Send our _head's rotation
            stream.SendNext(_head.transform.localRotation);
        }
        // Network player, receive data
        else
        {
            // Receive other players position and rotation
            this._bodyPosition = (Vector3)stream.ReceiveNext();
            this._bodyRotation = (Quaternion)stream.ReceiveNext();

            // Receive other player's head rotation
            this._headRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}