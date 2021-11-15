using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChartUtil;

public class MySceneDirector : Director
{
    [Space]
    public int dayNumber = 0;
    public int nCreatures;
    
    [Space]
    public MyCreature creaturePrefab;
    public GameObject creatureParent;
    public Transform cameraTransform;
    public GameObject [] housePrefabs;
    public GameObject [] itemModels; // index correspond to itemName enum number
    public Color highReputationColor;
    public Color lowReputationColor;
    public ChartData chartData;

    List<MyCreature> creatures = null;
    private float RADIUS = 4f;
    private float HEIGHT = 0.3f;
    private float HOUSE_HEIGHT = 0f;
    private float HOUSE_DIST = 1f;
    private int N_DAYS = 10; // number of days in the simulation

    protected override void Awake() {
        base.Awake();
        spawnBlobs(nCreatures);
        spawnHouses();
        camRig.GoToStandardPositions();

        // set object scale to zero if they're going to scale in
        for (int i = 0; i < creatures.Count; i++) {
            creatures[i].transform.localScale = Vector3.zero;
        }
        chartData.gameObject.transform.localScale = Vector3.zero;
    }

    protected void spawnBlobs(int n) {
        creatures = new List<MyCreature>();
        for (int i = 0; i < n; i++) {
            Vector3 homePos = getHomePos(i);
            MyCreature newCreature = Instantiate(creaturePrefab, homePos, Quaternion.identity);
            newCreature.transform.SetParent(creatureParent.transform);
            newCreature.homePos = homePos;
            creatures.Add(newCreature);
        }
    }

    protected void spawnHouses() {
        for (int i = 0; i < nCreatures; i++) {
            Vector3 housePos = creatures[i].homePos + (Vector3.Normalize(creatures[i].homePos) * HOUSE_DIST);
            housePos = new Vector3(housePos.x, HOUSE_HEIGHT, housePos.z);
            GameObject house = Instantiate(housePrefabs[(int)Random.Range(0, housePrefabs.Length - 1)], housePos, Quaternion.identity);
            
            // position the house nicely
            float houseScale = 0.18f;
            house.transform.localScale = new Vector3(houseScale, houseScale, houseScale);
            house.transform.LookAt(new Vector3(0, HOUSE_HEIGHT, 0));
            house.transform.RotateAround (house.transform.position, house.transform.up, 180f);
        }
    }

    private void updateGraph() {
        // get number of item types per creature (assumes its the same for all)
        int nItems = creatures[0].itemStash.items.Length;

        // initialise array to store the averages
        float [] averageValues = new float [nItems];
        SetZero(averageValues);

        // get all average perceived values
        for (int i = 0; i < nCreatures; i++) {
            for (int j = 0; j < nItems; j++) {
                averageValues[j] += creatures[i].itemStash.items[j].perceivedValue;
            }
        }

        // apply values to the graph
        for (int i = 0; i < nItems; i++) {
            averageValues[i] = averageValues[i]/nCreatures;
            chartData.series[0].data[i] = new Data(averageValues[i]);
        }
        chartData.gameObject.GetComponent<Chart>().UpdateChart();
    }

    public void SetZero(float [] array) {
        for (int i = 0; i < array.Length; i++) {
            array[i] = 0;
        }
    }

    private Vector3 getHomePos(int index) {
        float xPos;
        float zPos;
        float nPerSide = nCreatures/4;
        float offset = nPerSide/(RADIUS * 2);

        zPos = ((index % (nPerSide)) / nPerSide) * RADIUS * 2 - RADIUS + offset;
        if (index < nPerSide) {
            xPos = -RADIUS;
            return new Vector3(xPos, HEIGHT, zPos);
        }
        if (index < 2 * nPerSide) {
            xPos = RADIUS;
            return new Vector3(xPos, HEIGHT, zPos);
        }

        xPos = ((index % (nPerSide)) / nPerSide) * RADIUS * 2 - RADIUS + offset;
        if (index < 3 * nPerSide) {
            zPos = -RADIUS;
            return new Vector3(xPos, HEIGHT, zPos);
        }
        zPos = RADIUS;
        return new Vector3(xPos, HEIGHT, zPos);
    }

    //Called each frame
    protected override void Update() {
        base.Update();
        float rotSpeed = 4; //degrees per second
        camRig.transform.localRotation = Quaternion.Euler(0, rotSpeed * Time.deltaTime, 0) * camRig.transform.localRotation;
    }
    
    IEnumerator Appear() {
        updateGraph();
        chartData.gameObject.GetComponent<PrimerObject>().ScaleUpFromZero();
        for (int i = 0; i < creatures.Count; i++) {
            creatures[i].ScaleUpFromZero();
        }
        yield return null;
    }

    //Define event actions
    IEnumerator Zoom() { 
        camRig.ZoomTo(20f, duration: 4); 
        yield return new WaitForSeconds(4);
    }

    IEnumerator MoveAround() {
        for (int i = 0; i < creatures.Count; i++) {
            creatures[i].WalkTo(new Vector3(Random.Range(-3f, 3f), HEIGHT, Random.Range(-3f, 3f)), duration: 1f);
        }
        
        yield return new WaitForSeconds(1f);
        
        for (int i = 0; i < creatures.Count; i++) {
            creatures[i].WalkTo(creatures[i].homePos, duration: 1f);
        }

        yield return null;
    }

    IEnumerator RunTimestep() {
        for (int i = 0; i < creatures.Count; i++) {
            creatures[i].ProduceItems();
        }
        yield return new WaitForSeconds(3f);

        for (int i = 0; i < creatures.Count; i++) {
            creatures[i].WalkTo(new Vector3(Random.Range(-3f, 3f), HEIGHT, Random.Range(-3f, 3f)), duration: 1f);
        }
        
        yield return new WaitForSeconds(1f);
        
        for (int i = 0; i < creatures.Count; i++) {
            creatures[i].WalkTo(creatures[i].homePos, duration: 1f);
        }

        yield return new WaitForSeconds(1f);
        
        // if we have not reached the end of the simulation...
        if (dayNumber < N_DAYS) {
            
            // consume items (no animation for this currently)
            for (int i = 0; i < creatures.Count; i++) {
                creatures[i].ConsumeItems();
            }

            // update the bar graph
            updateGraph();

            // increase the date
            dayNumber++;
            StartCoroutine("RunTimestep");
        }
    }


    IEnumerator Disappear() {
        for (int i = 0; i < creatures.Count; i++) {
            creatures[i].ScaleDownToZero();
        }
        
        yield return null;
    }

    //Construct schedule
    protected override void DefineSchedule() {
        /*
        If flexible is true, blocks run as long (or not) as they need to,
        with later blocks waiting until StopWaiting is called. Then,
        the next block starts on the next frame and all later block timings
        are shifted by the same amount.
        Useful for simulations whose duration is not predetermined
        */
        new SceneBlock(0f, Appear);
        new SceneBlock(5f, RunTimestep);
        
        // new SceneBlock(17f, Disappear);
    }
}
