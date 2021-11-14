﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MySceneDirector : Director
{
    [SerializeField] List<MyCreature> creatures = null;
    public MyCreature creaturePrefab;
    public int nCreatures;
    public GameObject creatureParent;
    private float RADIUS = 4f;
    private float HEIGHT = 0.3f;
    private Graph graph;
    public Transform cameraTransform;

    protected override void Awake() {
        base.Awake();
        spawnBlobs(nCreatures);
        camRig.GoToStandardPositions();

        // set object scale to zero if they're going to scale in
        for (int i = 0; i < creatures.Count; i++) {
            creatures[i].transform.localScale = Vector3.zero;
        }
    }

    protected void spawnBlobs(int n) {
        for (int i = 0; i < n; i++) {
            Vector3 homePos = getHomePos(i);
            MyCreature newCreature = Instantiate(creaturePrefab, homePos, Quaternion.identity);
            newCreature.transform.SetParent(creatureParent.transform);
            newCreature.homePos = homePos;
            creatures.Add(newCreature);
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

    IEnumerator MoveAndColor() {
        for (int i = 0; i < creatures.Count; i++) {
            creatures[i].WalkTo(new Vector3(Random.Range(-3f, 3f), HEIGHT, Random.Range(-3f, 3f)), duration: 1f);
        }
        
        yield return new WaitForSeconds(1f);
        
        for (int i = 0; i < creatures.Count; i++) {
            creatures[i].WalkTo(creatures[i].homePos, duration: 1f);
        }

        yield return new WaitForSeconds(1f);
        
        for (int i = 0; i < creatures.Count; i++) {
            creatures[i].WalkTo(new Vector3(Random.Range(-3f, 3f), HEIGHT, Random.Range(-3f, 3f)), duration: 1f);
        }

        yield return new WaitForSeconds(1f);
        
        for (int i = 0; i < creatures.Count; i++) {
            creatures[i].WalkTo(creatures[i].homePos, duration: 1f);
        }

        yield return null;
    }

    //Define event actions
    IEnumerator GraphAppear() { 
        graph = Instantiate(graphPrefab);
        graph.transform.SetParent(cameraTransform);
        graph.transform.localPosition = new Vector3(-0.63f, 0.12f, 2.27f);
        graph.transform.LookAt(cameraTransform);
        graph.transform.RotateAround (graph.transform.position, transform.up, 180f);
        graph.transform.localRotation = Quaternion.Euler(0, 0, 0); 
        

        graph.Initialize(
            xTicStep: 5,
            xMax: 50,
            yMax: 50,
            yTicStep: 5,
            zTicStep: 1,
            zMin: 0,
            zMax: 5,
            xAxisLength: 8,
            yAxisLength: 5,
            zAxisLength: 0,
            scale: 0.07f, //True length is the axis length times scale. Scale controls thickness
            xAxisLabelPos: "along",
            xAxisLabelString: "Timestep",
            yAxisLabelString: "Amount"
        );
        graph.ScaleUpFromZero();
        yield return new WaitForSeconds(1);
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
        // new SceneBlock(2f, Zoom);
        new SceneBlock(1f, GraphAppear);
        new SceneBlock(5f, MoveAndColor);
        // new SceneBlock(17f, Disappear);
    }
}
