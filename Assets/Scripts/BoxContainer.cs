using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

/// An IContainer that can hold an unlimited number of PickUpObjects.
public class BoxContainer : NetworkBehaviour, IContainer
{
	public int Count
	{
		get
		{
			return transform.childCount;
		}
	}

	public int Capacity
	{
		get
		{
			return int.MaxValue;
		}
	}

	private PickUpObject GetChild(Transform parent)
	{
		if (Count < 1)
		{
			Debug.Log("BoxContainer empty.");
			return null;
		}

		GameObject child = transform.GetChild(0).gameObject;
		child.GetComponent<SpriteRenderer>().enabled = true;
		PickUpObject puo = child.GetComponent<PickUpObject>();
		puo.UpdateParent(parent, true);
		return puo;
	}

	public PickUpObject Get(Transform parent, NetworkRequest.Result handler)
	{
		if (isServer && isClient) {
			PickUpObject child = GetChild(parent);
			handler(child != null);
			return child;
		} else if (isServer) {
			PickUpObject child = GetChild(parent);
			handler(child != null);
			return child;
		} else if (isClient) {
			// TODO undo?
			PickUpObject child = GetChild(parent);
			NetworkInstanceId player = PlayerNumber.GetLocalPlayerGameObject().GetComponent<NetworkIdentity>().netId;
			NetworkRequestService.Instance().RequestGet(player, netId, handler);
			return child;
		} else {
			Debug.LogError("BoxContainer Get(...) called with invalid state.");
			throw new System.Exception();
		}
	}

	private void PutChild(PickUpObject obj)
	{
		if (Count >= Capacity)
		{
			Debug.Log("Slot not empty.");
			throw new System.MemberAccessException();
		}
		obj.UpdateParent(transform, false);
	}

	public void Put(PickUpObject obj, NetworkRequest.Result handler)
	{
		if (isServer && isClient) {
			PutChild(obj);
			handler(true);
		} else if (isServer) {
			PutChild(obj);
			handler(true);
		} else if (isClient) {
			PutChild(obj);
			NetworkInstanceId player = PlayerNumber.GetLocalPlayerGameObject().GetComponent<NetworkIdentity>().netId;
			NetworkInstanceId objNetId = obj.GetComponent<NetworkIdentity>().netId;
			NetworkRequestService.Instance().RequestPut(player, netId, objNetId, handler);
			throw new System.NotImplementedException();
		} else {
			Debug.LogError("BoxContainer Put(...) called with invalid state.");
			throw new System.Exception();
		}
	}

	void Update() {
		for(int i = 0; i < transform.childCount; ++i) {
			GameObject obj = transform.GetChild(i).gameObject;
			// special case for Cursor
			if (obj.tag != "Cursor")
			{
				obj.GetComponent<SpriteRenderer>().enabled = false;
			}
		}
	}
}
