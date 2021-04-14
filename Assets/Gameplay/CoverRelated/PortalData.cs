using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PortalData
{
    public string description;
    public string destination;
    public KeyCode control;

    public bool IsQuit { get { return destination.Equals(ScriptDispenser.QUIT); } }

    public static PortalData GetDefault()
    {
        return new PortalData(
            "main menu",
            ScriptDispenser.MAINMENU
            );
    }

    public static PortalData GetQuit()
    {
        return new PortalData(
            "quit",
            ScriptDispenser.QUIT
            );
    }

    public static PortalData Fetch(string content)
    {
        int last_space = content.LastIndexOf(" ");
        if (last_space == -1) {
            Debug.Log("portal name: ... destination: " + content.Substring(last_space + 1));
            return new PortalData("...", content); 
        }
        else
        {
            Debug.Log("portal name: " + content.Substring(0, last_space) + " destination: " + content.Substring(last_space + 1));
            //description is everything up to the first space
            return new PortalData(content.Substring(0, last_space), content.Substring(last_space + 1));
        }
    }

    public PortalData(string description, string destination)
    {
        this.description = description;
        this.destination = destination;
    }

    public bool Equals(PortalData other)
    {
        return (other.description.Equals(description) && other.destination.Equals(destination));
    }
}
