using System;
using System.Collections;
using System.Linq;
using AdjacentLetters;
using UnityEngine;

/// <summary>
/// On the Subject of Adjacent Letters
/// Created by lumbud84, implemented by Timwi
/// </summary>
public class AdjacentLettersModule : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMBombModule Module;
    public KMAudio Audio;
    public KMSelectable[] Buttons;
    public KMSelectable SubmitButton;
    public Material FontMaterial;
    public Material UnpushedButtonMaterial;
    public Material PushedButtonMaterial;
    public GameObject Label;

    private char[] _letters;
    private bool[] _expectation;
    private bool[] _pushed;
    private bool _isSolved;
    private IEnumerator[] _coroutines;
    private bool _submitButtonCoroutineActive = false;

    private static int _moduleIdCounter = 1;
    private int _moduleId;

    private static readonly string[] _leftRight = new[] {
        "GJMOY",
        "IKLRT",
        "BHIJW",
        "IKOPQ",
        "ACGIJ",
        "CERVY",
        "ACFNS",
        "LRTUX",
        "DLOWZ",
        "BQTUW",
        "AFPXY",
        "GKPTZ",
        "EILQT",
        "PQRSV",
        "HJLUZ",
        "DMNOX",
        "CEOPV",
        "AEGSU",
        "ABEKQ",
        "GVXYZ",
        "FMVXZ",
        "DHMNW",
        "DFHMN",
        "BDFKW",
        "BCHSU",
        "JNRSY"
    };
    private static readonly string[] _aboveBelow = new[] {
        "HKPRW",
        "CDFYZ",
        "DEMTU",
        "CJTUW",
        "KSUWZ",
        "AGJPQ",
        "HOQYZ",
        "DKMPS",
        "EFNUV",
        "EHIOS",
        "DIORZ",
        "ABRVX",
        "BFPWX",
        "AFGHL",
        "IQSTX",
        "CFHKR",
        "BDIKN",
        "BNOXY",
        "GMVYZ",
        "CJLSU",
        "BILNY",
        "AEJQX",
        "GLQRT",
        "AJNOV",
        "EGMTW",
        "CLMPV"
    };

    void Start()
    {
        _moduleId = _moduleIdCounter++;
        _pushed = new bool[12];
        _coroutines = new IEnumerator[12];
        _isSolved = false;

        FontMaterial.mainTexture.wrapMode = TextureWrapMode.Clamp;

        _letters = Enumerable.Range(0, 26).Select(i => (char) (i + 'A')).ToArray().Shuffle().Take(12).ToArray();
        _expectation = new bool[12];
        for (int i = 0; i < 12; i++)
        {
            var x = i % 4;
            var y = i / 4;
            if ((x > 0 && _leftRight[_letters[i] - 'A'].Contains(_letters[i - 1]) || (x < 3 && _leftRight[_letters[i] - 'A'].Contains(_letters[i + 1]))))
                _expectation[i] = true;
            if ((y > 0 && _aboveBelow[_letters[i] - 'A'].Contains(_letters[i - 4]) || (y < 2 && _aboveBelow[_letters[i] - 'A'].Contains(_letters[i + 4]))))
                _expectation[i] = true;
        }

        Debug.LogFormat("[AdjacentLetters #{1}] Solution:{0}", string.Join("", _expectation.Select((b, i) => (i % 4 == 0 ? "\n" : "") + string.Format(b ? "[{0}]" : " {0} ", _letters[i])).ToArray()), _moduleId);

        for (int i = 0; i < Buttons.Length; i++)
        {
            if (i == 0)
                Label.GetComponent<TextMesh>().text = _letters[i].ToString();
            else
            {
                var label = Instantiate(Label);
                label.name = "Label";
                label.transform.parent = Buttons[i].transform;
                label.transform.localPosition = new Vector3(0, 0.0401f, 0);
                label.transform.localEulerAngles = new Vector3(90, 0, 0);
                label.transform.localScale = new Vector3(.01f, .01f, .01f);
                label.GetComponent<TextMesh>().text = _letters[i].ToString();
            }

            var j = i;
            Buttons[i].OnInteract += delegate { Push(j); return false; };
            Buttons[i].GetComponent<MeshRenderer>().material = UnpushedButtonMaterial;
        }
        SubmitButton.OnInteract += delegate { Submit(); return false; };
    }

    private void Push(int i)
    {
        Buttons[i].AddInteractionPunch(.1f);
        Audio.PlaySoundAtTransform(_pushed[i] ? "ClickOut" : "ClickIn", Buttons[i].transform);

        _pushed[i] = !_pushed[i];
        if (_coroutines[i] == null)
        {
            _coroutines[i] = ButtonCoroutine(i, !_pushed[i]);
            StartCoroutine(_coroutines[i]);
        }
    }

    private IEnumerator ButtonCoroutine(int i, bool curState)
    {
        var origLocation = Buttons[i].transform.localPosition;
        while (_pushed[i] != curState)
        {
            curState = _pushed[i];
            if (curState)
            {
                for (int j = 0; j <= 5; j++)
                {
                    Buttons[i].transform.localPosition = new Vector3(origLocation.x, -j / 128.2f, origLocation.z);
                    yield return null;
                }
                Buttons[i].GetComponent<MeshRenderer>().material = PushedButtonMaterial;
                yield return new WaitForSeconds(.1f);
                for (int j = 0; j <= 10; j++)
                {
                    Buttons[i].transform.localPosition = new Vector3(origLocation.x, -0.039f + j / 1000f, origLocation.z);
                    yield return null;
                }
            }
            else
            {
                for (int j = 5; j >= 0; j--)
                {
                    Buttons[i].transform.localPosition = new Vector3(origLocation.x, -0.039f + j / 500f, origLocation.z);
                    yield return null;
                }
                Buttons[i].GetComponent<MeshRenderer>().material = UnpushedButtonMaterial;
                yield return new WaitForSeconds(.1f);
                for (int j = 10; j >= 0; j--)
                {
                    Buttons[i].transform.localPosition = new Vector3(origLocation.x, -j / 256.4f, origLocation.z);
                    yield return null;
                }
            }
            yield return new WaitForSeconds(.1f);
        }
        _coroutines[i] = null;
    }

    private IEnumerator SubmitButtonCoroutine()
    {
        var origLocation = SubmitButton.transform.localPosition;
        for (int j = 0; j <= 2; j++)
        {
            SubmitButton.transform.localPosition = new Vector3(origLocation.x, origLocation.y - j / 400f, origLocation.z);
            yield return null;
        }
        yield return new WaitForSeconds(.05f);
        for (int j = 5; j >= 0; j--)
        {
            SubmitButton.transform.localPosition = new Vector3(origLocation.x, origLocation.y - j / 1000f, origLocation.z);
            yield return null;
        }
        _submitButtonCoroutineActive = false;
    }

    private void Submit()
    {
        SubmitButton.AddInteractionPunch();
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, SubmitButton.transform);

        if (_isSolved)
            return;

        if (!_submitButtonCoroutineActive)
        {
            _submitButtonCoroutineActive = true;
            StartCoroutine(SubmitButtonCoroutine());
        }

        Debug.LogFormat("[AdjacentLetters #{1}] You submitted:{0}", string.Join("", _pushed.Select((b, i) => (i % 4 == 0 ? "\n" : "") + string.Format(b ? "[{0}]" : " {0} ", _letters[i])).ToArray()), _moduleId);

        if (_pushed.SequenceEqual(_expectation))
        {
            Module.HandlePass();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
            _isSolved = true;
        }
        else
        {
            Module.HandleStrike();
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} submit DPC INUF [submit letters to be pushed down; all other letters are brought back up] | !{0} submit! [bring all letters up and submit that]";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToUpperInvariant().Trim();

        if (!command.StartsWith("submit ", StringComparison.OrdinalIgnoreCase) && !command.EqualsIgnoreCase("submit!"))
            yield break;

        var letters = command.Substring(7).Replace(" ", "");
        if (command.EqualsIgnoreCase("submit!"))
            letters = "";
        if (letters.Any(ch => !_letters.Contains(ch)))
            yield break;

        yield return null;
        yield return Buttons.Where((btn, i) => _pushed[i] != letters.Contains(_letters[i]));
        yield return new WaitForSeconds(.6f);
        yield return new[] { SubmitButton };
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        for (var i = 0; i < 12; i++)
        {
            if (_expectation[i] != _pushed[i])
            {
                Buttons[i].OnInteract();
                yield return new WaitForSeconds(.1f);
            }
        }
        yield return new WaitForSeconds(.7f);
        SubmitButton.OnInteract();
    }
}
