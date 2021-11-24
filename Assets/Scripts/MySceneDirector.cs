using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChartUtil;
using TMPro;

public class MySceneDirector : Director
{
    [Space]
    public int dayNumber = 0;
    public int nAgents;
    
    [Space]
    public MyAgent agentPrefab;
    public GameObject agentParent;
    public TextMeshProUGUI dayNumberObj;
    public Transform cameraTransform;
    public GameObject [] housePrefabs;
    public GameObject [] itemModels; // index correspond to itemName enum number
    public Color highReputationColor;
    public Color lowReputationColor;
    public ChartData chartData;
    public GameObject meetingPlaceParent;

    public List<MyAgent> agents = null;
    private float RADIUS = 4f;
    private float HEIGHT = 0.3f;
    private float HOUSE_HEIGHT = 0f;
    private float HOUSE_DIST = 1f;
    private int N_DAYS = 20; // number of days in the simulation

    protected override void Awake() {
        base.Awake();
        spawnBlobs(nAgents);
        spawnHouses();
        camRig.GoToStandardPositions();

        // set object scale to zero if they're going to scale in
        for (int i = 0; i < agents.Count; i++) {
            agents[i].transform.localScale = Vector3.zero;
        }
        chartData.gameObject.transform.localScale = Vector3.zero;
    }

    protected void spawnBlobs(int n) {
        agents = new List<MyAgent>();
        for (int i = 0; i < n; i++) {
            Vector3 homePos = getHomePos(i);
            MyAgent newAgent = Instantiate(agentPrefab, homePos, Quaternion.identity);
            newAgent.transform.SetParent(agentParent.transform);
            newAgent.homePos = homePos;
            agents.Add(newAgent);
        }
    }

    protected void spawnHouses() {
        for (int i = 0; i < nAgents; i++) {
            Vector3 housePos = agents[i].homePos + (Vector3.Normalize(agents[i].homePos) * HOUSE_DIST);
            housePos = new Vector3(housePos.x, HOUSE_HEIGHT, housePos.z);
            GameObject house = Instantiate(housePrefabs[(int)Random.Range(0, housePrefabs.Length - 1)], housePos, Quaternion.identity);
            
            // position the house nicely
            float houseScale = 0.18f;
            house.transform.localScale = new Vector3(houseScale, houseScale, houseScale);
            house.transform.LookAt(new Vector3(0, HOUSE_HEIGHT, 0));
            house.transform.RotateAround (house.transform.position, house.transform.up, 180f);
        }
    }

    private void updateBarGraph() {
        // get number of item types per agent (assumes its the same for all)
        int nItems = agents[0].itemStash.items.Length;

        // initialise array to store the averages
        float [] averageValues = new float [nItems];
        SetZero(averageValues);

        // get all average perceived values
        for (int i = 0; i < nAgents; i++) {
            for (int j = 0; j < nItems; j++) {
                averageValues[j] += agents[i].itemStash.items[j].perceivedValue;
            }
        }

        // apply values to the graph
        for (int i = 0; i < nItems; i++) {
            averageValues[i] = averageValues[i]/nAgents;
            chartData.series[0].data[i] = new Data(averageValues[i]);
        }
        chartData.gameObject.GetComponent<Chart>().UpdateChart();
    }

    private void updateLineGraph() {
        // get number of item types per agent (assumes its the same for all)
        int nItems = agents[0].itemStash.items.Length;

        // initialise array to store the averages
        float [] averageValues = new float [nItems];
        SetZero(averageValues);

        // get all average perceived values
        for (int i = 0; i < nAgents; i++) {
            for (int j = 0; j < nItems; j++) {
                averageValues[j] += agents[i].itemStash.items[j].perceivedValue;
            }
        }

        // apply values to the graph
        for (int i = 0; i < nItems; i++) {
            averageValues[i] = averageValues[i]/nAgents;
            chartData.series[i].data.Add(new Data(averageValues[i], dayNumber));
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
        float nPerSide = nAgents/4;
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

    // Called each frame
    protected override void Update() {
        base.Update();
        float rotSpeed = 4; //degrees per second
        camRig.transform.localRotation = Quaternion.Euler(0, rotSpeed * Time.deltaTime, 0) * camRig.transform.localRotation;
    }
    
    IEnumerator Appear() {
        updateLineGraph();
        chartData.gameObject.GetComponent<PrimerObject>().ScaleUpFromZero();
        for (int i = 0; i < agents.Count; i++) {
            agents[i].ScaleUpFromZero();
        }
        yield return null;
    }

    // Define event actions
    IEnumerator Zoom() { 
        camRig.ZoomTo(20f, duration: 4); 
        yield return new WaitForSeconds(4);
    }

    IEnumerator MoveAround() {
        for (int i = 0; i < agents.Count; i++) {
            agents[i].WalkTo(new Vector3(Random.Range(-3f, 3f), HEIGHT, Random.Range(-3f, 3f)), duration: 1f);
        }
        
        yield return new WaitForSeconds(1f);
        
        for (int i = 0; i < agents.Count; i++) {
            agents[i].WalkTo(agents[i].homePos, duration: 1f);
        }

        yield return null;
    }

    IEnumerator RunTimestep() {
        for (int i = 0; i < agents.Count; i++) {
            agents[i].ExecuteBehaviour(3f);
        }
        yield return new WaitForSeconds(3f);
        
        // if we have not reached the end of the simulation...
        if (dayNumber < N_DAYS) {

            // update graph
            updateLineGraph();

            // increase the date
            dayNumber++;
            dayNumberObj.text = dayNumber.ToString();
            StartCoroutine("RunTimestep");
        }
    }

    IEnumerator Disappear() {
        for (int i = 0; i < agents.Count; i++) {
            agents[i].ScaleDownToZero();
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
