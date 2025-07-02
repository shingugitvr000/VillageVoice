using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("UI ¿ä¼Ò")]
    public Text characterNameText;
    public Text dialogueText;
    public Image characterPortrait;  // Ä³¸¯ÅÍ ÃÊ»óÈ­ (¼±ÅÃ»çÇ×)

    [Header("À½¼º ¼³Á¤")]
    public AudioSource audioSource;

    [Header("±¸µÎÁ¡ À½¼º (°íÁ¤)")]
    public AudioClip dotSound;          // ¸¶Ä§Ç¥(.), ½°Ç¥(,) ¼Ò¸®
    public AudioClip questionSound;     // ¹°À½Ç¥(?) ¼Ò¸®
    public AudioClip exclamationSound;  // ´À³¦Ç¥(!) ¼Ò¸®
    public AudioClip defaultPuncSound;  // ±âÅ¸ ±¸µÎÁ¡ ¼Ò¸®

    [Header("´ëÈ­ µ¥ÀÌÅÍ")]
    public DialogueData currentDialogue;

    [Header("°øÅë ¼³Á¤")]
    public float defaultTypingSpeed = 0.15f;

    private int currentLineIndex = 0;
    private bool isTyping = false;
    private CharacterData currentCharacter;

    void Start()
    {
        if (currentDialogue != null)
        {
            StartDialogue();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                // Å¸ÀÌÇÎ ÁßÀÌ¸é Áï½Ã ¿Ï·á
                StopAllCoroutines();
                CompleteCurrentLine();
            }
            else
            {
                // ´ÙÀ½ ´ëÈ­·Î
                NextDialogue();
            }
        }
    }

    public void SetDialogue(DialogueData newDialogue)
    {
        currentDialogue = newDialogue;
        currentLineIndex = 0;
        StartDialogue();
    }

    void StartDialogue()
    {
        if (currentDialogue == null || currentDialogue.dialogueLines.Length == 0)
        {
            Debug.LogWarning("´ëÈ­ µ¥ÀÌÅÍ°¡ ¾ø½À´Ï´Ù!");
            return;
        }

        DisplayCurrentLine();
    }

    void DisplayCurrentLine()
    {
        if (currentLineIndex >= currentDialogue.dialogueLines.Length)
        {
            if (currentDialogue.loopDialogue)
            {
                currentLineIndex = 0;  // Ã³À½ºÎÅÍ ´Ù½Ã
            }
            else
            {
                Debug.Log("´ëÈ­ Á¾·á");
                return;
            }
        }

        DialogueLine currentLine = currentDialogue.dialogueLines[currentLineIndex];
        currentCharacter = currentLine.speaker;

        if (currentCharacter != null)
        {
            SetupCharacterUI();
            float speed = currentLine.useSlowTyping ? 0.3f :
                         (currentCharacter.typingSpeed > 0 ? currentCharacter.typingSpeed : defaultTypingSpeed);

            StartCoroutine(TypeText(currentLine.message, speed));
        }
        else
        {
            Debug.LogWarning($"´ëÈ­ ¶óÀÎ {currentLineIndex}¿¡ Ä³¸¯ÅÍ°¡ ¼³Á¤µÇÁö ¾Ê¾Ò½À´Ï´Ù!");
        }
    }

    void SetupCharacterUI()
    {
        // Ä³¸¯ÅÍ ÀÌ¸§°ú »ö»ó ¼³Á¤
        characterNameText.text = currentCharacter.characterName;
        characterNameText.color = currentCharacter.nameColor;

        // Ä³¸¯ÅÍ ÃÊ»óÈ­ ¼³Á¤ (ÀÖ´Â °æ¿ì)
        if (characterPortrait != null && currentCharacter.characterPortrait != null)
        {
            characterPortrait.sprite = currentCharacter.characterPortrait;
            characterPortrait.gameObject.SetActive(true);
        }
        else if (characterPortrait != null)
        {
            characterPortrait.gameObject.SetActive(false);
        }

        // À½¼º ¼³Á¤
        audioSource.pitch = currentCharacter.pitch;
        audioSource.volume = currentCharacter.volume;

        Debug.Log($"{currentCharacter.characterName}ÀÌ(°¡) ¸»ÇÕ´Ï´Ù!");
    }

    void NextDialogue()
    {
        currentLineIndex++;
        DisplayCurrentLine();
    }

    void CompleteCurrentLine()
    {
        isTyping = false;
        DialogueLine currentLine = currentDialogue.dialogueLines[currentLineIndex];
        dialogueText.text = currentLine.message;
    }

    IEnumerator TypeText(string message, float speed)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in message)
        {
            dialogueText.text += c;
            PlayCharacterSound(c);
            yield return new WaitForSeconds(speed);
        }

        isTyping = false;
    }

    void PlayCharacterSound(char character)
    {
        if (currentCharacter == null) return;

        audioSource.Stop();

        if (char.IsLetter(character) || char.IsDigit(character))
        {
            AudioClip soundToPlay = GetVowelSound(character);

            if (soundToPlay != null)
            {
                audioSource.clip = soundToPlay;
                audioSource.Play();
                Debug.Log($"'{character}' ¼Ò¸® Àç»ý (±ÛÀÚ/¼ýÀÚ)");
            }
        }
        else if (char.IsPunctuation(character))
        {
            AudioClip punctuationClip = GetPunctuationSound(character);

            if (punctuationClip != null)
            {
                audioSource.clip = punctuationClip;
                audioSource.Play();
                Debug.Log($"'{character}' ±¸µÎÁ¡ ¼Ò¸® Àç»ý: {punctuationClip.name}");
            }
        }
    }

    AudioClip GetVowelSound(char c)
    {
        // ¼ýÀÚ Ã³¸®
        if (char.IsDigit(c))
        {
            return GetNumberSound(c);
        }

        // ÇÑ±Û Ã³¸®
        if (c >= '°¡' && c <= 'ÆR')
        {
            return GetKoreanVowelSound(c);
        }

        // ¿µ¾î Ã³¸®
        if (char.IsLetter(c))
        {
            return GetEnglishVowelSound(c);
        }

        return currentCharacter.defaultSound;
    }

    AudioClip GetPunctuationSound(char punctuation)
    {
        switch (punctuation)
        {
            case '.':   // ¸¶Ä§Ç¥
            case ',':   // ½°Ç¥
            case ';':   // ¼¼¹ÌÄÝ·Ð
            case ':':   // ÄÝ·Ð
                return dotSound;

            case '?':   // ¹°À½Ç¥
                return questionSound;

            case '!':   // ´À³¦Ç¥
                return exclamationSound;

            case '~':   // ¹°°áÇ¥
            case '-':   // ÇÏÀÌÇÂ
            case '"':   // µû¿ÈÇ¥
            case '\'':  // ÀÛÀºµû¿ÈÇ¥
            case '(':   // °ýÈ£
            case ')':
            case '[':
            case ']':
            case '{':
            case '}':
            default:
                return defaultPuncSound;
        }
    }

    AudioClip GetNumberSound(char number)
    {
        // ¼ýÀÚ¸¦ ÇÑ±¹¾î ¹ßÀ½¿¡ ¸Â°Ô ¸ðÀ½À¸·Î ¸ÅÇÎ
        switch (number)
        {
            case '0': return currentCharacter.oSound;   // ¿µ(yeong) ¡æ ¤Å ¡æ e
            case '1': return currentCharacter.iSound;   // ÀÏ(il) ¡æ ¤Ó ¡æ i  
            case '2': return currentCharacter.iSound;   // ÀÌ(i) ¡æ ¤Ó ¡æ i
            case '3': return currentCharacter.aSound;   // »ï(sam) ¡æ ¤¿ ¡æ a
            case '4': return currentCharacter.aSound;   // »ç(sa) ¡æ ¤¿ ¡æ a
            case '5': return currentCharacter.oSound;   // ¿À(o) ¡æ ¤Ç ¡æ o
            case '6': return currentCharacter.uSound;   // À°(yuk) ¡æ ¤Ì ¡æ u
            case '7': return currentCharacter.iSound;   // Ä¥(chil) ¡æ ¤Ó ¡æ i
            case '8': return currentCharacter.aSound;   // ÆÈ(pal) ¡æ ¤¿ ¡æ a
            case '9': return currentCharacter.uSound;   // ±¸(gu) ¡æ ¤Ì ¡æ u
            default: return currentCharacter.defaultSound;
        }
    }

    AudioClip GetKoreanVowelSound(char korean)
    {
        int code = korean - '°¡';
        int vowelIndex = (code % 588) / 28;

        switch (vowelIndex)
        {
            case 0:
            case 1:
            case 2:
            case 3:  // ¤¿, ¤À, ¤Á, ¤Â
                return currentCharacter.aSound;

            case 4:
            case 5:
            case 6:
            case 7:  // ¤Ã, ¤Ä, ¤Å, ¤Æ
                return currentCharacter.eSound;

            case 8:
            case 12:  // ¤Ç, ¤Ë
                return currentCharacter.oSound;

            case 13:
            case 17:
            case 18:  // ¤Ì, ¤Ð, ¤Ñ
                return currentCharacter.uSound;

            case 20:  // ¤Ó
                return currentCharacter.iSound;

            default:
                return currentCharacter.defaultSound;
        }
    }

    AudioClip GetEnglishVowelSound(char english)
    {
        char lower = char.ToLower(english);

        switch (lower)
        {
            case 'a': return currentCharacter.aSound;
            case 'e': return currentCharacter.eSound;
            case 'i': return currentCharacter.iSound;
            case 'o': return currentCharacter.oSound;
            case 'u': return currentCharacter.uSound;
            default: return currentCharacter.uSound; // ÀÚÀ½
        }
    }

    // ¿¡µðÅÍ¿¡¼­ ½±°Ô Å×½ºÆ®ÇÒ ¼ö ÀÖ´Â ÇÔ¼ö
    [ContextMenu("´ÙÀ½ ´ëÈ­")]
    void TestNextDialogue()
    {
        if (!isTyping)
        {
            NextDialogue();
        }
    }
}