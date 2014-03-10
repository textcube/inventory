using UnityEngine;
using System.Collections;

/// <summary>
/// Restart Button
/// </summary>
public class RestartButton : MonoBehaviour {
	// restart scene on click
	void OnClick(){
		Application.LoadLevel(Application.loadedLevel);
	}
}
