using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public ItemName itemName;
    public float inherentValue; // value derived from all the parameters
    public float perceivedValue; 
    public int quantity;

    // in range 0-1 and necessity > usefulness > happiness
    public float necessityPercent; // Parameter to quantify the necessity of the item
    public float usefulnessPercent; // Parameter to quantify the usefulness of the item
    public float happinessPercent; // Parameter to quantify the beauty, deliciousness, or usefulness or other nice qualites
    public float weightValue; // Parameter in kg
    public float productionChance;

    public int consumeRate; // days between consumption

    static float necessityWeighting = 1f; // Parameter
    static float usefulnessWeighting = 0.7f; // Parameter
    static float happinessWeighting = 0.4f; // Parameter
    static int valueScale = 10; // Parameter to scale the value of items up into more appropriate range
    
    public Item(ItemName itemName, float necessity, float usefulness, float happiness, float weight, int consumeRate, float productionChance) {
        this.itemName = itemName;
        necessityPercent = necessity;
        usefulnessPercent = usefulness;
        happinessPercent = happiness;
        weightValue = weight;
        this.consumeRate = consumeRate;
        this.productionChance = productionChance;

        quantity = 0;
        inherentValue = calculateInherentValue();
        perceivedValue = inherentValue;
    }

    private float calculateInherentValue() {
        float necessityValue = necessityPercent * valueScale * necessityWeighting;
        float usefulnessValue = usefulnessPercent * valueScale * usefulnessWeighting;
        float happinessValue = happinessPercent * valueScale * happinessWeighting;

        return necessityValue + usefulnessValue + happinessValue;
    }

    // TODO consider item quantity?
    ///<summary> Function to recalculate the perceived value based on whether the agent currently needs it or not </summary>
    public void CalculatePerceivedValue(int daysSinceLastConsumed, int qty) {
        // use days since last consumed to determine value
        int daysLeft = consumeRate - daysSinceLastConsumed;
        float interpolation = Mathf.Clamp(Mathf.InverseLerp(5f, -5f, daysLeft), 0, 1); // 1 = desperately need, 0 = eh still got time left
        
        if (qty > 0)
            perceivedValue += interpolation - 0.5f; // can go down
        else
            perceivedValue += interpolation; // only goes up

        // clamp to zero
        if (perceivedValue < 0) {
            perceivedValue = 0;
        }
    }
}

public class ItemStash {
    public Item [] items = new Item[] {
        new Item(ItemName.VEGETABLE, 1, 0, 0.2f, 0.5f, (int)Random.Range(3f, 6f), Random.Range(0.75f, 1f)),
        new Item(ItemName.GRAIN, 1, 0, 0.2f, 2, (int)Random.Range(3f, 6f), Random.Range(0.75f, 1f)),
        new Item(ItemName.MEAT, 1, 0, 0.7f, 2, (int)Random.Range(3f, 6f), Random.Range(0.75f, 1f)),
        new Item(ItemName.TRINKET, 0, 0, 0.8f, 0.5f, (int)Random.Range(7f, 15f), Random.Range(0.25f, 0.75f)),
        new Item(ItemName.PEBBLE, 0, 0, 0, 0.1f, (int)Random.Range(1000f, 2000f), 1),
        new Item(ItemName.TOOL, 0, 1, 0.1f, 10, (int)Random.Range(7f, 11f), Random.Range(0.25f, 0.6f)),
        new Item(ItemName.FURNITURE, 0.2f, 0.4f, 0.2f, 30, (int)Random.Range(10f, 16f), Random.Range(0.2f, 0.5f)),
        new Item(ItemName.CLOTHING, 0.2f, 0.4f, 0.2f, 1, (int)Random.Range(5f, 11f), Random.Range(0.25f, 0.75f)),
        new Item(ItemName.TOILET_PAPER, 0.2f, 0.1f, 0f, 0.1f, (int)Random.Range(5f, 8f), Random.Range(0.25f, 0.75f))
    };
}

public enum ItemName {
    VEGETABLE,
    GRAIN,
    MEAT,
    TRINKET,
    PEBBLE,
    TOOL,
    FURNITURE,
    CLOTHING,
    TOILET_PAPER
}
