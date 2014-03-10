using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Grid Item.
/// </summary>
public class GameItem : MonoBehaviour {
	// delete item on click.
	void OnClick(){
		transform.parent.GetComponent<GridControl>().DeleteItem(this);
	}
}
