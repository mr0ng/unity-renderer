using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhoneNumberFormatter : MonoBehaviour
{
    private TMP_InputField inputField;

    private void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.onValueChanged.AddListener(FormatPhoneNumber);
    }

    private void FormatPhoneNumber(string input)
    {
        string formattedNumber = string.Empty;

        // Remove any non-digit characters from the input
        foreach (char character in input)
        {
            if (char.IsDigit(character))
            {
                formattedNumber += character;
            }
        }

        // Add spaces to format the phone number
        if (formattedNumber.Length >= 3)
        {
            formattedNumber = formattedNumber.Insert(3, " ");
        }

        if (formattedNumber.Length >= 7)
        {
            formattedNumber = formattedNumber.Insert(7, " ");
        }

        // Set the formatted number to the TextMeshProUGUI component
        inputField.text = formattedNumber;
    }
}
