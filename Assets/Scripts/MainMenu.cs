using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	public void OnCreateServerButtonPressed()
	{
		SceneManager.LoadScene("ServerMenu");
	}

	public void OnJoinServerButtonPressed()
	{
		print("Join");
	}
}
