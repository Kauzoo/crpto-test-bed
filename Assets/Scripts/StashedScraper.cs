using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Diagnostics;

public class StashedScraper : MonoBehaviour
{
    // Start is called before the first frame update
    public float counter = 0f;
    public float timeToAct = 0.5f;
    private Renderer rend;
    private bool once = true;

    void Start()
    {
                rend = GetComponent<Renderer>();
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
            //UnityEngine.Debug.Log("hello :3");
            /*
            Process proc = new Process
            {
                Filename = "powershell.exe",
                Arguments = "ls",
                UseShellExecute = true;
                RedirectStandardOutput = true;
            }
            */
            if (once)
            {
                //Process process = process.Start("powershell.exe", "echo ':3' >> temp.exe");
                //once = false;
                Process proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "C:/Users/User/AppData/Roaming/fnm/node-versions/v20.17.0/installation/node.exe",
                        Arguments = "c:/Users/User/Documents/dome/2024/FuckingNode/index.js",
                        RedirectStandardOutput = true,
                        UseShellExecute = false

                    }
                    
                };
                proc.Start();
                while(!proc.StandardOutput.EndOfStream){
                    string line = proc.StandardOutput.ReadLine();
                    UnityEngine.Debug.Log(line);
                    if (line == "true") rend.enabled = false;
                }
            }

            //int exitCode = await shellCommandToken;            
        }
    }
    
}