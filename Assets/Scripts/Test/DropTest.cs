using UnityEngine;

public class DropTest : MonoBehaviour {

    private GameObject grid; 
    private float dropY = 0.0f;
    private float dropHeight;

    // Use this for initialization
    void Start () {
        grid = GameObject.FindWithTag("grid00");
        dropHeight = grid.GetComponent<RectTransform>().position.y - 200.0f;
        dropY = Time.deltaTime * 500;
    }
	
	// Update is called once per frame
	void Update () {

        if(dropHeight >= dropY)
        {
            grid.GetComponent<RectTransform>().position += new Vector3(0.0f, -dropY, 0.0f);
            dropHeight = dropHeight - dropY;
        }
        else
        {
            grid.GetComponent<RectTransform>().position += new Vector3(0.0f, -dropHeight, 0.0f);
        }
    }
}
