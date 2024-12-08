using A5.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace A5.UI;

public partial class ScenarioTwoControl : UserControl
{
    private readonly IConverterService _converterService;
    private readonly IDecoderService _decoderService;
    private readonly IEncoderService _encoderService;
    private readonly IMatrixService _matrixService;
    private readonly INoisyChannelService _noisyChannelService;
    private readonly IValidationService _validationService;

    public List<int[]> ReceivedVectorsEncoded = new List<int[]>();
    public List<int[]> ReceivedVectorsNotEncoded = new List<int[]>();

    public int SentVectorLength = 0;
    public int LastVectorPadding = 0;

    public ScenarioTwoControl()
    {
        InitializeComponent();

        LogsTextBlock.Text = "Started Logging...";

        var serviceProvider = ((App)Application.Current).Services;

        _converterService = serviceProvider.GetRequiredService<IConverterService>();
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
                ReceivedVectorsEncoded.Clear();
                ReceivedVectorsNotEncoded.Clear();

                ResetWindowElements();
                SendVectors(isEncoded: true);
                SendVectors(isEncoded: false);
            }

            return;
        };

        DecodeButton.Click += (_, _) =>
        {
            bool isReceiveInputValid = ValidateDecodeInput();

            if (isReceiveInputValid)
            {
                DecodeVectors(isEncoded: true);
                DecodeVectors(isEncoded: false);
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

        string receivedVectorNoSpaces = EncodedReceivedVectorTextBox.Text.Replace(" ", "");

        if (!_validationService.IsBinaryVectorValid(receivedVectorNoSpaces))
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

        EncodedReceivedVectorTextBox.Foreground = Brushes.Black;
        return true;
    }

    private void SendVectors(bool isEncoded)
    {
        try
        {
            int m = int.Parse(InputM.Text.Trim());

            SentVectorLength = m + 1;

            string inputProbabilitySafe = InputProbability.Text.Replace('.', ',');
            double probabilityOfDistortion = double.Parse(inputProbabilitySafe);

            if (isEncoded)
            {
                AppendToLogs($"[E] Input Text: {InputText.Text}");
                EncodedInputTextTextBox.Text = InputText.Text;
            }
            else
            {
                AppendToLogs($"[NE] Input Text: {InputText.Text}");
                NotEncodedInputTextTextBox.Text = InputText.Text;
            }

            (List<int[]>, int) inputVectors = _converterService.GetVectorsFromText(InputText.Text, m);

            LastVectorPadding = inputVectors.Item2;

            if (isEncoded)
            {
                AppendToLogs($"[E] Input Vectors: {string.Join(" ", inputVectors.Item1.Select(i => string.Join("", i)))}");
                EncodedInputVectorTextBox.Text = string.Join(" ", inputVectors.Item1.Select(i => string.Join("", i)));
            }
            else
            {
                AppendToLogs($"[NE] Input Vectors: {string.Join(" ", inputVectors.Item1.Select(i => string.Join("", i)))}");
                NotEncodedInputVectorTextBox.Text = string.Join(" ", inputVectors.Item1.Select(i => string.Join("", i)));
            }

            List<int[]> processedVectors = new List<int[]>();

            if (isEncoded)
            {
                foreach (int[] inputVector in inputVectors.Item1)
                {
                    processedVectors.Add(_encoderService.Encode(inputVector, _matrixService.GetGeneratorMatrix(m)));
                }
            }
            else
            {
                foreach (int[] inputVector in inputVectors.Item1)
                {
                    processedVectors.Add(inputVector);
                }
            }

            if (isEncoded)
            {
                EncodedEncodedVectorTextBox.Text = string.Join(" ", processedVectors.Select(i => string.Join("", i)));
            }

            foreach (int[] processedVector in processedVectors)
            {
                if (isEncoded)
                {
                    AppendToLogs($"[E] Processed Vector: {string.Join("", processedVector)}");
                }
                else
                {
                    AppendToLogs($"[NE] Processed Vector: {string.Join("", processedVector)}");
                }

                int[] receivedVector = _noisyChannelService.GetOutputVector(processedVector, probabilityOfDistortion);

                if (isEncoded)
                {
                    ReceivedVectorsEncoded.Add(receivedVector);

                    AppendToLogs($"[E] Received Vector: {string.Join("", receivedVector)}");
                    EncodedReceivedVectorTextBox.Text = string.Join(" ", ReceivedVectorsEncoded.Select(i => string.Join("", i)));
                }
                else
                {
                    ReceivedVectorsNotEncoded.Add(receivedVector);

                    AppendToLogs($"[NE] Received Vector: {string.Join("", receivedVector)}");
                    NotEncodedReceivedVectorTextBox.Text = string.Join(" ", ReceivedVectorsNotEncoded.Select(i => string.Join("", i)));
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

    private void DecodeVectors(bool isEncoded)
    {
        try
        {
            List<int[]> decodedVectors = new List<int[]>();

            int m = SentVectorLength - 1;

            if (isEncoded)
            {
                foreach (int[] receivedVector in ReceivedVectorsEncoded)
                {
                    List<int[][]> hMatrices = _matrixService.GetHMatrices(m);

                    int[] decodedVector = _decoderService.Decode(receivedVector, m, hMatrices);

                    decodedVectors.Add(decodedVector);

                    AppendToLogs($"[E] Decoded Vector: {string.Join("", decodedVector)}");
                }

                EncodedDecodedVectorTextBox.Text = string.Join(" ", decodedVectors.Select(i => string.Join("", i)));
                EncodedOutputTextTextBox.Text = _converterService.GetTextFromVectors(decodedVectors, LastVectorPadding);
            }
            else
            {
                NotEncodedOutputTextTextBox.Text = _converterService.GetTextFromVectors(ReceivedVectorsNotEncoded, LastVectorPadding);
            }
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
