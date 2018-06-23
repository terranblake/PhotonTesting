using UnityEngine;

public class NetworkActions : Photon.MonoBehaviour
{
    // TO SHOOT A PROJECTILE:   NetworkActions.Fire(ProjectilePrefab.name, pos, _head.transform.forward, 100f, 1f);
    [PunRPC]
    public void Fire(string prefabName, Vector3 pos, Vector3 dir, float force, float delay)
    {
        GameObject projectile = Instantiate(Resources.Load(prefabName) as GameObject, pos, Quaternion.Euler(dir));

        if (this.photonView.isMine)
            projectile.GetComponent<Renderer>().material.color = new Color(255f, 0f, 255f);

        projectile.GetComponent<Rigidbody>().AddForce(dir * force, ForceMode.Force);
        Destroy(projectile, delay);

        if (this.photonView.isMine)
            this.photonView.RPC("Fire", PhotonTargets.Others, pos, dir, delay);
    }

    private int itemCount = 0;

    // TO CREATE AN ITEM:       NetworkActions.Creation(cube, new Vector3(1, 1, 1), Quaternion.identity);
    [PunRPC]
    public void Creation(string toCreate, Vector3 position, Quaternion direction)
    {
        GameObject spawned = Instantiate(Resources.Load(toCreate) as GameObject, position, direction);

        if (spawned.GetComponent<Rigidbody>() == null)
            spawned.AddComponent<Rigidbody>();

        spawned.transform.name = string.Format("{0}{1}", toCreate, itemCount);
        itemCount += 1;

        if (this.photonView.isMine)
            this.photonView.RPC("Creation", PhotonTargets.OthersBuffered, toCreate, position, direction);
    }

    // TO DESTROY AN ITEM:      NetworkActions.Destruction(itemName);
    [PunRPC]
    public void Destruction(string toDestroy)
    {
        GameObject destroyMe = GameObject.Find(toDestroy);

        if (destroyMe != null)
            Destroy(destroyMe);

        if (this.photonView.isMine)
            this.photonView.RPC("Destruction", PhotonTargets.Others, toDestroy);
    }

    // TO DROP AN ITEM:         NetworkActions.OwnershipTransfer(null, _inventoryView.viewID, 0);
    // TO PICK UP AN ITEM:      NetworkActions.OwnershipTransfer(itemName, _inventoryView.viewID, -1);
    [PunRPC]
    public void OwnershipTransfer(string itemName, int parent, int inventoryPos)
    {
        GameObject item = GameObject.Find(itemName);
        GameObject parentGO = PhotonView.Find(parent).gameObject;

        if (item != null && parentGO != null)
        {
            Destroy(item.GetComponent<Rigidbody>());

            item.transform.parent = parentGO.transform;
            item.transform.localEulerAngles = Vector3.zero;
            item.transform.localPosition = Vector3.zero;
        }
        else if (inventoryPos != -1)
        {
            //  parentGO is the object that the item will be centered on
            Transform itemTransform = parentGO.transform.GetChild(inventoryPos);
            Rigidbody itemRigidbody = itemTransform.GetComponent<Rigidbody>();

            if (itemRigidbody == null)
                itemTransform.gameObject.AddComponent<Rigidbody>();

            itemTransform.parent = null;
        }
        else
        {
            Debug.Log("Item does not exist.");
        }

        if (this.photonView.isMine)
            this.photonView.RPC("OwnershipTransfer", PhotonTargets.OthersBuffered, itemName, parent, inventoryPos);
    }

    [PunRPC]
    public void RendererState(string itemName, int state)
    {
        GameObject item = GameObject.Find(itemName);

        if (item != null)
        {
            Renderer[] renderers = item.GetComponentsInChildren<Renderer>();

            if (renderers != null)
            {
                bool rendererState;

                if (state == -1)
                    rendererState = !renderers[0].enabled;
                else if (state == 0)
                    rendererState = false;
                else
                    rendererState = true;

                foreach (Renderer renders in renderers)
                {
                    if (renders.enabled != rendererState)
                        renders.enabled = rendererState;
                }
            }
        }

        if (this.photonView.isMine)
            this.photonView.RPC("RendererState", PhotonTargets.OthersBuffered, itemName, state);
    }

    [PunRPC]
    public void InventoryUpdate(int inventoryId, string selectedItem)
    {
        Transform inventory = PhotonView.Find(inventoryId).transform;

        for (int x = 0; x < inventory.childCount; x++) {
            GameObject item = inventory.GetChild(x).gameObject;
            Renderer[] renderers = item.GetComponentsInChildren<Renderer>();

            bool state = item.name == selectedItem ? true: false;

            foreach (Renderer renders in renderers) {
                renders.enabled = state;
            }
        }

        if (this.photonView.isMine)
            this.photonView.RPC("InventoryUpdate", PhotonTargets.OthersBuffered, inventoryId, selectedItem);
    }
}
