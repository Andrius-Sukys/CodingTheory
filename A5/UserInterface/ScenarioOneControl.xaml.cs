using A5.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace A5.UI;

public partial class ScenarioOneControl : UserControl
{
    private readonly IDecoderService _decoderService;
    private readonly IEncoderService _encoderService;
    private readonly IMatrixService _matrixService;
    private readonly INoisyChannelService _noisyChannelService;
    private readonly IValidationService _validationService;

    public ScenarioOneControl()
    {
        InitializeComponent();

        LogsTextBlock.Text = "Started Logging...";

        var serviceProvider = ((App)Application.Current).Services;

        _decoderService = serviceProvider.GetRequiredService<IDecoderService>();
        _encoderService = serviceProvider.GetRequiredService<IEncoderService>();
        _matrixService = serviceProvider.GetRequiredService<IMatrixService>();
        _noisyChannelService = serviceProvider.GetRequiredService<INoisyChannelService>();
        _validationService = serviceProvider.GetRequiredService<IValidationService>();

        SendButton.Click += (_, _) =>
        {
            bool isSendInputValid = ValidateSendInput();

            if (isSendInputValid)
            {
                ResetWindowElements();
                SendVector(isEncoded: true);
                SendVector(isEncoded: false);
            }

            return;
        };

        DecodeButton.Click += (_, _) =>
        {
            bool isReceiveInputValid = ValidateDecodeInput();

            if (isReceiveInputValid)
            {
                DecodeVector();
            }

            return;
        };

        ClearLogsButton.Click += (_, _) =>
        {
            ClearLogs();
        };
    }

    private bool ValidateSendInput()
    {
        if (!_validationService.IsMValid(InputM.Text))
        {
            AppendToLogs("Invalid Input: M. Please enter a positive integer!");
            ClearResultFields();
            return false;
        }

        var inputProbabilitySafe = InputProbability.Text.Replace('.', ',');

        if (!_validationService.IsProbabilityOfDistortionValid(inputProbabilitySafe))
        {
            AppendToLogs("Invalid Input: Probability of Distortion. Please enter a number in range of [0; 1]!");
            ClearResultFields();
            return false;
        }

        if (!_validationService.IsBinaryVectorValid(InputVector.Text))
        {
            AppendToLogs("Invalid Input: Input Vector. Please enter a binary (values = 0, 1) vector!");
            ClearResultFields();
            return false;
        }

        if (!_validationService.IsInputVectorSizeValid(InputVector.Text, InputM.Text))
        {
            AppendToLogs("Invalid Input: Input vector must be of size (m + 1)!");
            ClearResultFields();
            return false;
        }

        return true;
    }

    private void ResetWindowElements()
    {
        EncodedDecodedVectorTextBox.Text = string.Empty;
        EncodedReceivedVectorTextBox.Foreground = Brushes.Black;
    }

    private bool ValidateDecodeInput()
    {
        if (string.IsNullOrWhiteSpace(EncodedReceivedVectorTextBox.Text))
        {
            AppendToLogs("Error: Cannot decode without sending!");
            return false;
        }

        if (!_validationService.IsBinaryVectorValid(EncodedReceivedVectorTextBox.Text))
        {
            AppendToLogs("Invalid Input: Input Vector. Please enter a binary (values = 0, 1) vector!");
            EncodedReceivedVectorTextBox.Foreground = Brushes.Red;
            EncodedDecodedVectorTextBox.Text = string.Empty;
            return false;
        }

        if (EncodedEncodedVectorTextBox.Text.Length != EncodedReceivedVectorTextBox.Text.Length)
        {
            AppendToLogs("Invalid Input: Received Vector. Received vector's length must match the sent (encoded) vector's length!");
            EncodedReceivedVectorTextBox.Foreground = Brushes.Red;
            EncodedDecodedVectorTextBox.Text = string.Empty;
            return false;
        }

        if (!_validationService.IsMValid(InputM.Text))
        {
            AppendToLogs("Invalid Input: M. Please enter a positive integer!");
            EncodedReceivedVectorTextBox.Foreground = Brushes.Red;
            EncodedDecodedVectorTextBox.Text = string.Empty;
            return false;
        }

        if (!_validationService.IsReceivedVectorSizeValid(EncodedReceivedVectorTextBox.Text, InputM.Text))
        {
            AppendToLogs("Invalid Input: Received vector must be of size (2^m)!");
            EncodedReceivedVectorTextBox.Foreground = Brushes.Red;
            EncodedDecodedVectorTextBox.Text = string.Empty;
            return false;
        }

        EncodedReceivedVectorTextBox.Foreground = Brushes.Black;
        return true;
    }

    private void SendVector(bool isEncoded)
    {
        try
        {
            int m = int.Parse(InputM.Text.Trim());

            string inputProbabilitySafe = InputProbability.Text.Replace('.', ',');
            double probabilityOfDistortion = double.Parse(inputProbabilitySafe);

            int[] inputVector = InputVector.Text.Trim().Select(s => s - '0').ToArray();

            if (isEncoded)
            {
                AppendToLogs($"[E] Input Vector: {string.Join("", inputVector)}");
                EncodedInputVectorTextBox.Text = string.Join("", inputVector);
            }
            else
            {
                AppendToLogs($"[NE] Input Vector: {string.Join("", inputVector)}");
                NotEncodedInputVectorTextBox.Text = string.Join("", inputVector);
            }

            int[] processedVector = isEncoded
                ? _encoderService.Encode(inputVector, _matrixService.GetGeneratorMatrix(m))
                : inputVector;

            if (isEncoded)
            {
                AppendToLogs($"[E] Processed Vector: {string.Join("", processedVector)}");
                EncodedEncodedVectorTextBox.Text = string.Join("", processedVector);
            }
            else
            {
                AppendToLogs($"[NE] Processed Vector: {string.Join("", processedVector)}");
            }

            int[] receivedVector = _noisyChannelService.GetOutputVector(processedVector, probabilityOfDistortion);

            if (isEncoded)
            {
                AppendToLogs($"[E] Received Vector: {string.Join("", receivedVector)}");
                EncodedReceivedVectorTextBox.Text = string.Join("", receivedVector);
            }
            else
            {
                AppendToLogs($"[NE] Received Vector: {string.Join("", receivedVector)}");
                NotEncodedReceivedVectorTextBox.Text = string.Join("", receivedVector);
            }

            List<int> distortedPositions = _noisyChannelService.GetDistortedPositions(processedVector, receivedVector);
            int distortedPositionsCount = distortedPositions.Count;

            if (isEncoded && distortedPositions.Any())
            {
                AppendToLogs($"[E] Distorted Positions ({distortedPositionsCount}): {string.Join(", ", distortedPositions)}");
            }
            else if (distortedPositions.Any())
            {
                AppendToLogs($"[NE] Distorted Positions ({distortedPositionsCount}): {string.Join(", ", distortedPositions)}");
            }
        }
        catch (Exception ex)
        {
            if (isEncoded)
            {
                AppendToLogs($"[E] Error: {ex.Message}");
            }
            else
            {
                AppendToLogs($"[NE] Error: {ex.Message}");
            }
        }
    }

    private void DecodeVector()
    {
        try
        {
            int m = EncodedInputVectorTextBox.Text.Length - 1;
            List<int[][]> hMatrices = _matrixService.GetHMatrices(m);

            int[] receivedVector = EncodedReceivedVectorTextBox.Text.Select(s => s - '0').ToArray();

            int[] decodedVector = _decoderService.Decode(receivedVector, m, hMatrices);

            AppendToLogs($"[E] Decoded Vector: {string.Join("", decodedVector)}");
            EncodedDecodedVectorTextBox.Text = string.Join("", decodedVector);
        }
        catch (Exception ex)
        {
            AppendToLogs($"[E] Error: {ex.Message}");
        }
    }

    private void AppendToLogs(string message)
    {
        LogsTextBlock.Text += $"\n{DateTime.Now:HH:mm:ss}: {message}";
    }

    private void ClearLogs()
    {
        LogsTextBlock.Text = "Started Logging...";
    }

    private void ClearResultFields()
    {
        EncodedInputVectorTextBox.Text = string.Empty;
        EncodedEncodedVectorTextBox.Text = string.Empty;
        EncodedReceivedVectorTextBox.Text = string.Empty;
        EncodedDecodedVectorTextBox.Text = string.Empty;

        NotEncodedInputVectorTextBox.Text = string.Empty;
        NotEncodedReceivedVectorTextBox.Text = string.Empty;
    }
}
