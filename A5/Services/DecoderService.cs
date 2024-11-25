namespace A5.Services;

// Service that is dedicated to decoding of vectors

public interface IDecoderService
{
    int[] Decode(int[] receivedVector, int m, List<int[][]> hMatrices);
}

public class DecoderService : IDecoderService
{
    private readonly IMatrixService _matrixService;
    private readonly IConverterService _converterService;

    public DecoderService(IMatrixService matrixService, IConverterService converterService)
    {
        _matrixService = matrixService;
        _converterService = converterService;
    }

    // Method used to decode the vector by changing 0s to -1s, calculating w-vectors, getting the position of the
    // biggest absolute value, turning that position into reversed binary sequence and adding sign bit in front
    public int[] Decode(int[] receivedVector, int m, List<int[][]> hMatrices)
    {
        // Replacing 0s with -1s in the received vector
        int[] wChanged = ReplaceZeroWithMinusOne(receivedVector);

        // Initializing a variable to calculate w-vectors with the initial value of the changed received vector
        int[] currentW = wChanged;
        
        // Working with every H-matrix that is passed
        for (int i = 0; i < m; ++i)
        {
            // Replacing the old w-vector value with the newly calculated w-vector by getting
            // the product of current w-vector and H-matrix i
            currentW = _matrixService.GetProductOfVectorAndMatrix(currentW, hMatrices[i]);
        }

        // Getting the biggest absolute value
        int maxAbsoluteValue = currentW.Max(Math.Abs);

        // Matching all the elements in the last w-vector to find the value and its actual sign
        int trueMaxAbsoluteValue = currentW.First(v => Math.Abs(v) == maxAbsoluteValue);

        // Getting the position (index) of the true biggest (or smallest) value
        int maxAbsoluteValuePosition = Array.IndexOf(currentW, trueMaxAbsoluteValue);

        // Getting the position number's binary representation
        string positionInBinary = _converterService.GetBinaryFromDecimal(maxAbsoluteValuePosition, m);

        // Reversing the bits of position's number in binary
        string positionInBinaryReversed = new string(positionInBinary.ToArray().Reverse().ToArray());

        // Getting the sign bit based on the value of the true biggest (or smallest) value
        string signBit = trueMaxAbsoluteValue > 0 ? "1" : "0";

        // Getting the whole decoded string comprised of sign bit and reversed position's number in binary
        string decodedString = signBit + positionInBinaryReversed;

        // Decoded string gets converted to numbers instead of text
        return decodedString.Select(s => s - '0').ToArray();
    }

    // Method used to replace 0 with -1 in the received vector
    private int[] ReplaceZeroWithMinusOne(int[] receivedVector) =>
        receivedVector.Select(s => s == 0 ? -1 : s).ToArray();
}
