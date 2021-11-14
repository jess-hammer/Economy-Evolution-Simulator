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
    public float happinessPercent; // Parameter to quantify the beauty, deliciousness or other nice qualites
    public float weightValue; // Parameter in kg
    public float sizeValue; // Parameter in cm

    public int consumeRate; // days between consumption

    static float necessityWeighting = 1f; // Parameter
    static float usefulnessWeighting = 0.7f; // Parameter
    static float happinessWeighting = 0.4f; // Parameter
    static int valueScale = 10; // Parameter to scale the value of items up into more appropriate range
    
    public Item(ItemName itemName, float necessity, float usefulness, float happiness, float weight, float size, int consumeRate) {
        this.itemName = itemName;
        necessityPercent = necessity;
        usefulnessPercent = usefulness;
        happinessPercent = happiness;
        weightValue = weight;
        sizeValue = size;
        this.consumeRate = consumeRate;

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
}

public class ItemStash {
    public Item [] items = new Item[] {
        new Item(ItemName.VEGETABLE, 1, 0, 0.2f, 0.5f, 20, 2),
        new Item(ItemName.GRAIN, 1, 0, 0.2f, 2, 20, 1),
        new Item(ItemName.MEAT, 1, 0, 0.7f, 2, 20, 3),
        new Item(ItemName.TRINKET, 0, 0, 0.8f, 0.1f, 5, (int)Random.Range(5f, 10f)),
        new Item(ItemName.PEBBLE, 0, 0, 0, 0.1f, 2, 0),
        new Item(ItemName.TOOL, 0, 1, 0.1f, 10, 20, (int)Random.Range(5f, 10f)),
        new Item(ItemName.FURNITURE, 0.2f, 0.4f, 0.2f, 30, 100, (int)Random.Range(10f, 20f)),
        new Item(ItemName.CLOTHING, 0.2f, 0.4f, 0.2f, 1, 50, (int)Random.Range(5f, 10f)),
        new Item(ItemName.TOILET_PAPER, 0.2f, 0.1f, 0f, 0.1f, 10, 6)
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
