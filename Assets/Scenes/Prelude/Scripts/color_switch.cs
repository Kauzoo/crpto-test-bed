using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class color_switch : MonoBehaviour
{
    // Start is called before the first frame update
    private Renderer hans;
    public int henlo = 1;

    private static Color black = new Color(0f, 0f, 0f, 1f);
    private static Color cyan = new Color(0f, 1f, 0.97f, 1f);
    private static Color red = new Color(1f, 0f, 0f, 1f);
    private static Color lime = new Color(0.7f, 1f, 0.1f, 1f);
    private static Color blue = new Color(0.01f, 0.4f, 0.95f, 1f);
    private static Color yellow = new Color(1f, 1f, 0f, 1f);
    private static Color yvi = new Color(0.01f, 1f, 0.2f, 1f);
    private static Color pink = new Color(0.9f, 0.01f, 1f, 1f);
    private static Color ourple = new Color(0.7f, 0.9f, 0.97f, 1f);
    private static Color gold = new Color(1f, 0.8f, 0.01f, 1f);
    private static Color violet = new Color(0.23f, 0.02f, 0.95f, 1f);

    private Color[] notes = {black, black, cyan, red, lime, cyan, red, lime, black, black, cyan, red, lime, cyan, red, lime, black, black, blue, yellow, yvi, blue, yellow, yvi, black, black, blue, yellow, yvi, blue, yellow, yvi, black, black, cyan, yellow, yvi, cyan, yellow, yvi, black, black, cyan, yellow, yvi, cyan, yellow, yvi, black, black, cyan, red, lime, cyan, red, lime, black, black, cyan, red, lime, cyan, red, lime, black, black, blue, lime, blue, blue, lime, blue, black, black, blue, lime, blue, blue, lime, blue};

    private int notetracker = 0;
        
    private float counter = 0;
    private float timeToAct = 0.25f;
    
    void Start()
    {
        hans = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (counter < timeToAct)
        {
            counter += Time.deltaTime;
        }
        else
        {
            counter = 0;
            hans.material.SetColor("_tint", notes[notetracker]);
            if (notetracker < 64) notetracker++;
            print(hans.material.GetColor("_tint").ToString());
            
        }
    }
}
