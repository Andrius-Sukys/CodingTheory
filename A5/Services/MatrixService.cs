using A5.Repositories;

namespace A5.Services;

// Service that is dedicated to all matrix-related operations

public interface IMatrixService
{
    int[][] GetGeneratorMatrix(int m);

    int[] GetProductOfVectorAndMatrix(int[] vector, int[][] matrix, bool isBinaryCalculation = false);

    List<int[][]> GetHMatrices(int m);

    int[][] GetKroneckerProduct(int[][] A, int[][] B);
}

public class MatrixService : IMatrixService
{
    private readonly IMatrixRepository _matrixRepository;

    public MatrixService(IMatrixRepository matrixRepository)
    {
        _matrixRepository = matrixRepository;
    }

    // Method used to get generator matrix needed for encoding, based on parameter m
    public int[][] GetGeneratorMatrix(int m)
    {
        // Assembling a key for this generator matrix based on m
        string key = $"GeneratorMatrix_{m}";

        // If needed matrix has been calculated previously, it is in the repository
        if (_matrixRepository.ContainsMatrix(key))
        {
            // If it is in the repository, it is taken from there
            return _matrixRepository.GetMatrix(key)!;
        }

        // Count of columns for generator matrix is (2^m)
        int countOfColumns = (int)Math.Pow(2, m);

        // Count of rows for generator matrix is (m + 1)
        int countOfRows = m + 1;

        // Initializing a new matrix that has (m + 1) rows
        int[][] matrix = new int[countOfRows][];

        // Working with each row of the matrix
        for (int i = 0; i < countOfRows; ++i) 
        {
            // Each row has (2^m) columns
            matrix[i] = new int[countOfColumns]; 

            // Each row is generated based on a pattern of 0s and 1s comprised of (2^i) symbols
            int patternLength = (int)Math.Pow(2, i); 

            // As left side of the patterns should be filled with 0s and the right side with 1s, it's useful to split the pattern in two
            int halfPatternLength = patternLength / 2;

            // Filling each column of the current row i with a value of 0 or 1
            for (int j = 0; j < countOfColumns; ++j)
            {
                // If inside the pattern block the position is on the left side, the value shall be 0, otherwise 1
                matrix[i][j] = ((j % patternLength) < halfPatternLength) ? 0 : 1;
            }
        }

        // Newly calculated matrix gets saved in the repository so there's no need to recalculate
        _matrixRepository.SaveMatrix(key, matrix);
        return matrix;
    }

    // Method used to get the product of vector and matrix, can be either in binary or standard mode
    public int[] GetProductOfVectorAndMatrix(int[] vector, int[][] matrix, bool isBinaryCalculation = false)
    {
        // Getting the count of columns of the input matrix
        int countOfColumnsMatrix = matrix[0].Length;

        // Getting the length of the input vector
        int countOfColumnsVector = vector.Length;

        // Initializing a vector for result that has the same count of columns as the input matrix does
        int[] result = new int[countOfColumnsMatrix];

        // Calculation is performed using all columns of the matrix
        for (int i = 0; i < countOfColumnsMatrix; ++i)
        {
            // Initializing an aggregate variable that is used to store the sum of products for the current column
            int sum = 0;

            // Calculation is performed using all members of the vector
            for (int j = 0; j < countOfColumnsVector; ++j)
            {
                // Adding the product of vector's value and matrix's value to the aggregate sum
                sum += vector[j] * matrix[j][i];
            }

            // If the calculations are performed in binary mode, modulus of 2 is applied to the sum
            if (isBinaryCalculation)
            {
                sum = sum % 2;
            }

            // Putting sum as the value for the current position of the resulting vector
            result[i] = sum;
        }

        return result;
    }

    // Method used to get so-called H-matrices used when calculating Kronecker product at decoding stage
    public List<int[][]> GetHMatrices(int m)
    {
        // Assembling a key for this set of H-matrices based on m
        string key = $"HMatrices_{m}";

        // If needed matrix set has been calculated previously, it is in the repository
        if (_matrixRepository.ContainsMatrices(key))
        {
            // If it is in the repository, it is taken from there
            return _matrixRepository.GetMatrices(key)!;
        }

        // Initializing a new list of 2D matrices to store H-matrices
        List<int[][]> matrices = new List<int[][]>();

        // Initializing matrix H that has constant values that are known
        int[][] H =
        [
            [1, 1],
            [1, -1]
        ];

        // Starting at (i = 1), H-matrices are generated until (i = m)
        for (int i = 1; i <= m; ++i)
        {
            // Both left-side and right-side of the H-matrices calculation formula uses identity matrices with H in the middle

            // Getting the left-side identity matrix that is of size (2^(m - i))
            int[][] identityMatrixLeft = GetIdentityMatrix((int)Math.Pow(2, m - i));

            // Getting the right-side identity matrix that is of size (2^(i - 1))
            int[][] identityMatrixRight = GetIdentityMatrix((int)Math.Pow(2, i - 1));

            // Getting the H-matrix formula using identity matrices and H with current i
            int[][] hMatrix = GetKroneckerProduct(GetKroneckerProduct(identityMatrixLeft, H), identityMatrixRight);

            // Adding the calculated H-matrix to the list of H-matrices
            matrices.Add(hMatrix);
        }

        // Newly calculated matrices get saved in the repository so there's no need to recalculate
        _matrixRepository.SaveMatrices(key, matrices);
        return matrices;
    }

    // Method used to get an identity matrix of requested size
    private int[][] GetIdentityMatrix(int size)
    {
        // Assembling a key for this identity matrix based on m
        string key = $"IdentityMatrix_{size}";

        // If needed matrix has been calculated previously, it is in the repository
        if (_matrixRepository.ContainsMatrix(key))
        {
            // If it is in the repository, it is taken from there
            return _matrixRepository.GetMatrix(key)!;
        }

        // Initializing a new matrix that has rows of requested size
        int[][] matrix = new int[size][];

        // Working with the whole size of the matrix
        for (int i = 0; i < size; ++i)
        {
            // Each row has columns of the same requested size
            matrix[i] = new int[size];

            // If column and row positions match, the value should be 1
            matrix[i][i] = 1;

            // Otherwise the value remains at 0 which was determined on initialization
        }

        // Newly calculated matrix gets saved in the repository so there's no need to recalculate
        _matrixRepository.SaveMatrix(key, matrix);
        return matrix;
    }

    // Method used to get Kronecker product of two matrices A and B
    public int[][] GetKroneckerProduct(int[][] A, int[][] B)
    {
        // Getting count of rows and columns for matrix A
        int countOfRowsA = A.Length;
        int countOfColumnsA = A[0].Length;

        // Getting count of rows and columns for matrix B
        int countOfRowsB = B.Length;
        int countOfColumnsB = B[0].Length;

        // Calculating the count of rows for Kronecker product
        int countOfRowsKroneckerProduct = countOfRowsA * countOfRowsB;

        // Calculating the count of columns for Kronecker product
        int countOfColumnsKroneckerProduct = countOfColumnsA * countOfColumnsB;

        return GenerateKroneckerProduct(A, B, countOfRowsKroneckerProduct, countOfColumnsKroneckerProduct);
    }
    
    // Method used to generate Kronecker product for matrices A and B
    private int[][] GenerateKroneckerProduct(int[][] A, int[][] B, int resultRows, int resultColumns)
    {
        // Initializing a new matrix to store results
        int[][] result = new int[resultRows][];

        // Working with each row of the resulting matrix
        for (int i = 0; i < resultRows; ++i)
        {
            // Each row has a known number of columns
            result[i] = new int[resultColumns];
        }

        // Working with each row of the matrix A
        for (int i = 0; i < A.Length; ++i)
        {
            // Working with each column of the matrix A
            for (int j = 0; j < A[0].Length; ++j)
            {
                // Updating the result by merging the calculated submatrix
                // Here i represents the row of the element (block in Kronecker product) currently worked within
                // Here j represents the column of the element (block in the Kronecker product) currently worked within
                result = MergeSubMatrix(result, B, i, j, A[i][j]);
            }
        }

        return result;
    }

    // Method used to merge existing and new result onto the Kronecker product matrix
    private int[][] MergeSubMatrix(int[][] result, int[][] B, int blockRow, int blockColumn, int valueFromA)
    {
        // Getting count of rows and columns for matrix B
        int countOfRowsB = B.Length;
        int countOfColumnsB = B[0].Length;

        // Working with each row of the matrix B
        for (int i = 0; i < countOfRowsB; ++i)
        {
            // Working with each column of the matrix B
            for (int j = 0; j < countOfColumnsB; ++j)
            {
                // Calculating which result row to update by offsetting it by the block number (based element's position in matrix A)
                // multiplied by count of rows inside a block (equal to rows in matrix B) and adding the position inside the given block i
                int resultRow = blockRow * countOfRowsB + i;

                // Calculating which result column to update by offsetting it by the block number (based on element's position in matrix B)
                // multiplied by count of columns inside a block (equal to columns in matrix B) and adding the position inside the given block j
                int resultColumn = blockColumn * countOfColumnsB + j;

                // Updating the result for calculated row and column with product of value from matrix A and element from B matrix
                result[resultRow][resultColumn] = valueFromA * B[i][j];
            }
        }

        return result;
    }
}
