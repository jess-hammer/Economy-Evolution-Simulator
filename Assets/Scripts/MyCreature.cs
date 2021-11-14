using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCreature : PrimerObject
{
    public ItemStash itemStash;
    public float [] productionRates;
    public int [] daysSinceLastConsumed;
    public ItemName currentlySeeking;
    public Vector3 homePos;
    public float reputation;

    private static float maxReputation = 5;
    private static float minReputation = 0;

    private Material material;

    // Start is called before the first frame update
    void Start()
    {
        itemStash = new ItemStash();
        initialiseProductionRates();
        initialiseDaysSinceLastConsumed();
        reputation = Random.Range(0, 5);
        
        // set material
        material = this.gameObject.GetComponentInChildren<MeshRenderer>().materials[0];
    }

    public void RefreshColor(Color col1, Color col2) {
        float amount = Mathf.InverseLerp(minReputation, maxReputation, reputation);
        Color newColor = Color.Lerp(col1, col2, amount);
        material.SetColor("_EmissionColor", newColor);
    }

    private void initialiseProductionRates() {
        int n = itemStash.items.Length;
        productionRates = new float [n];
        for (int i = 0; i < n; i++) {
            productionRates[i] = Random.Range(0f, 1f);

            // if production rate is too low just set it to 0
            if (productionRates[i] < 0.25) {
                productionRates[i] = 0;
            }
        }
    }

    private void initialiseDaysSinceLastConsumed() {
        int n = itemStash.items.Length;
        daysSinceLastConsumed = new int [n];
        for (int i = 0; i < n; i++) {
            daysSinceLastConsumed[i] = 0;
        }
    }

    public void TryProduceItems() {
        for (int i = 0; i < itemStash.items.Length; i++) {
            if (Random.Range(0f, 1f) < productionRates[i]) {
                itemStash.items[i].quantity += (int)(3 * productionRates[i]);
            }
        }
    }

    public void ConsumeItems() {
        for (int i = 0; i < itemStash.items.Length; i++) {
            if (daysSinceLastConsumed[i] >= itemStash.items[i].consumeRate) {
                if (itemStash.items[i].quantity >= 1) {
                    itemStash.items[i].quantity -= 1;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
