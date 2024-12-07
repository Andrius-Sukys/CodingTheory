using System.IO;

namespace A5.Services;

// Service that is dedicated to validation of user inputs such as text entered in text boxes and uploaded images

public interface IValidationService
{
    bool IsMValid(string m);

    bool IsProbabilityOfDistortionValid(string probability);

    bool IsBinaryVectorValid(string vector);

    bool IsInputVectorSizeValid(string vectorSize, string m);

    bool IsReceivedVectorSizeValid(string vectorSize, string m);

    bool IsTextValid(string text);

    bool IsValidBMPFile(string filePath);
}

public class ValidationService : IValidationService
{
    // Method used to determine whether parameter m is valid
    public bool IsMValid(string m)
    {
        // If the text box is empty, it's invalid
        if (string.IsNullOrWhiteSpace(m))
        {
            return false;
        }

        // If the input is not an integer, it's invalid
        if (!int.TryParse(m.Trim(), out int mInt))
        {
            return false;
        }

        // If the number is lower than 1, it's invalid
        if (mInt < 1)
        {
            return false;
        }

        // Otherwise, it's valid
        return true;
    }

    // Method used to determine whether the probability of distortion is valid
    public bool IsProbabilityOfDistortionValid(string probability)
    {
        // If the text box is empty, it's invalid
        if (string.IsNullOrWhiteSpace(probability))
        {
            return false;
        }

        // If the input is not an integer, it's invalid
        if (!double.TryParse(probability.Trim(), out double probabilityDouble))
        {
            return false;
        }

        // If the number is not in range of [0; 1], it's invalid
        if ((probabilityDouble < 0) || (probabilityDouble > 1))
        {
            return false;
        }

        // Otherwise, it's valid
        return true;
    }

    // Method used to determine whether the input binary vector is valid
    public bool IsBinaryVectorValid(string vector)
    {
        // If the text box is empty, it's invalid
        if (string.IsNullOrWhiteSpace(vector))
        {
            return false;
        }

        // If any character inside the vector is not 0 or 1, it's invalid
        foreach (char c in vector)
        {
            if (c != '0' && c != '1')
            {
                return false;
            }
        }

        // Otherwise, it's valid
        return true;
    }

    // Method used to determine whether the input vector is of size (m + 1)
    public bool IsInputVectorSizeValid(string vectorSize, string m)
        => vectorSize.Length == (int.Parse(m) + 1);

    // Method used to determine whether the received vector is of size (2^m)
    public bool IsReceivedVectorSizeValid(string vectorSize, string m)
        => vectorSize.Length == Math.Pow(2, int.Parse(m));

    // Method used to determine whether the input text is valid
    public bool IsTextValid(string text)
    {
        // If the text box is empty, it's invalid
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        // Otherwise it's valid
        return true;
    }

    // Method used to determine whether the uploaded file is a valid BMP image
    public bool IsValidBMPFile(string filePath)
    {
        // If the file does not exist in the specified path, it is not valid
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return false;
        }

        // If the extension of the file is not ".bmp", it is not valid
        if (Path.GetExtension(filePath).ToLower() != ".bmp")
        {
            return false;
        }

        // Otherwise, it is valid
        return true;
    }
}
