namespace A5.Services;

// Service that is dedicated to encoding of vectors

public interface IEncoderService
{
    int[] Encode(int[] inputVector, int[][] generatorMatrix);
}

public class EncoderService : IEncoderService
{
    private readonly IMatrixService _matrixService;

    public EncoderService(IMatrixService matrixService)
    {
        _matrixService = matrixService;
    }

    // Method used to encode the vector by multiplying it by generator matrix and using the binary mode
    public int[] Encode(int[] inputVector, int[][] generatorMatrix) =>
        _matrixService.GetProductOfVectorAndMatrix(inputVector, generatorMatrix, isBinaryCalculation: true);
}
