using System.Drawing;
using Color = System.Drawing.Color;

namespace A5.Services;

// Service that is dedicated to all conversion-related operations such as binary to decimal conversion,
// text to vector conversion and vice versa, picture to vector conversion and vice versa

public interface IConverterService
{
    string GetBinaryFromDecimal(int decimalValue, int m);

    (List<int[]>, int) GetVectorsFromText(string text, int m);

    string GetTextFromVectors(List<int[]> vectors, int padding);

    (List<int[]>, int, int, int) GetVectorsFromBMPImage(Bitmap image, int m);

    Bitmap GetBMPImageFromVectors(List<int[]> vectors, int padding, int imageHeight, int imageWidth);
}

public class ConverterService : IConverterService
{
    // Method used to get binary value from its decimal representation
    public string GetBinaryFromDecimal(int decimalValue, int m)
    {
        // Declaring a variable to store remainder in
        int remainder;

        // Initializing a variable to store the converted binary value in
        string binaryValue = string.Empty;

        // Cycle goes on until decimalValue is more than 0
        while (decimalValue > 0)
        {
            // Remainder is the result of modulus of 2 applied on the decimal value
            remainder = decimalValue % 2;

            // Decimal value is divided by two
            decimalValue /= 2;

            // Remainder gets appended to the existing calculations of binary value, if any
            binaryValue = remainder.ToString() + binaryValue;
        }

        // Calculating the difference between m and length of the binary value, i.e. needed padding
        int padding = m - binaryValue.Length;

        // Working with each required bit of padding
        for (int i = 0; i < padding; ++i)
        {
            // During each execution of the cycle binary value gets 0 appended in front
            binaryValue = "0" + binaryValue;
        }

        return binaryValue;
    }

    // Method used to convert text to binary and split to multiple vectors based on parameter m
    public (List<int[]>, int) GetVectorsFromText(string text, int m)
    {
        // Converting input text into UTF8 bytes where each byte represents a letter
        byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(text);

        // Converting UTF8 bytes into binary representation, each byte takes up 8 characters with padding if needed
        string binaryText = string.Concat(utf8Bytes.Select(s => Convert.ToString(s, 2).PadLeft(8, '0')));

        // Each of the split vectors should be of size (m + 1)
        int vectorSize = m + 1;

        // Initializing a list to store split vectors
        List<int[]> vectors = new List<int[]>();

        // The cycle goes until it covers all symbols of the binary text vector
        for (int i = 0; i < binaryText.Length; i+= vectorSize)
        {
            // Splitting the vectors and converting them to arrays of numbers
            vectors.Add(binaryText.Skip(i).Take(vectorSize).Select(s => s - '0').ToArray());
        }

        // Getting the last vector in the list
        int[] lastVector = vectors[vectors.Count - 1];

        // Initializing a variable to store padding to be added to the last vector if any
        int padding = 0;

        // If the size of the last vector is smaller than (m + 1), it should be padded
        if (lastVector.Length < vectorSize)
        {
            // Calculating the padding that's needed for the last vector in the list
            padding = vectorSize - lastVector.Length;

            // Initializing a new array to store the padded vector
            int[] paddedVector = new int[vectorSize];

            // For the length of last vector...
            for (int i = 0; i < lastVector.Length; ++i)
            {
                // ... the symbols should remain the same in the padded vector
                paddedVector[i] = lastVector[i];
            }

            // To satisfy the required size
            for (int i = lastVector.Length; i < vectorSize; ++i)
            {
                // ... 0s should be appended to existing symbols
                paddedVector[i] = 0;
            }

            // Replacing the initial last vector with the padded vector
            vectors[vectors.Count - 1] = paddedVector;
        }

        return (vectors, padding);
    }

    // Method used to convert multiple vectors to a single block of text
    public string GetTextFromVectors(List<int[]> vectors, int padding)
    {
        // Joining all of the vectors into a single string of text
        string binaryText = string.Concat(vectors.SelectMany(v => v.Select(v => v.ToString())));

        // Removing the padding that was added when converting text to vectors to match size of (m + 1)
        binaryText = binaryText.Substring(0, binaryText.Length - padding);

        // Initializing a list of bytes to hold UTF8 bytes obtained from conversion
        List<byte> utf8Bytes = new List<byte>();

        // The cycle goes until it covers all symbols of the binary vector
        for (int i = 0; i < binaryText.Length; i += 8)
        {
            // Convering each 8 bits in binary text vector to bytes and adding it to the list
            utf8Bytes.Add(Convert.ToByte(binaryText.Substring(i, 8), 2));
        }

        // Converting bytes into a UTF8 string
        return System.Text.Encoding.UTF8.GetString(utf8Bytes.ToArray());
    }

    // Method used to convert a BMP picture to binary and split to multiple vectors based on parameter m
    public (List<int[]>, int, int, int) GetVectorsFromBMPImage(Bitmap image, int m)
    {
        // Initializing a list to store RGB values for each pixel in the image
        List<int> rgbValues = new List<int>();

        // The cycle goes until it covers the whole height of the image
        for (int y = 0; y < image.Height; ++y)
        {
            // The cycle goes until it covers the whole width of the image
            for (int x = 0; x < image.Width; ++x)
            {
                // Getting the color of the pixel at (x, y) coordinate
                Color pixel = image.GetPixel(x, y);

                // Adding RGB values to the list
                rgbValues.Add(pixel.R);
                rgbValues.Add(pixel.G);
                rgbValues.Add(pixel.B);
            }
        }

        // Converting RGB values (0-255) into binary representation, each takes up 8 characters with padding if needed 
        string binaryImage = string.Concat(rgbValues.Select(v => Convert.ToString(v, 2).PadLeft(8, '0')));

        // Each of the split vectors should be of size (m + 1)
        int vectorSize = m + 1;

        // Initializing a list to store split vectors
        List<int[]> vectors = new List<int[]>();

        // The cycle goes until it covers all symbols of the binary image vector
        for (int i = 0; i < binaryImage.Length; i += vectorSize)
        {
            // Splitting the vectors and converting them to arrays of numbers
            vectors.Add(binaryImage.Skip(i).Take(vectorSize).Select(s => s - '0').ToArray());
        }

        // Getting the last vector in the list
        int[] lastVector = vectors[vectors.Count - 1];

        // Initializing a variable to store padding to be added to the last vector if any
        int padding = 0;

        // If the size of the last vector is smaller than (m + 1), it should be padded
        if (lastVector.Length < vectorSize)
        {
            // Calculating the padding that's needed for the last vector in the list
            padding = vectorSize - lastVector.Length;

            // Initializing a new array to store the padded vector
            int[] paddedVector = new int[vectorSize];

            // For the length of last vector...
            for (int i = 0; i < lastVector.Length; ++i)
            {
                // ... the symbols should remain the same in the padded vector
                paddedVector[i] = lastVector[i];
            }

            // To satisfy the required size
            for (int i = lastVector.Length; i < vectorSize; ++i)
            {
                // ... 0s should be appended to existing symbols
                paddedVector[i] = 0;
            }

            // Replacing the initial last vector with the padded vector
            vectors[vectors.Count - 1] = paddedVector;
        }

        return (vectors, padding, image.Height, image.Width);
    }

    // Method used to convert multiple vectors to a BMP image
    public Bitmap GetBMPImageFromVectors(List<int[]> vectors, int padding, int imageHeight, int imageWidth)
    {
        // Joining all of the vectors into a single string of text
        string binaryImage = string.Concat(vectors.SelectMany(v => v.Select(v => v.ToString())));

        // Removing the padding that was added when converting text to vectors to match size of (m + 1)
        binaryImage = binaryImage.Substring(0, binaryImage.Length - padding);

        // Initializing a list to store RGB values for each pixel in the image
        List<int> rgbValues = new List<int>();

        // The cycle goes until it covers all symbols of the binary vector
        for (int i = 0; i < binaryImage.Length; i += 8)
        {
            // Convering each 8 bits in binary text vector to an RGB value (0-255) and adding it to the list
            rgbValues.Add(Convert.ToInt32(binaryImage.Substring(i, 8), 2));
        }

        // Initializing a new Bitmap image to store the resulting image
        Bitmap image = new Bitmap(imageWidth, imageHeight);

        // The cycle goes until it covers the whole height of the image
        for (int y = 0; y < imageHeight; ++y)
        {
            // The cycle goes until it covers the whole width of the image
            for (int x = 0; x < imageWidth; ++x)
            {
                // The cycle goes until it covers all RGB values from the list
                for (int i = 0; i < rgbValues.Count; i += 3)
                {
                    // Extracting RGB values for pixel at position (x, y)
                    int r = rgbValues[i];
                    int g = rgbValues[i + 1];
                    int b = rgbValues[i + 2];

                    // Creating a color to display in the image
                    Color color = Color.FromArgb(r, g, b);

                    // Setting the color onto the pixel at position (x, y)
                    image.SetPixel(x, y, color);
                }
            }
        }

        return image;
    }
}
