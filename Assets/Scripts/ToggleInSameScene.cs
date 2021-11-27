using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleInSameScene : MonoBehaviour
{
	public GameObject [] menuPageObjects;
	public GameObject [] informationPageObjects;
	public GameObject [] parameterPageObjects;

	public Parameters parameters;

	private GameObject [] currPageObjects;

	void Start () {
		currPageObjects = menuPageObjects;
	}

	public void GoToMenuPage() {
		setArrayActive(currPageObjects, false);
		setArrayActive(menuPageObjects, true);
		currPageObjects = menuPageObjects;
	}
	public void GoToInformationPage() {
		setArrayActive(currPageObjects, false);
		setArrayActive(informationPageObjects, true);
		currPageObjects = informationPageObjects;
	}
	public void GoToParameterPage() {
		setArrayActive(currPageObjects, false);
		setArrayActive(parameterPageObjects, true);
		currPageObjects = parameterPageObjects;
	}

	private void setArrayActive(GameObject [] array, bool active)
	{
		for (int i = 0; i < array.Length; i++) {
			array [i].SetActive (active);
		}
	}
}
