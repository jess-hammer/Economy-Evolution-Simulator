using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class AgentSelectorController : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject scrollViewObj;
    public TextMeshProUGUI scrollViewText;
    public Camera camera;

    private MyAgent agentClicked = null;
    void Start()
    {
        scrollViewObj.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)){ // if left button pressed...
            if (Input.mousePosition.x < (Screen.width/4) * 3) {
                Clear();
            }

            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)){
                // the object identified by hit.transform was clicked
                MyAgent newAgentClicked = hit.transform.gameObject.GetComponent<MyAgent>();
                if (newAgentClicked != null) {
                    Clear();
                    agentClicked = newAgentClicked;
                    GameObject arrow = agentClicked.transform.Find("Arrow").gameObject;
                    if (arrow)
                        arrow.gameObject.SetActive(true);
                    UpdateScrollViewText();
                    scrollViewObj.SetActive(true);
                }
            }
        }
    }

    public void UpdateScrollViewText() {
        if (agentClicked != null)
            scrollViewText.text = agentClicked.ToString();
    }

    private void Clear() {
        if (agentClicked != null) {
            GameObject arrow = agentClicked.transform.Find("Arrow").gameObject;
            if (arrow)
                arrow.gameObject.SetActive(false);
            scrollViewObj.SetActive(false);
            agentClicked = null;
        }
    }
}
