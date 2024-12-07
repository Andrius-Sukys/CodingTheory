namespace A5.Repositories;

// Repository that is dedicated to saving all calculated matrices to they don't have to be recalculated again

public interface IMatrixRepository
{
    void SaveMatrix(string key, int[][] matrix);

    int[][]? GetMatrix(string key);

    bool ContainsMatrix(string key);

    void SaveMatrices(string key, List<int[][]> matrices);

    List<int[][]>? GetMatrices(string key);

    bool ContainsMatrices(string key);
}

public class MatrixRepository : IMatrixRepository
{
    // Dictionaries store calculated matrices or lists of matrices
    // Compatible with generator matrices, H-matrices and identity matrices
    private readonly Dictionary<string, int[][]> _singleMatrices = new();
    private readonly Dictionary<string, List<int[][]>> _listOfMatrices = new();

    // Method used to save a single matrix to the dictionary storage
    public void SaveMatrix(string key, int[][] matrix)
    {
        // If dictionary does not contain a matrix that has a certain key...
        if (!_singleMatrices.ContainsKey(key))
        {
            // ... such matrix gets added to the storage
            _singleMatrices[key] = matrix;
        }
    }

    // Method used to get a single matrix from the dictionaty storage
    public int[][]? GetMatrix(string key) =>
        _singleMatrices.TryGetValue(key, out var matrix) ? matrix : null;

    // Method used to check if dictionary storage contains a needed matrix
    public bool ContainsMatrix(string key) =>
        _singleMatrices.ContainsKey(key);

    // Method used to save a list of matrices to the dictionary storage
    public void SaveMatrices(string key, List<int[][]> matrices)
    {
        // If dictionary does not contain a list of matrices that has a certain key...
        if (!_listOfMatrices.ContainsKey(key))
        {
            // ... such list of matrices gets added to the storage
            _listOfMatrices[key] = matrices;
        }
    }

    // Method used to get a list of matrices from the dictionaty storage
    public List<int[][]>? GetMatrices(string key) =>
        _listOfMatrices.TryGetValue(key,out var matrices) ? matrices : null;

    // Method used to check if dictionary storage contains a needed list of matrices
    public bool ContainsMatrices(string key) =>
        _listOfMatrices.ContainsKey(key);
}