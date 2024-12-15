using A5.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace A5.UI;

// Control class for Scenario One
public partial class ScenarioOneControl : UserControl
{
    // Declaring required services
    private readonly IDecoderService _decoderService;
    private readonly IEncoderService _encoderService;
    private readonly IMatrixService _matrixService;
    private readonly INoisyChannelService _noisyChannelService;
    private readonly IValidationService _validationService;

    // Initializing a variable to keep track of logging status (enabled/disabled)
    private bool _isLoggingEnabled = true;

    public ScenarioOneControl()
    {
        // Initializing the window
        InitializeComponent();

        // Initializing the logs text block
        LogsTextBlock.Text = "Started Logging...";

        // Initializing required services
        var serviceProvider = ((App)Application.Current).Services;
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
                // Window elements are reset to clean any marked errors or results
                ResetWindowElements();

                // Vector is sent both with and without encoding
                SendVector(isEncoded: true);
                SendVector(isEncoded: false);
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
                DecodeVector();
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
            LogsTextBlock.Text = _isLoggingEnabled ? "Started Logging..." : "Logging Disabled.";
        };

        // When "Clear Logs" button is clicked...
        ClearLogsButton.Click += (_, _) =>
        {
            // The logs get cleared
            ClearLogs();
        };
    }

    // Method used to validate input required to send the vector
    private bool ValidateSendInput()
    {
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

        // Checks if the entered binary vector is valid
        if (!_validationService.IsBinaryVectorValid(InputVector.Text))
        {
            // If not, the user is notified of the invalid input...
            AppendToLogs("Invalid Input: Input Vector. Please enter a binary (values = 0, 1) vector!", shouldLogWhenDisabled: true);

            // ... and result fields are cleared
            ClearResultFields();

            // The input is not valid
            return false;
        }

        // Checks if the size of the entered binary vector is (m + 1)
        if (!_validationService.IsInputVectorSizeValid(InputVector.Text, InputM.Text))
        {
            // If not, the user is notified of the invalid input...
            AppendToLogs("Invalid Input: Input vector must be of size (m + 1)!", shouldLogWhenDisabled: true);

            // ... and result fields are cleared
            ClearResultFields();

            // The input is not valid
            return false;
        }

        // If all the checks above pass, the input is valid
        return true;
    }

    // Method used to reset window elements before sending the vector
    private void ResetWindowElements()
    {
        EncodedDecodedVectorTextBox.Text = string.Empty;
        EncodedReceivedVectorTextBox.Foreground = Brushes.Black;
    }

    // Method used to validate input required to decode the encoded vector
    private bool ValidateDecodeInput()
    {
        // Checks if the vector has been sent already
        if (string.IsNullOrWhiteSpace(EncodedReceivedVectorTextBox.Text))
        {
            // If not, the user is notified that decoding is not possible without sending
            AppendToLogs("Error: Cannot decode without sending!", shouldLogWhenDisabled: true);

            // The operation is invalid
            return false;
        }

        // Checks if the received (and possibly modified) binary vector is valid
        if (!_validationService.IsBinaryVectorValid(EncodedReceivedVectorTextBox.Text))
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

        // Checks if the entered M is valid
        if (!_validationService.IsMValid(InputM.Text))
        {
            // If not, the user is notified of the invalid input...
            AppendToLogs("Invalid Input: M. Please enter a positive integer!", shouldLogWhenDisabled: true);

            // Possibly faulty results are wiped
            EncodedDecodedVectorTextBox.Text = string.Empty;

            // The input is not valid
            return false;
        }

        // Checks if the received (and possibly modified) vector's length aligns with parameter m
        if (!_validationService.IsReceivedVectorSizeValid(EncodedReceivedVectorTextBox.Text, InputM.Text))
        {
            // If not, the user is notified of the invalid input...
            AppendToLogs("Invalid Input: Received vector must be of size (2^m)!", shouldLogWhenDisabled: true);

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

    // Method used to send the binary vector both with and without encoding based on isEncoded value
    private void SendVector(bool isEncoded)
    {
        try
        {
            // Getting parameter m from the input box, turning it to a number
            int m = int.Parse(InputM.Text.Trim());

            // Getting the probability of distortion form the input box, turning it to a number
            string inputProbabilitySafe = InputProbability.Text.Replace('.', ',');
            double probabilityOfDistortion = double.Parse(inputProbabilitySafe);

            // Getting the input vector from the input box, turning it to an array of numbers
            int[] inputVector = InputVector.Text.Trim().Select(s => s - '0').ToArray();

            if (isEncoded) // If the vector shall be encoded
            {
                // Logs and the result text box are updated
                AppendToLogs($"[E] Input Vector: {string.Join("", inputVector)}");
                EncodedInputVectorTextBox.Text = string.Join("", inputVector);
            }
            else // If the vector shall not be encoded
            {
                // Logs and the result text box are updated
                AppendToLogs($"[NE] Input Vector: {string.Join("", inputVector)}");
                NotEncodedInputVectorTextBox.Text = string.Join("", inputVector);
            }

            // Getting the processed vector - it's either an encoded vector or just the input vector
            // Processing depends on whether the vector shall be encoded or not
            int[] processedVector = isEncoded
                ? _encoderService.Encode(inputVector, _matrixService.GetGeneratorMatrix(m))
                : inputVector;

            if (isEncoded) // If the vector is encoded
            {
                // Logs and the result text box are updated
                AppendToLogs($"[E] Processed Vector: {string.Join("", processedVector)}");
                EncodedEncodedVectorTextBox.Text = string.Join("", processedVector);
            }
            else // If the vector is not encoded
            {
                AppendToLogs($"[NE] Processed Vector: {string.Join("", processedVector)}");
            }

            // Running the vector through the noisy channel and getting the output vector, possibly distorted
            int[] receivedVector = _noisyChannelService.GetOutputVector(processedVector, probabilityOfDistortion);

            if (isEncoded) // If the vector is encoded
            {
                // Logs and the result text box are updated
                AppendToLogs($"[E] Received Vector: {string.Join("", receivedVector)}");
                EncodedReceivedVectorTextBox.Text = string.Join("", receivedVector);
            }
            else // If the vector is not encoded
            {
                // Logs and the result text box are updated
                AppendToLogs($"[NE] Received Vector: {string.Join("", receivedVector)}");
                NotEncodedReceivedVectorTextBox.Text = string.Join("", receivedVector);
            }

            // Getting the distorted positions by comparing the processed and received vectors in each position
            List<int> distortedPositions = _noisyChannelService.GetDistortedPositions(processedVector, receivedVector);

            // Getting the count of distortions
            int distortedPositionsCount = distortedPositions.Count;

            if (isEncoded && distortedPositions.Any())  // If the vector is encoded and distortions occurred
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
        catch (Exception ex) // If any unforseen error occurs...
        {
            if (isEncoded) // If the vector is encoded
            {
                // Logs are updated
                AppendToLogs($"[E] Error: {ex.Message}", shouldLogWhenDisabled: true);
            }
            else // If the vector is not encoded
            {
                // Logs are updated
                AppendToLogs($"[NE] Error: {ex.Message}", shouldLogWhenDisabled: true);
            }
        }
    }

    // Method used to decode the encoded received binary vector
    private void DecodeVector()
    {
        try
        {
            // Getting parameter m by deducting 1 from the encoded vector's length
            int m = EncodedInputVectorTextBox.Text.Length - 1;

            // Getting needed Hadamard matrices based on parameter m
            List<int[][]> hMatrices = _matrixService.GetHMatrices(m);

            // Getting the received vector from the input box, turning it to an array of numbers
            int[] receivedVector = EncodedReceivedVectorTextBox.Text.Select(s => s - '0').ToArray();

            // Getting the decoded vector
            int[] decodedVector = _decoderService.Decode(receivedVector, m, hMatrices);

            // Logs and the result text box are updated
            AppendToLogs($"[E] Decoded Vector: {string.Join("", decodedVector)}");
            EncodedDecodedVectorTextBox.Text = string.Join("", decodedVector);
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
        LogsTextBlock.Text = _isLoggingEnabled ? "Started Logging..." : "Logging Disabled.";
    }

    // Method used to clear all result fields
    private void ClearResultFields()
    {
        // Clearing all text boxes related to encoded vector
        EncodedInputVectorTextBox.Text = string.Empty;
        EncodedEncodedVectorTextBox.Text = string.Empty;
        EncodedReceivedVectorTextBox.Text = string.Empty;
        EncodedDecodedVectorTextBox.Text = string.Empty;

        // Clearing all text boxes related to vector that's not encoded
        NotEncodedInputVectorTextBox.Text = string.Empty;
        NotEncodedReceivedVectorTextBox.Text = string.Empty;
    }
}
