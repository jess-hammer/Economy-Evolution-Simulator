using System;
using System.Collections;
using System.Collections.Generic;
using Michsky.UI.ModernUIPack;
using UnityEngine;


public class Parameters : MonoBehaviour
{
	public int nAgents = 20;
    public int nDays = 100; // number of days in the simulation
    public int itemProductionAmount = 2;
    public float timestepDuration = 1f;

	private static Parameters self;

	void Awake() {
		if (self == null) {
			self = this;
		} else {
			Destroy(this.gameObject);
		}
	}
	public bool isValid()
	{
		if (nDays <= 2) {
			return false;
		}
		if (nAgents < 4) {
			return false;
		}
		if (itemProductionAmount < 0) {
			return false;
		}
		if (timestepDuration < 0.25f) {
			return false;
		}
		return true;
	}

	public void updateValue (ParameterName parameter, float value)
	{
		switch (parameter) {
		case ParameterName.N_AGENTS:
			nAgents = (int)value;
			break;
		case ParameterName.N_DAYS:
			nDays = (int)value;
			break;
		case ParameterName.ITEM_PRODUCTION_AMOUNT:
			itemProductionAmount = (int)value;
			break;
		case ParameterName.TIMESTEP_DURATION:
			timestepDuration = value;
			break;
		default:
			Debug.Log ("Update value failed");
			break;
		}
	}

	public float getValue (ParameterName parameter)
	{
		switch (parameter) {
		case ParameterName.N_AGENTS:
			return nAgents;
		case ParameterName.N_DAYS:
			return nDays;
		case ParameterName.ITEM_PRODUCTION_AMOUNT:
			return itemProductionAmount;
		case ParameterName.TIMESTEP_DURATION:
			return timestepDuration;
		default:
			Debug.Log ("Get value failed");
			break;
		}
		return -1;
	}
}

public enum ParameterName {
	N_AGENTS,
    N_DAYS,
    ITEM_PRODUCTION_AMOUNT,
    TIMESTEP_DURATION
}
