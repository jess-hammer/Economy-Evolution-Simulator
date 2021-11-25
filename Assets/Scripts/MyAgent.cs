using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyAgent : PrimerObject
{
    public ItemStash itemStash;
    public int [] daysSinceLastConsumed;
    public Vector3 homePos;
    public float reputation = 0;

    private static float maxReputation = 1;
    private static float minReputation = 0;
    private MySceneDirector mySceneDirector;
    private Material material;

    public Item itemNeeded;
    public float [] opinions;
    public int selfIndex;

    public Vector3 meetingPlacePos;

    // Start is called before the first frame update
    void Start() {
        mySceneDirector = GameObject.Find("Scene Director").GetComponent<MySceneDirector>();
        itemStash = new ItemStash();
        initialiseDaysSinceLastConsumed();
        initialiseOpinionArray();
        findSelfIndex();
        
        // set material object
        material = this.gameObject.GetComponentInChildren<MeshRenderer>().materials[0];
        RefreshColor(mySceneDirector.lowReputationColor, mySceneDirector.highReputationColor);
        
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

    private void findSelfIndex() {
        for (int i = 0; i < mySceneDirector.nAgents; i++) {
            if (mySceneDirector.agents[i] == this) {
                selfIndex = i;
                return;
            }
        }
    }

    private void initialiseOpinionArray() {
        opinions = new float [mySceneDirector.nAgents];
        for (int i = 0; i < opinions.Length; i++) {
            opinions[i] = 0;
        }
    }

    public float Sigmoid(float x) {
        float a = 0.5f;
        float b = -4.2f;
        return 1/(1 + Mathf.Pow(2.718f, -a * (x + b)));
    }

    public void ProduceItems(int nTimes, float duration) {
        StartCoroutine(produceItems(nTimes, duration));
    }

    private IEnumerator produceItems(int nTimes, float duration) {
        for (int i = 0; i < nTimes; i++) {
            int itemIndex = (int)Random.Range(0, itemStash.items.Length); // don't need -1 cuz casting to int rounds down
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
    
    public void ExecuteBehaviour(float duration) {
        StartCoroutine(executeBehaviour(duration));
    }

    private IEnumerator executeBehaviour(float duration) {
        ProduceItems(2, duration/4);
        yield return new WaitForSeconds(duration/4);
        
        GoToRandomMeetingPlace(duration/4);
        yield return new WaitForSeconds(duration/4);

        GiveGifts(duration/4);
        yield return new WaitForSeconds(duration/4);

        GoHome(duration/4);
        yield return new WaitForSeconds(duration/4);

        ConsumeItems();
        UpdatePerceivedValues();
        UpdateReputation();
        RefreshColor(mySceneDirector.lowReputationColor, mySceneDirector.highReputationColor);
        // if (this.selfIndex == 0) {
        //     Debug.Log(this.ToString());
        // }
    }
    public override string ToString() {
        string str = "";
        str += "Average Reputation: " + reputation;
        str += "\nOpinion Array: ";
        str += opinions[0];
        for (int i = 1; i < opinions.Length; i++) {
            str += ", " + opinions[i];
        }
        for (int i = 0; i < itemStash.items.Length; i++) {
            str += "\n" + itemStash.items[i].itemName + " :";
            str += "\n    PerceivedValue: " + itemStash.items[i].perceivedValue;
            str += "\n    Quantity: " + itemStash.items[i].quantity;
            str += "\n    Days left until needed: " + (itemStash.items[i].consumeRate - daysSinceLastConsumed[i]).ToString();
        }
        return str;
    }

    public void GiveGifts(float duration) {
        // loop through all the Agents
        for (int i = 0; i < mySceneDirector.nAgents; i++) {
            // if the agents are at the same meeting place and agent is not itself
            if (mySceneDirector.agents[i].meetingPlacePos == meetingPlacePos 
            && mySceneDirector.agents[i].selfIndex != selfIndex) {
                // randomly pick gift
                int giftItemIndex = pickGiftItem();
                if (giftItemIndex >= 0) {
                    float interactionStatus = HaveInteraction(this, mySceneDirector.agents[i]);
                    attemptIncreaseValue(giftItemIndex, mySceneDirector.agents[i], interactionStatus);
                    // if the interaction was good, give a gift
                    if (interactionStatus >= 0.5) {
                        // give the gift
                        GiveGift(duration, mySceneDirector.agents[i], giftItemIndex);
                    }
                }
            }
        }
    }

    public void UpdatePerceivedValues() {
        for (int i = 0; i < itemStash.items.Length; i++) {
            itemStash.items[i].CalculatePerceivedValue(daysSinceLastConsumed[i], itemStash.items[i].quantity);
        }
    }

    // TODO pick gifts based on actual logic, not randomness?
    // TODO consider weight and distance?
    private int pickGiftItem() {
        // initialise array
        int [] indexArray = new int [itemStash.items.Length];
        for (int i = 0; i < indexArray.Length; i++) {
            indexArray[i] = i;
        }
        // shuffle array in order to randomly pick an item each time
        Shuffle(indexArray);

        // pick item if have enough
        for (int i = 0; i < indexArray.Length; i++) {
            if (itemStash.items[indexArray[i]].quantity > 1) {
                return indexArray[i];
            }
        }
        return -1;
    }
    
    void Shuffle(int[] array) {
        int p = array.Length;
        for (int n = p - 1; n > 0; n--) {
            int r = Random.Range(0, n);
            int t = array[r];
            array[r] = array[n];
            array[n] = t;
        }
    }

    public void GiveGift(float duration, MyAgent receiver, int giftItemIndex) {
        // transfer the gift
        PrimerObject itemObject = Instantiate(mySceneDirector.itemModels[giftItemIndex], this.transform.position, Quaternion.identity).GetComponent<PrimerObject>();
        itemObject.MoveAndDestroy(receiver.transform.position, 0, duration);

        // pick random quantity
        int giftQuantity = Random.Range(1, itemStash.items[giftItemIndex].quantity);

        // remove from this agents stash
        itemStash.items[giftItemIndex].quantity -= giftQuantity;
        if (itemStash.items[giftItemIndex].quantity < 0)
            Debug.Log("warning: item quantity is negative");

        // add to other agents stash
        receiver.ReceiveGift(this, giftItemIndex, giftQuantity);
    }

    ///<summary> Function that returns a float between 0 and 1 depending on agent's opinions of each other. 
    /// 0.5 indicates interaction went neutral, 1 indicates it went well, 0 indicates it went badly. </summary>
    public float HaveInteraction(MyAgent agentGiving, MyAgent agentReceiving) {
        float randNum = Random.Range(0f, 1f);
        float combinedReputation = Sigmoid(agentGiving.opinions[agentReceiving.selfIndex]) + 
        Sigmoid(agentReceiving.opinions[agentGiving.selfIndex]);
        combinedReputation = combinedReputation / 2;
        combinedReputation = combinedReputation * 0.5f + 0.4f; // make in range 0.5 - 1
        float interactionValue = distributeNumberAround(randNum, combinedReputation);

        // adjust agents opinion of eachother
        agentGiving.opinions[agentReceiving.selfIndex] += interactionValue * 2 - 1;
        agentReceiving.opinions[agentGiving.selfIndex] += interactionValue * 2 - 1;
        return interactionValue;
    }

    private void attemptIncreaseValue(int itemIndex, MyAgent receiver, float chanceValue) {
        if (chanceValue > 0.6) {
            receiver.itemStash.items[itemIndex].perceivedValue += 1;
        }
        if (chanceValue < 0.4) {
            receiver.itemStash.items[itemIndex].perceivedValue -= 1;
        }
    }

    ///<summary> Function to generate a number (between 0-1) that is biased toward the bias parameter (between 0-1). 
    /// Basically just a cubic function where the bias parameter determines the height of the graph. </summary>
    public float distributeNumberAround(float x, float biasToward) {
        float widthParameter = 1.8f;
        float positionParameter = 0.9f;
        int powerParameter = 5;
        return Mathf.Clamp(Mathf.Pow(x * widthParameter - positionParameter, powerParameter) + biasToward, 0, 1);
    }

    public void ReceiveGift(MyAgent giver, int giftItemIndex, int giftQuantity) {
        // add to stash
        itemStash.items[giftItemIndex].quantity += giftQuantity;

        // calculate perceived value of gift
        float giftValue = itemStash.items[giftItemIndex].perceivedValue * giftQuantity;

        // update opinion depending on perceived value of gift
        opinions[giver.selfIndex] += Mathf.Clamp(Mathf.Lerp(giftValue, 5, 200), -3, 3);
    }

    public void UpdateReputation() {
        float sum = 0;
        for (int i = 0; i < mySceneDirector.nAgents; i++) {
            if (i != selfIndex)
                sum += Sigmoid(mySceneDirector.agents[i].opinions[selfIndex]);
        }
        reputation = sum / (mySceneDirector.nAgents - 1);
    }

    public void GoHome(float duration) {
        WalkTo(homePos, duration: duration);
    }

    public void GoToRandomMeetingPlace(float duration) {
        Transform [] places = mySceneDirector.meetingPlaceParent.GetComponentsInChildren<Transform>();
        int randNum = (int)Random.Range(0, places.Length - 1);
        meetingPlacePos = places[randNum].position;

        Vector3 currPos = this.transform.position;
        Vector3 stopPos = new Vector3(meetingPlacePos.x, this.transform.position.y, meetingPlacePos.z);
        stopPos = currPos + ((stopPos - currPos) * 0.9f);
        WalkTo(stopPos, duration: duration);
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

