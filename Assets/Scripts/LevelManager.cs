using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public List<Pillar> pillars = new List<Pillar>();
    public int scoreThreshold = 3; // Example score threshold for completing the level

    private int savedPeopleCount = 0;

    private void Start()
    {
        FindAllPillars();
    }

    private void FindAllPillars()
    {
        pillars.Clear();
        Pillar[] foundPillars = FindObjectsOfType<Pillar>();

        foreach (Pillar pillar in foundPillars)
        {
            pillars.Add(pillar);
        }
    }

    public void SetFinalPillar(Pillar finalPillar)
    {
        foreach (Pillar pillar in pillars)
        {
            if (pillar == finalPillar)
            {
                pillar.isFinal = true;
                //pillar.ResetPillar();
            }
            else
            {
                pillar.isFinal = false;
            }
        }
    }

    public void SavePerson()
    {
        savedPeopleCount++;
        CheckIfLevelComplete();
    }

    private void CheckIfLevelComplete()
    {
        if (savedPeopleCount >= scoreThreshold)
        {
            // Level complete logic here
        }
    }
}
