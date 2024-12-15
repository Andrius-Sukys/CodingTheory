using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace A5.Services;

// Service that is dedicated to all conversion-related operations such as binary to decimal conversion,
// text to vector conversion and vice versa, picture to vector conversion and vice versa

public interface IConverterService
{
    string GetBinaryFromDecimal(int decimalValue, int m);

    (List<int[]>, int) GetVectorsFromText(string text, int m);

    string GetTextFromVectors(List<int[]> vectors, int padding);

    (List<int[]>, int, int, int) GetVectorsFromBMPImage(BitmapImage image, int m);

    BitmapImage GetBMPImageFromVectors(List<int[]> vectors, int padding, int imageHeight, int imageWidth);
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

    // Method used to convert text to binary and split to multiple vectors based on parameter m (also returns padding)
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
        for (int i = 0; i < binaryText.Length; i += vectorSize)
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

    // Method used to convert multiple vectors to a single block of text (also takes padding)
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

    // Method used to convert a BMP image to binary vectors (also returns padding and height and width of the image)
    public (List<int[]>, int, int, int) GetVectorsFromBMPImage(BitmapImage bitmapImage, int m)
    {
        // Converting a BitmapImage to Bitmap (needed for pixel manipulation)
        Bitmap bitmap = BitmapImageToBitmap(bitmapImage);

        // Getting the width and height of the image
        int width = bitmap.Width;
        int height = bitmap.Height;

        BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

        int bytesPerPixel = Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
        int stride = bitmapData.Stride;
        IntPtr ptr = bitmapData.Scan0;

        byte[] pixelBuffer = new byte[stride * bitmap.Height];
        System.Runtime.InteropServices.Marshal.Copy(ptr, pixelBuffer, 0, pixelBuffer.Length);
        bitmap.UnlockBits(bitmapData);

        List<int> rgbValues = new List<int>();
        for (int i = 0; i < pixelBuffer.Length; i += bytesPerPixel)
        {
            rgbValues.Add(pixelBuffer[i + 2]); // Red
            rgbValues.Add(pixelBuffer[i + 1]); // Green
            rgbValues.Add(pixelBuffer[i]);     // Blue
        }

        // Convert RGB values to binary
        string binaryImage = string.Concat(rgbValues.Select(v => Convert.ToString(v, 2).PadLeft(8, '0')));

        // Split binary into vectors of size (m + 1)
        int vectorSize = m + 1;
        List<int[]> vectors = new List<int[]>();
        for (int i = 0; i < binaryImage.Length; i += vectorSize)
        {
            vectors.Add(binaryImage.Skip(i).Take(vectorSize).Select(c => c - '0').ToArray());
        }

        int[] lastVector = vectors[vectors.Count - 1];

        // Handle padding for the last vector
        int padding = 0;

        if (lastVector.Length < vectorSize)
        {
            padding = vectorSize - lastVector.Length;

            int[] paddedVector = new int[vectorSize];

            Array.Copy(lastVector, paddedVector, lastVector.Length);

            vectors[^1] = paddedVector;
        }

        return (vectors, padding, height, width);
    }

    // Method used to convert binary vectors to a BMP image (also takes padding and height and width of the image)
    public BitmapImage GetBMPImageFromVectors(List<int[]> vectors, int padding, int imageHeight, int imageWidth)
    {
        // Join vectors and safely remove padding
        string binaryImage = string.Concat(vectors.SelectMany(v => v.Select(i => i.ToString())));

        // Ensure the padding removal doesn't cause out-of-range issues
        if (padding > 0 && padding <= binaryImage.Length)
        {
            binaryImage = binaryImage.Substring(0, binaryImage.Length - padding);
        }

        // Ensure the binary length is divisible by 8
        int remainder = binaryImage.Length % 8;
        if (remainder != 0)
        {
            // Pad the binary string with 0s to make it divisible by 8
            binaryImage = binaryImage.PadRight(binaryImage.Length + (8 - remainder), '0');
        }

        // Convert binary string to RGB values
        List<int> rgbValues = new List<int>();
        for (int i = 0; i < binaryImage.Length; i += 8)
        {
            rgbValues.Add(Convert.ToInt32(binaryImage.Substring(i, 8), 2));
        }

        // Create a bitmap and populate it with RGB values
        Bitmap bitmap = new Bitmap(imageWidth, imageHeight);
        int index = 0;

        for (int y = 0; y < imageHeight; y++)
        {
            for (int x = 0; x < imageWidth; x++)
            {
                if (index + 2 < rgbValues.Count)
                {
                    // Extract RGB values and set the pixel color
                    Color color = Color.FromArgb(rgbValues[index], rgbValues[index + 1], rgbValues[index + 2]);
                    bitmap.SetPixel(x, y, color);
                    index += 3;
                }
                else
                {
                    // Fill remaining pixels with black
                    bitmap.SetPixel(x, y, Color.Black);
                }
            }
        }

        return BitmapToBitmapImage(bitmap);
    }

    private Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
    {
        using MemoryStream outStream = new();
        BitmapEncoder encoder = new BmpBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
        encoder.Save(outStream);

        outStream.Position = 0;
        return new Bitmap(outStream);
    }

    private BitmapImage BitmapToBitmapImage(Bitmap bitmap)
    {
        using MemoryStream memory = new();
        bitmap.Save(memory, ImageFormat.Bmp);
        memory.Position = 0;

        BitmapImage bitmapImage = new();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = memory;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.EndInit();

        return bitmapImage;
    }
}