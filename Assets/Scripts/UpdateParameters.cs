﻿using System;
using System.Collections;
using System.Collections.Generic;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using UnityEngine.UI;

public class UpdateParameters : MonoBehaviour {
	public ParameterName nameToUpdate;
	public Parameters parameters;

	private void Start ()
	{
		if (this.TryGetComponent<Slider> (out Slider slider)) {
			slider.value = parameters.getValue (nameToUpdate);
		} else {
			Debug.Log (this.name + "is not a slider lol");
		}
	}

	public void UpdatePref(float value)
	{
		parameters.updateValue (nameToUpdate, value);
	}
}
