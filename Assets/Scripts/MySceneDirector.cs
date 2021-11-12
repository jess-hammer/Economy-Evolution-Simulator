using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MySceneDirector : Director
{
    [SerializeField] List<MyCreature> creatures = null;
    public MyCreature creaturePrefab;
    public int nCreatures;
    public GameObject creatureParent;

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
            MyCreature newCreature = Instantiate(creaturePrefab, creatureParent.transform.position, Quaternion.identity);
            newCreature.transform.SetParent(creatureParent.transform);
            creatures.Add(newCreature);
        }
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
            creatures[i].WalkTo(new Vector3(Random.Range(-3f, 3f), 0.5f, Random.Range(-3f, 3f)), duration: 1f);
        }
        
        yield return new WaitForSeconds(2f);
        
        for (int i = 0; i < creatures.Count; i++) {
            creatures[i].WalkTo(new Vector3(Random.Range(-3f, 3f), 0.5f, Random.Range(-3f, 3f)), duration: 1f);
        }

        yield return new WaitForSeconds(2f);
        
        for (int i = 0; i < creatures.Count; i++) {
            creatures[i].WalkTo(new Vector3(Random.Range(-3f, 3f), 0.5f, Random.Range(-3f, 3f)), duration: 1f);
        }

        yield return null;
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
        new SceneBlock(3f, Zoom);
        new SceneBlock(5f, MoveAndColor);
        new SceneBlock(17f, Disappear);
    }
}
