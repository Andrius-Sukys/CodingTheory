namespace A5.Services;

// Service that is dedicated to all conversion-related operations such as binary to decimal conversion,
// text to vector conversion and vice versa, picture to vector conversion and vice versa

public interface IConverterService
{
    string GetBinaryFromDecimal(int decimalValue, int m);
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
}
