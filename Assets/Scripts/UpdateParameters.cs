using System;
using System.Collections;
using System.Collections.Generic;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using UnityEngine.UI;

public class UpdateParameters : MonoBehaviour {
	public ParameterName nameToUpdate;
	private Parameters parameters;

	private void Start ()
	{
		parameters = GameObject.FindWithTag("Parameters").GetComponent<Parameters>();
		if (this.TryGetComponent<Slider> (out Slider slider)) {
			slider.value = parameters.getValueFloat (nameToUpdate);
		} else if (this.TryGetComponent<Toggle> (out Toggle toggle)) {
			toggle.isOn = parameters.getValueBool (nameToUpdate);
		} else {
			Debug.Log (this.name + "is not a slider lol");
		}
	}

	public void UpdatePref(float value)
	{
		parameters.updateValue (nameToUpdate, value);
	}

	public void UpdatePref(bool value)
	{
		parameters.updateValue (nameToUpdate, value);
	}
}
