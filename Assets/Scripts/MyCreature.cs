using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class MyCreature : PrimerObject
{
    public ItemStash itemStash;
    public float [] productionRates;
    public int [] daysSinceLastConsumed;
    public Vector3 homePos;
    public float reputation = 0;

    private static float maxReputation = 5;
    private static float minReputation = 0;
    private MySceneDirector mySceneDirector;

    public ActionChoice currentAction = ActionChoice.NULL;
    public MyCreature currentlySeeking = null;
    public Item itemNeeded;

    public float [] opinions;

    public Queue<Debt> activeDebts = new Queue<Debt>();
    public List<Debt> pastDebts = new List<Debt>();
    public List<Debt> activeCredits = new List<Debt>();
    public List<Debt> pastCredits = new List<Debt>();

    private Material material;
    private static float INTERACTION_RADIUS = 0.7f;
    private MyCreature [] neighboursInOrder;

    // Start is called before the first frame update
    void Start() {
        mySceneDirector = GameObject.Find("Scene Director").GetComponent<MySceneDirector>();
        itemStash = new ItemStash();
        initialiseProductionRates();
        initialiseDaysSinceLastConsumed();
        initialiseNeighbours();
        
        // set material object
        material = this.gameObject.GetComponentInChildren<MeshRenderer>().materials[0];
        RefreshColor(mySceneDirector.lowReputationColor, mySceneDirector.highReputationColor);
    }

    private void initialiseNeighbours() {
        int len = mySceneDirector.nCreatures;
        GameObject [] neighbours = new GameObject [len];

        // copy array values from other script
        for (int i = 0; i < len; i++) {
            neighbours[i] = mySceneDirector.creatures[i].gameObject;
        }
        // sort by physical distance
        neighbours = neighbours.OrderBy(go => Vector3.Distance(go.transform.position, transform.position)).ToArray();
        
        // disgustingly typecast the array
        neighboursInOrder = new MyCreature [len];
        for (int i = 0; i < len; i++) {
            neighboursInOrder[i] = neighbours[i].GetComponent<MyCreature>();
        }
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
            productionRates[i] = UnityEngine.Random.Range(0f, 1f);

            // if production rate is too low just set it to 0
            if (productionRates[i] < 0.5) {
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

    public float Sigmoid(float x) {
        float a = 1.4f;
        float b = -3f;
        return 1/(1 + Mathf.Pow(2.718f, -a * (x + b)));
    }

    public void ProduceItems() {
        StartCoroutine("produceItems");
    }
    private IEnumerator produceItems() {
        for (int i = 0; i < itemStash.items.Length; i++) {
            if (UnityEngine.Random.Range(0f, 1f) < productionRates[i]) {
                // can produce up to three items of each type
                int amountProducing = (int)(3 * productionRates[i]);
                itemStash.items[i].quantity += amountProducing;

                // assumes index corresponds to correct gameobject
                PrimerObject itemObject = Instantiate(mySceneDirector.itemModels[i], this.homePos, Quaternion.identity).GetComponent<PrimerObject>();
                itemObject.MoveAndDestroy(new Vector3(homePos.x, homePos.y + 0.5f, homePos.z), 0, 0.5f);
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    public void DecideWhatToDo() {
        itemNeeded = calculateItemNeeded();

        // if the creature needs an item
        if (itemNeeded != null) {
            // if it can produce the item itself
            if (CanProduceItem(itemNeeded.itemName)) {
                currentAction = ActionChoice.PRODUCING;
                ProduceItems();
            }
        }
        else {
            currentAction = ActionChoice.GIFTING;
            GiveGift();
        }
    }

    public void GiveGift() {
        StartCoroutine(giveGift(pickGiftReceiver()));
    }

    // TODO finish this function
    private MyCreature pickGiftReceiver() {
        return neighboursInOrder[(int)UnityEngine.Random.Range(0, 5)];
    }

    public IEnumerator giveGift(MyCreature receiver) {
        TravelTowards(receiver.gameObject);
        yield return new WaitForSeconds(0.5f);

        // TODO transfer the gift
    }

    public bool CanProduceItem(ItemName itemName) {
        for (int i = 0; i < itemStash.items.Length; i++) {
            if (itemName == itemStash.items[i].itemName) {
                if (productionRates[i] > 0.5f)
                    return true;
            }
        }
        return false;
    }

    public void TravelTowards(GameObject target, float duration = 0.5f) {
        StartCoroutine(travelTowards(target, duration));
    }

    private IEnumerator travelTowards(GameObject target, float duration) {
        float stepSize = calculateStepSize();
        float startTime = Time.time;

        while (Time.time < startTime + duration)
        {   
            if (Vector3.Distance(target.transform.position, transform.position) > INTERACTION_RADIUS) {
                Vector3 direction = Vector3.Normalize(target.transform.position - transform.position);
                transform.position =  transform.position + direction * stepSize;
                
                if (target.transform.position - transform.position != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation (target.transform.position - transform.position);
            }
            yield return null;
        }
    }

    private float calculateStepSize() {
        float MAX_SIZE = 0.1f;
        float MIN_SIZE = 0.001f;
        return MAX_SIZE;
    }

    public Item calculateItemNeeded() {
        Item currItem = null;
        for (int i = 0; i < itemStash.items.Length; i++) {
            if (daysSinceLastConsumed[i] > itemStash.items[i].consumeRate && itemStash.items[i].quantity < 0) {
                if (currItem != null) {
                    if (itemStash.items[i].inherentValue > currItem.inherentValue) {
                        currItem = itemStash.items[i];
                    }
                } else {
                    currItem = itemStash.items[i];
                }
            }
        }
        if (currItem == null)
            return null;
        Debug.Log("Need " + currItem.itemName);
        return currItem;
    }

    public void ConsumeItems() {
        for (int i = 0; i < itemStash.items.Length; i++) {
            if (daysSinceLastConsumed[i] > itemStash.items[i].consumeRate) {
                if (itemStash.items[i].quantity > 0) {
                    itemStash.items[i].quantity -= 1;
                    daysSinceLastConsumed[i] = 0;
                }
            }
            daysSinceLastConsumed[i] += 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class Debt {
    public int date;
    public MyCreature debtor;
    public MyCreature creditor;
    public float value;
    public bool isRepayed;
}

public enum ActionChoice {
    NULL,
    PRODUCING,
    GIFTING,
    SEEKING
}
