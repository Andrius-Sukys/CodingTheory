using A5.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace A5.UI;

// Control class for Scenario Two
public partial class ScenarioTwoControl : UserControl
{
    // Declaring required services
    private readonly IConverterService _converterService;
    private readonly IDecoderService _decoderService;
    private readonly IEncoderService _encoderService;
    private readonly IMatrixService _matrixService;
    private readonly INoisyChannelService _noisyChannelService;
    private readonly IValidationService _validationService;

    // Initializing a variable to keep track of logging status (enabled/disabled)
    private bool _isLoggingEnabled = false;

    // Initializing variables to store received vectors for cases with and without encoding
    public List<int[]> ReceivedVectorsEncoded = new List<int[]>();
    public List<int[]> ReceivedVectorsNotEncoded = new List<int[]>();

    // Initializing a variable to store sent vector's length
    public int SentVectorLength = 0;

    // Initializing a variable to store padding of the last vector
    public int LastVectorPadding = 0;

    public ScenarioTwoControl()
    {
        // Initializing the window
        InitializeComponent();

        // Initializing the logs text block
        LogsTextBlock.Text = "Logging Disabled.";

        // Initializing required services
        var serviceProvider = ((App)Application.Current).Services;
        _converterService = serviceProvider.GetRequiredService<IConverterService>();
        _decoderService = serviceProvider.GetRequiredService<IDecoderService>();
        _encoderService = serviceProvider.GetRequiredService<IEncoderService>();
        _matrixService = serviceProvider.GetRequiredService<IMatrixService>();
        _noisyChannelService = serviceProvider.GetRequiredService<INoisyChannelService>();
        _validationService = serviceProvider.GetRequiredService<IValidationService>();

        // When "Send" button is clicked...
        SendButton.Click += (_, _) =>
        {
            // Input required to send is validated
            bool isSendInputValid = ValidateSendInput();

            // If the input is valid...
            if (isSendInputValid)
            {
                // Lists of received vectors for both cases with and without encoding are cleared
                ReceivedVectorsEncoded.Clear();
                ReceivedVectorsNotEncoded.Clear();

                // Window elements are reset to clean any marked errors or results
                ResetWindowElements();

                // Vector is sent both with and without encoding
                SendVectors(isEncoded: true);
                SendVectors(isEncoded: false);
            }

            return;
        };

        // When "Decode" button is clicked...
        DecodeButton.Click += (_, _) =>
        {
            // Input required to decode is validated
            bool isReceiveInputValid = ValidateDecodeInput();

            // If the input is valid...
            if (isReceiveInputValid)
            {
                // The encoded vector is decoded
                DecodeVectors(); 
            }

            return;
        };

        // When "Clear Results" button is clicked...
        ClearResultsButton.Click += (_, _) =>
        {
            // Result fields get cleared
            ClearResultFields();
        };

        // When "Enable/Disable Logs" button is clicked...
        ToggleLoggingButton.Click += (_, _) =>
        {
            // Logging status is changed
            _isLoggingEnabled = !_isLoggingEnabled;

            // Text for both the button and the text block of the logs gets changed (wiped)
            ToggleLoggingButton.Content = _isLoggingEnabled ? "Disable Logs" : "Enable Logs";

            if (_isLoggingEnabled)
            {
                LogsTextBlock.Text = "Enabled logging may highly impact performance! ";
                LogsTextBlock.Text += "If you encounter any issues, please disable or clear the logs!\n\n";
                LogsTextBlock.Text += "Started Logging...";
            }
            else
            {
                LogsTextBlock.Text = "Logging Disabled.";
            }
        };

        // When "Clear Logs" button is clicked...
        ClearLogsButton.Click += (_, _) =>
        {
            // The logs get cleared
            ClearLogs();
        };
    }

    // Method used to validate input required to send the vectors representing text
    private bool ValidateSendInput()
    {
        // Checks if the entered text contains at least one symbol
        if (string.IsNullOrWhiteSpace(InputText.Text))
        {
            // If not, the user is notified of the invalid input...
            AppendToLogs("Invalid Input: Input Text. Please enter at least one symbol!", shouldLogWhenDisabled: true);

            // ... and result fields are cleared
            ClearResultFields();

            // The input is not valid
            return false;
        }

        // Checks if the entered parameter m is valid
        if (!_validationService.IsMValid(InputM.Text))
        {
            // If not, the user is notified of the invalid input...
            AppendToLogs("Invalid Input: M. Please enter a positive integer!", shouldLogWhenDisabled: true);

            // ... and result fields are cleared
            ClearResultFields();

            // The input is not valid
            return false;
        }

        // Checks if the entered probability of distortion is valid
        if (!_validationService.IsProbabilityOfDistortionValid(InputProbability.Text.Replace('.', ',')))
        {
            // If not, the user is notified of the invalid input...
            AppendToLogs("Invalid Input: Probability of Distortion. Please enter a number in range of [0; 1]!", shouldLogWhenDisabled: true);

            // ... and result fields are cleared
            ClearResultFields();

            // The input is not valid
            return false;
        }

        // If all the checks above pass, the input is valid
        return true;
    }

    // Method used to reset window elements before sending the vectors
    private void ResetWindowElements()
    {
        EncodedDecodedVectorTextBox.Text = string.Empty;
        EncodedOutputTextTextBox.Text = string.Empty;

        EncodedReceivedVectorTextBox.Foreground = Brushes.Black;

        NotEncodedOutputTextTextBox.Text = string.Empty;
    }

    // Method used to validate input required to decode the encoded vectors
    private bool ValidateDecodeInput()
    {
        // Checks if the vectors have been sent already
        if (string.IsNullOrWhiteSpace(EncodedReceivedVectorTextBox.Text))
        {
            // If not, the user is notified that decoding is not possible without sending
            AppendToLogs("Error: Cannot decode without sending!", shouldLogWhenDisabled: true);

            // The operation is invalid
            return false;
        }

        // Checks if the received (and possibly modified) binary vector is valid
        if (!_validationService.IsBinaryVectorValid(EncodedReceivedVectorTextBox.Text.Replace(" ", "")))
        {
            // If not, the user is notified of the invalid input...
            AppendToLogs("Invalid Input: Input Vector. Please enter a binary (values = 0, 1) vector!", shouldLogWhenDisabled: true);

            // ... and the received binary vector is marked in red, possibly faulty results are wiped
            EncodedReceivedVectorTextBox.Foreground = Brushes.Red;
            EncodedDecodedVectorTextBox.Text = string.Empty;

            // The input is not valid
            return false;
        }

        // Checks if the encoded and received (and possibly modified) vectors match in length 
        if (EncodedEncodedVectorTextBox.Text.Length != EncodedReceivedVectorTextBox.Text.Length)
        {
            // If not, the user is notified of the invalid input...
            AppendToLogs("Invalid Input: Received Vector. Received vector's length must match the sent (encoded) vector's length!", shouldLogWhenDisabled: true);

            // ... and the received binary vector is marked in red, possibly faulty results are wiped
            EncodedReceivedVectorTextBox.Foreground = Brushes.Red;
            EncodedDecodedVectorTextBox.Text = string.Empty;

            // The input is not valid
            return false;
        }

        // If all the checks above pass, the input is valid
        EncodedReceivedVectorTextBox.Foreground = Brushes.Black;
        return true;
    }

    // Method used to send the binary vectors both with and without encoding based on isEncoded value
    private void SendVectors(bool isEncoded)
    {
        try
        {
            // Getting parameter m from the input box, turning it to a number
            int m = int.Parse(InputM.Text.Trim());

            // Getting the length of the vectors to be sent, it shall be (m + 1)
            SentVectorLength = m + 1;

            // Getting the probability of distortion form the input box, turning it to a number
            string inputProbabilitySafe = InputProbability.Text.Replace('.', ',');
            double probabilityOfDistortion = double.Parse(inputProbabilitySafe);

            if (isEncoded) // If the vectors shall be encoded
            {
                // Logs and the result text box are updated
                AppendToLogs($"[E] Input Text: {InputText.Text}");
                EncodedInputTextTextBox.Text = InputText.Text;
            }
            else // If the vectors shall not be encoded
            {
                // Logs and the result text box are updated
                AppendToLogs($"[NE] Input Text: {InputText.Text}");
                NotEncodedInputTextTextBox.Text = InputText.Text;
            }

            // Getting the input vectors by converting text from the input box to a list of arrays of numbers
            (List<int[]> vectors, int padding) = _converterService.GetVectorsFromText(InputText.Text, m);

            // With the input vectors, added padding is also returned and saved
            LastVectorPadding = padding;

            if (isEncoded) // If the vectors shall be encoded
            {
                // Logs and the result text box are updated
                AppendToLogs($"[E] Input Vectors: {string.Join(" ", vectors.Select(i => string.Join("", i)))}");
                EncodedInputVectorTextBox.Text = string.Join(" ", vectors.Select(i => string.Join("", i)));
            }
            else // If the vectors shall not be encoded
            {
                // Logs and the result text box are updated
                AppendToLogs($"[NE] Input Vectors: {string.Join(" ", vectors.Select(i => string.Join("", i)))}");
                NotEncodedInputVectorTextBox.Text = string.Join(" ", vectors.Select(i => string.Join("", i)));
            }
            
            // Initializing a new list to store processed vectors
            List<int[]> processedVectors = new List<int[]>();

            if (isEncoded) // If the vectors shall be encoded
            {
                // Processing every vector in the list of input vectors ...
                foreach (int[] inputVector in vectors)
                {
                    // ... by encoding each one of them and adding to the list of processed vectors
                    processedVectors.Add(_encoderService.Encode(inputVector, _matrixService.GetGeneratorMatrix(m)));
                }
            }
            else // If the vectors shall not be encoded
            {
                // Processed vectors remain the same
                processedVectors = vectors;
            }

            if (isEncoded) // If the vectors are encoded
            {
                // The result text box is updated
                EncodedEncodedVectorTextBox.Text = string.Join(" ", processedVectors.Select(i => string.Join("", i)));
            }
            
            // Working with each vector in the list of processed vectors
            foreach (int[] processedVector in processedVectors)
            {
                if (isEncoded) // If the vector is encoded
                {
                    // Logs are updated
                    AppendToLogs($"[E] Processed Vector: {string.Join("", processedVector)}");
                }
                else // If the vector is not encoded
                {
                    // Logs are updated
                    AppendToLogs($"[NE] Processed Vector: {string.Join("", processedVector)}");
                }

                // Running the vector through the noisy channel and getting the output vector, possibly distorted
                int[] receivedVector = _noisyChannelService.GetOutputVector(processedVector, probabilityOfDistortion);

                if (isEncoded) // If the vector is encoded
                {
                    // Adding the received vector to the list of received vectors (with encoding)
                    ReceivedVectorsEncoded.Add(receivedVector);

                    // Logs and the result text box are updated
                    AppendToLogs($"[E] Received Vector: {string.Join("", receivedVector)}");
                }
                else // If the vector is not encoded
                {
                    // Adding the received vector to the list of received vectors (without encoding)
                    ReceivedVectorsNotEncoded.Add(receivedVector);

                    // Logs and the result text box are updated
                    AppendToLogs($"[NE] Received Vector: {string.Join("", receivedVector)}");
                }

                // Getting the distorted positions by comparing the processed and received vectors in each position
                List<int> distortedPositions = _noisyChannelService.GetDistortedPositions(processedVector, receivedVector);

                // Getting the count of distortions
                int distortedPositionsCount = distortedPositions.Count;

                if (isEncoded && distortedPositions.Any()) // If the vector is encoded and distortions occurred
                {
                    // Logs are updated
                    AppendToLogs($"[E] Distorted Positions ({distortedPositionsCount}): {string.Join(", ", distortedPositions)}");
                }
                else if (distortedPositions.Any()) // If the vector is is not encoded and distortions occurred
                {
                    // Logs are updated
                    AppendToLogs($"[NE] Distorted Positions ({distortedPositionsCount}): {string.Join(", ", distortedPositions)}");
                }
            }

            if (isEncoded) // If the vectors are encoded
            {
                // Result box is updated
                EncodedReceivedVectorTextBox.Text = string.Join(" ", ReceivedVectorsEncoded.Select(v => string.Join("", v)));
            }
            else // If the vectors are not encoded
            {
                // Result box is updated
                NotEncodedReceivedVectorTextBox.Text = string.Join(" ", ReceivedVectorsNotEncoded.Select(v => string.Join("", v)));
            }

            if (!isEncoded) // If the vectors are not encoded
            {
                // The received vectors are turned into a list of arrays of numbers
                ReceivedVectorsNotEncoded = NotEncodedReceivedVectorTextBox.Text.Split(' ').Select(g => g.Select(c => int.Parse(c.ToString())).ToArray()).ToList();

                // Result text box is updated by converting these received vectors back to text
                NotEncodedOutputTextTextBox.Text = _converterService.GetTextFromVectors(ReceivedVectorsNotEncoded, LastVectorPadding);
            }
        }
        catch (Exception ex) // If any unforseen error occurs...
        {
            if (isEncoded) // If the vectors are encoded
            {
                // Logs are updated
                AppendToLogs($"[E] Error: {ex.Message}", shouldLogWhenDisabled: true);
            }
            else // If the vectors are not encoded
            {
                // Logs are updated
                AppendToLogs($"[NE] Error: {ex.Message}", shouldLogWhenDisabled: true);
            }
        }
    }

    // Method used to decode the encoded received binary vectors
    private void DecodeVectors()
    {
        try
        {
            // Initializing a new list to store decoded vectors
            List<int[]> decodedVectors = new List<int[]>();

            // Getting parameter m by deducting 1 from the sent vector's length
            int m = SentVectorLength - 1;

            // The received vectors are turned into a list of arrays of numbers
            ReceivedVectorsEncoded = EncodedReceivedVectorTextBox.Text.Split(' ').Select(g => g.Select(c => int.Parse(c.ToString())).ToArray()).ToList();

            // Working with each vector in the list of received vectors
            foreach (int[] receivedVector in ReceivedVectorsEncoded)
            {
                // Getting needed Hadamard matrices based on parameter m
                List<int[][]> hMatrices = _matrixService.GetHMatrices(m);

                // Getting the decoded vector
                int[] decodedVector = _decoderService.Decode(receivedVector, m, hMatrices);

                // Adding the decoded vector to the list of decoded vectors
                decodedVectors.Add(decodedVector);

                // Logs are updated
                AppendToLogs($"[E] Decoded Vector: {string.Join("", decodedVector)}");
            }

            // Result text box is updated with decoded vectors
            EncodedDecodedVectorTextBox.Text = string.Join(" ", decodedVectors.Select(i => string.Join("", i)));

            // Result text box is updated by converting these decoded vectors back to text
            EncodedOutputTextTextBox.Text = _converterService.GetTextFromVectors(decodedVectors, LastVectorPadding);
        }
        catch (Exception ex) // If any unforseen error occurs...
        {
            // ... logs are updated
            AppendToLogs($"[E] Error: {ex.Message}", shouldLogWhenDisabled: true);
        }
    }

    // Method used to append some text to the existing logs
    private void AppendToLogs(string message, bool shouldLogWhenDisabled = false)
    {
        // If logging is disabled, don't log anything
        if (!_isLoggingEnabled && !shouldLogWhenDisabled) return;

        // Otherwise, append the needed text with the time of the action
        LogsTextBlock.Text += $"\n{DateTime.Now:HH:mm:ss}: {message}";
    }

    // Method used to clear the logs
    private void ClearLogs()
    {
        // Updating the text block of logs based on the status of logging (enabled/disabled)
        if (_isLoggingEnabled)
        {
            LogsTextBlock.Text = "Enabled logging may highly impact performance! ";
            LogsTextBlock.Text += "If you encounter any issues, please disable or clear the logs!\n\n";
            LogsTextBlock.Text += "Started Logging...";
        }
        else
        {
            LogsTextBlock.Text = "Logging Disabled.";
        }
    }

    // Method used to clear all result fields
    private void ClearResultFields()
    {
        // Clearing all text boxes related to encoded vectors
        EncodedInputTextTextBox.Text = string.Empty;
        EncodedInputVectorTextBox.Text = string.Empty;
        EncodedEncodedVectorTextBox.Text = string.Empty;
        EncodedReceivedVectorTextBox.Text = string.Empty;
        EncodedDecodedVectorTextBox.Text = string.Empty;
        EncodedOutputTextTextBox.Text = string.Empty;

        // Clearing all text boxes related to vectors that are not encoded
        NotEncodedInputTextTextBox.Text = string.Empty;
        NotEncodedInputVectorTextBox.Text = string.Empty;
        NotEncodedReceivedVectorTextBox.Text = string.Empty;
        NotEncodedOutputTextTextBox.Text = string.Empty;
    }
}
