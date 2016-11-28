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
    public Font Font;
    public Material FontMaterial;
    public Material UnpushedButtonMaterial;
    public Material PushedButtonMaterial;

    private char[] _letters;
    private bool[] _pushed;
    private bool _isSolved;
    private IEnumerator[] _coroutines;
    private bool _submitButtonCoroutineActive = false;

    private static string[] _leftRight = new[] {
        "GLMOY",
        "AEFNX",
        "AOSWY",
        "KMOUY",
        "HKUVW",
        "BCJPT",
        "CRTXY",
        "EMRUV",
        "BHJLP",
        "AQSWX",
        "AGHLP",
        "CEIMT",
        "IQVWX",
        "GLSWZ",
        "LPRVY",
        "DEHJK",
        "AEPRZ",
        "IKQTV",
        "BDJNZ",
        "CGHIO",
        "FMQSZ",
        "IJQRT",
        "BFKNX",
        "BCDFU",
        "DFGNS",
        "DNOUZ"
    };
    private static string[] _aboveBelow = new[] {
        "DEQVZ",
        "DIRTU",
        "HKQRV",
        "EFHLT",
        "NPQST",
        "MOUXY",
        "BEJLW",
        "CDKTW",
        "KNVWX",
        "HOPUZ",
        "EJRYZ",
        "BDNPR",
        "ADFRS",
        "ABJMX",
        "GKSXZ",
        "AGMVY",
        "FHILN",
        "CEGMS",
        "CILMU",
        "AVWYZ",
        "CHOPY",
        "FOSUW",
        "CIJOQ",
        "AGKNX",
        "BIJLQ",
        "BFGPT"
    };

    void Start()
    {
        _pushed = new bool[12];
        _coroutines = new IEnumerator[12];
        _isSolved = false;

        _letters = Enumerable.Range(0, 26).Select(i => (char) (i + 'A')).ToArray().Shuffle().Take(12).ToArray();
        Debug.LogFormat("[AdjacentLetters] Letters:{0}", string.Join("", _letters.Select((b, i) => (i % 4 == 0 ? "\n" : "") + b.ToString()).ToArray()));

        for (int i = 0; i < Buttons.Length; i++)
        {
            var label = new GameObject { name = "Label" };
            label.transform.parent = Buttons[i].transform;
            label.transform.localPosition = new Vector3(0, 0.0401f, 0);
            label.transform.localEulerAngles = new Vector3(90, 0, 0);
            label.transform.localScale = new Vector3(.01f, .01f, .01f);

            var t = label.AddComponent<TextMesh>();
            t.text = _letters[i].ToString();
            t.anchor = TextAnchor.MiddleCenter;
            t.alignment = TextAlignment.Center;
            t.fontSize = 72;
            t.font = Font;
            t.color = Color.black;

            t.GetComponent<MeshRenderer>().material = FontMaterial;

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

        var expectation = new bool[12];
        for (int i = 0; i < 12; i++)
        {
            var x = i % 4;
            var y = i / 4;
            if ((x > 0 && _leftRight[_letters[i] - 'A'].Contains(_letters[i - 1]) || (x < 3 && _leftRight[_letters[i] - 'A'].Contains(_letters[i + 1]))))
                expectation[i] = true;
            if ((y > 0 && _aboveBelow[_letters[i] - 'A'].Contains(_letters[i - 4]) || (y < 2 && _aboveBelow[_letters[i] - 'A'].Contains(_letters[i + 4]))))
                expectation[i] = true;
        }

        if (_pushed.SequenceEqual(expectation))
        {
            Module.HandlePass();
            _isSolved = true;
        }
        else
        {
            Debug.LogFormat("[AdjacentLetters] Submitted:{0}\nExpected: {1}",
                string.Join(" ", _pushed.Select((b, i) => (i % 4 == 0 ? "\n" : "") + string.Format(b ? "[{0}]" : "{0}", _letters[i])).ToArray()),
                string.Join(" ", expectation.Select((b, i) => (i % 4 == 0 ? "\n" : "") + string.Format(b ? "[{0}]" : "{0}", _letters[i])).ToArray()));
            Module.HandleStrike();
        }
    }
}
