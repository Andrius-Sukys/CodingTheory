namespace A5.Services;

// Service that is dedicated to noisy channel simulation

public interface INoisyChannelService
{
    int[] GetOutputVector(int[] inputVector, double probabilityOfDistortion);
}

public class NoisyChannelService : INoisyChannelService
{
    // Declaring seed used for random number generator initialization
    private readonly int _seed;

    // Declaring random number generator
    private readonly Random _randomNumberGenerator;

    public NoisyChannelService()
    {
        // Taking current time in form of ticks to use as the seed for random number generator
        _seed = Environment.TickCount;

        // Initializing the random number generator
        _randomNumberGenerator = new Random(_seed);
    }

    // Method used to run the input vector through the noisy channel that may distort it and get the output vector
    public int[] GetOutputVector(int[] inputVector, double probabilityOfDistortion)
    {
        // Initializing output vector that is the same in length as the input vector
        int[] outputVector = new int[inputVector.Length];

        // Working with every bit present in the input vector
        for (int i = 0; i < inputVector.Length; ++i)
        {
            // If the bit shall be distorted...
            if (ShouldDistortCurrentBit(probabilityOfDistortion))
            {
                // ... then if bit is 0 it gets changed to 1 and vice versa
                outputVector[i] = inputVector[i] == 0 ? 1 : 0;
            }
            else
            {
                // Otherwise the bit is kept as it is in the input vector
                outputVector[i] = inputVector[i];
            }
        }

        return outputVector;
    }

    // Method used to determine whether a bit should be distorted (changed, i.e. 0 -> 1, 1 -> 0) or not
    private bool ShouldDistortCurrentBit(double probabilityOfDistortion)
    {
        // Generating a random double value and adding epsilon to make the possible range [0; 1]
        double randomValue = (_randomNumberGenerator.NextDouble() * (1.0 + double.Epsilon));

        // Determining if the bit should be distorted by comparing generated value to probability of distortion
        // Bit shall be distorted if the generated value is less than the probability of distortion
        bool shouldDistort = randomValue < probabilityOfDistortion;

        return shouldDistort;
    }
}
