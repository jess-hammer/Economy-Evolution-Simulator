using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MyCreature : PrimerObject
{
    public ItemStash itemStash;
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

    public Vector3 meetingPlacePos;

    // Start is called before the first frame update
    void Start() {
        mySceneDirector = GameObject.Find("Scene Director").GetComponent<MySceneDirector>();
        itemStash = new ItemStash();
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

    public void ProduceItems(int nTimes, float duration) {
        StartCoroutine(produceItems(nTimes, duration));
    }
    private IEnumerator produceItems(int nTimes, float duration) {
        for (int i = 0; i < nTimes; i++) {
            int itemIndex = (int)Random.Range(0, itemStash.items.Length - 1);
            float randNum = Random.Range(0f, 1f);
            
            Item item = itemStash.items[itemIndex];
            if (randNum < item.productionChance) {
                // can produce up to three items of each type
                int amountProducing = (int)Random.Range(0, 3f);

                // update the quantity of the item
                item.quantity += amountProducing;

                // visualise production of item
                PrimerObject itemObject = Instantiate(mySceneDirector.itemModels[itemIndex], this.homePos, Quaternion.identity).GetComponent<PrimerObject>();
                itemObject.MoveAndDestroy(new Vector3(homePos.x, homePos.y + 0.5f, homePos.z), 0, duration/nTimes);
                yield return new WaitForSeconds(duration/nTimes);
            }
        }
    }
    // TODO actually run this function
    public void ExecuteBehaviour(float duration) {
        StartCoroutine(executeBehaviour(duration));
    }
    private IEnumerator executeBehaviour(float duration) {
        itemNeeded = calculateItemNeeded();

        currentAction = ActionChoice.PRODUCING;
        ProduceItems(2, duration/4);
        yield return new WaitForSeconds(duration/4);
        
        
        currentAction = ActionChoice.GIFTING;
        GoToRandomMeetingPlace(duration/4);
        yield return new WaitForSeconds(duration/4);

        // GiveGift(duration, pickGiftReceiver(), pickGiftItem())
        // yield return new WaitForSeconds(duration/4);

        GoHome(duration/4);
        yield return new WaitForSeconds(duration/4);

        ConsumeItems();
    }

    // TODO finish this function
    private MyCreature pickGiftReceiver() {
        return neighboursInOrder[(int)UnityEngine.Random.Range(0, 5)];
    }

    // TODO finish this function
    private int pickGiftItem() {
        int index = (int)UnityEngine.Random.Range(0, itemStash.items.Length);
        return index;
    }

    public void GiveGift(float duration, MyCreature receiver, int giftItemIndex) {
        // transfer the gift
        // TODO change quantity
        PrimerObject itemObject = Instantiate(mySceneDirector.itemModels[giftItemIndex], this.transform.position, Quaternion.identity).GetComponent<PrimerObject>();
        itemObject.MoveAndDestroy(receiver.transform.position, 0, duration);
    }

    public void GoHome(float duration) {
        WalkTo(homePos, duration: duration);
    }

    public void GoToRandomMeetingPlace(float duration) {
        Transform [] places = mySceneDirector.meetingPlaceParent.GetComponentsInChildren<Transform>();
        int randNum = (int)Random.Range(1, places.Length - 1); // not sure if this includes the parent
        meetingPlacePos = places[randNum].position;

        // Vector3 currPos = this.transform.position;
        Vector3 stopPos = new Vector3(meetingPlacePos.x, this.transform.position.y, meetingPlacePos.z);
        // stopPos = currPos + ((stopPos - currPos) * 0.9f);
        WalkTo(stopPos, duration: duration);
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

    // TODO fix this function
    private float calculateStepSize() {
        float MAX_SIZE = 0.1f;
        float MIN_SIZE = 0.001f;
        return MAX_SIZE;
    }

    public Item calculateItemNeeded() {
        Item currItem = null;
        for (int i = 0; i < itemStash.items.Length; i++) {
            if (daysSinceLastConsumed[i] > itemStash.items[i].consumeRate && itemStash.items[i].quantity <= 0) {
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
