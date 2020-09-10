using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;
using System;

public class SeaBearAttacksScript : MonoBehaviour {

    public KMAudio audio;
    public KMBombInfo bomb;
    public KMSelectable[] buttons;

    public AudioSource strike;
    public AudioClip[] strikesounds;

    public TextMesh[] actions;
    public SpriteRenderer bear;

    private string[] correctActions = new string[] { "Play tuba\nbadly", "Play clarinet\nnicely", "Eat sliced\ncheese", "Eat grated\ncheese", "Wear\nsneakers", "Wear fancy\nshoes", "Take a\nwalk outside", "Go on\nhiking trip", "Drive nails\ninto ground", "Lay on\nground", "Wear sombrero\nin cool fashion", "Wear pants\nin goofy fashion", "Wear\nbubble skirt", "Wear\nhigh skirt", "Howl like\nwolf", "Screech like\nowl", "Walk back\nhome", "Jog around\nneighborhood", "Squeeze into\ntight space", "Do a\nhandstand", "Wave stick\nb&f quickly", "Wave flashlight\nb&f slowly" };
    private string[] incorrectActions = new string[] { "Play clarinet\nbadly", "Eat cubed\ncheese", "Wear\nclown shoes", "Run for\nyour life", "Stomp on\nground", "Wear sombrero\nin goofy fashion", "Wear\nhoop skirt", "Screech like\nchimpanzee", "Limp back\nhome", "Crawl into\ntight space", "Wave flashlight\nb&f quickly" };
    private int correct;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleActive;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        moduleActive = false;
        GetComponent<KMNeedyModule>().OnNeedyActivation += OnNeedyActivation;
        GetComponent<KMNeedyModule>().OnTimerExpired += OnTimerExpired;
        foreach (KMSelectable obj in buttons)
        {
            KMSelectable pressed = obj;
            pressed.OnInteract += delegate () { PressButton(pressed); return false; };
        }
        bomb.OnBombExploded += delegate () { OnEnd(false); };
        bomb.OnBombSolved += delegate () { OnEnd(true); };
    }

    void Start () {
        correct = -1;
        bear.enabled = false;
        actions[0].text = "";
        actions[1].text = "";
        actions[2].text = "";
        Debug.LogFormat("[Sea Bear Attacks #{0}] Needy Sea Bear Attacks has loaded! Waiting for first activation...", moduleId);
    }

    void OnEnd(bool n)
    {
        bombSolved = true;
        if (n)
        {
            actions[0].text = "";
            actions[1].text = "";
            actions[2].text = "";
        }
    }

    void PressButton(KMSelectable pressed)
    {
        if (moduleActive == true)
        {
            pressed.AddInteractionPunch(0.5f);
            audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
            if (Array.IndexOf(buttons, pressed) == correct)
            {
                GetComponent<KMNeedyModule>().HandlePass();
                Debug.LogFormat("[Sea Bear Attacks #{0}] The action \"{1}\" was correct! Module temporarily neutralized! Waiting for next activation...", moduleId, actions[Array.IndexOf(buttons, pressed)].text.Replace("\n", " "));
            }
            else
            {
                strike.clip = strikesounds[UnityEngine.Random.Range(0, 6)];
                strike.Play();
                GetComponent<KMNeedyModule>().HandleStrike();
                GetComponent<KMNeedyModule>().HandlePass();
                Debug.LogFormat("[Sea Bear Attacks #{0}] The action \"{1}\" was incorrect! Strike! Waiting for next activation...", moduleId, actions[Array.IndexOf(buttons, pressed)].text.Replace("\n", " "));
            }
            bear.enabled = false;
            actions[0].text = "";
            actions[1].text = "";
            actions[2].text = "";
            moduleActive = false;
        }
    }

    protected void OnNeedyActivation()
    {
        List<int> order = new List<int>() { 0, 1, 2 };
        int[] chosen = new int[] { -1, -1 };
        order = order.Shuffle();
        correct = order[0];
        for (int i = 0; i < 3; i++)
        {
            int choice = UnityEngine.Random.Range(0, i == 0 ? correctActions.Length : incorrectActions.Length);
            if (i != 0)
            {
                while (chosen.Contains(choice) || actions[order[0]].text.Equals(correctActions[choice * 2]) || actions[order[0]].text.Equals(correctActions[choice * 2 + 1]))
                    choice = UnityEngine.Random.Range(0, incorrectActions.Length);
                chosen[i - 1] = choice;
                if (choice == 5 && order[i] != 1)
                    actions[order[i]].characterSize = 65;
                else if (choice == 10 && order[i] == 0)
                    actions[order[i]].characterSize = 70;
                else
                    actions[order[i]].characterSize = 80;
            }
            else
            {
                if ((choice == 10 || choice == 11) && order[0] != 1)
                    actions[order[i]].characterSize = 65;
                else if (choice == 17 && order[0] == 2)
                    actions[order[i]].characterSize = 75;
                else if (choice == 21 && order[i] == 0)
                    actions[order[i]].characterSize = 70;
                else
                    actions[order[i]].characterSize = 80;
            }
            actions[order[i]].text = i == 0 ? correctActions[choice] : incorrectActions[choice];
        }
        Debug.LogFormat("[Sea Bear Attacks #{0}] The module has activated! The displayed actions are: \"{1}\", \"{2}\", and \"{3}\"", moduleId, actions[0].text.Replace("\n", " "), actions[1].text.Replace("\n", " "), actions[2].text.Replace("\n", " "));
        Debug.LogFormat("[Sea Bear Attacks #{0}] The correct action is \"{1}\"", moduleId, actions[order[0]].text.Replace("\n", " "));
        bear.enabled = true;
        moduleActive = true;
    }

    protected void OnTimerExpired()
    {
        strike.clip = strikesounds[UnityEngine.Random.Range(0, 6)];
        strike.Play();
        GetComponent<KMNeedyModule>().HandleStrike();
        Debug.LogFormat("[Sea Bear Attacks #{0}] The correct action was not pressed in time! Strike! Waiting for next activation...", moduleId);
        correct = -1;
        bear.enabled = false;
        actions[0].text = "";
        actions[1].text = "";
        actions[2].text = "";
        moduleActive = false;
    }

    //twitch plays
    private bool bombSolved = false;
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} press <#> [Presses the specified action] | Valid actions are 1-3 from top to bottom";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length > 2)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 2)
            {
                int temp = 0;
                if (int.TryParse(parameters[1], out temp))
                {
                    if (temp < 1 || temp > 3)
                    {
                        yield return "sendtochaterror The specified action to press '" + parameters[1] + "' is out of range 1-3!";
                        yield break;
                    }
                    buttons[temp - 1].OnInteract();
                }
                else
                {
                    yield return "sendtochaterror The specified action to press '" + parameters[1] + "' is invalid!";
                }
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify which action to press!";
            }
            yield break;
        }
    }

    void TwitchHandleForcedSolve()
    {
        //The code is done in a coroutine instead of here so that if the solvebomb command was executed this will just input the number right when it activates and it wont wait for its turn in the queue
        StartCoroutine(DealWithNeedy());
    }

    private IEnumerator DealWithNeedy()
    {
        while (!bombSolved)
        {
            while (!moduleActive) { yield return new WaitForSeconds(0.1f); }
            buttons[correct].OnInteract();
        }
    }
}
