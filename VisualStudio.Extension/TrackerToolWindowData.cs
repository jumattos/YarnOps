using Microsoft.VisualStudio.Extensibility.UI;
using System.Runtime.Serialization;

namespace VisualStudio.Extension
{
    [DataContract]
    internal class TrackerToolWindowData : NotifyPropertyChangedObject
    {
        private string[] _patternLines = Array.Empty<string>();

        public TrackerToolWindowData()
        {
            AddOrRemoveIncreaseCommand = new AsyncCommand((parameter, clientContext, cancellationToken) =>
            {
                if (string.Equals("+", parameter))
                    Increase++;
                else if (string.Equals("-", parameter) && Increase > 1)
                    Increase--;

                return Task.CompletedTask;
            });

            AddStitchesCommand = new AsyncCommand((parameter, clientContext, cancellationToken) =>
            {
                int inc = parameter as int? ?? 0;
                Stitch += inc;
                return Task.CompletedTask;
            });

            NextRowCommand = new AsyncCommand((parameter, clientContext, cancellationToken) =>
            {
                Row++;
                UpdateInstructionFromPattern();
                return Task.CompletedTask;
            });

            LoadPatternCommand = new AsyncCommand(async (parameter, clientContext, cancellationToken) =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(FilePath))
                    {
                        Instruction = "Please enter a file path to load a pattern.";
                        return;
                    }

                    // Handle optional quotes around the file path (common from "Copy as path")
                    string cleanPath = FilePath.Trim();
                    if (cleanPath.StartsWith("\"") && cleanPath.EndsWith("\"") && cleanPath.Length > 1)
                    {
                        cleanPath = cleanPath.Substring(1, cleanPath.Length - 2);
                    }

                    if (!File.Exists(cleanPath))
                    {
                        Instruction = $"File not found: {cleanPath}";
                        return;
                    }

                    string[] lines = await File.ReadAllLinesAsync(cleanPath, cancellationToken);
                    _patternLines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
                    
                    // Reset to row 1 and update instruction
                    Row = 1;
                    UpdateInstructionFromPattern();
                }
                catch (Exception ex)
                {
                    Instruction = $"Error loading pattern: {ex.Message}";
                }
            });
        }

        private void UpdateInstructionFromPattern()
        {
            if (_patternLines.Length == 0)
            {
                Instruction = "Load pattern to see instructions here.";
                return;
            }

            // Get the instruction for the current row (1-based indexing)
            int rowIndex = Row - 1;
            if (rowIndex >= 0 && rowIndex < _patternLines.Length)
            {
                Instruction = _patternLines[rowIndex];
            }
            else if (rowIndex >= _patternLines.Length)
            {
                Instruction = $"Pattern complete! (Only {_patternLines.Length} rows in pattern)";
            }
            else
            {
                Instruction = "Load pattern to see instructions here.";
            }
        }

        private int _increase = 1;
        [DataMember]
        public int Increase
        {
            get => _increase;
            set => SetProperty(ref this._increase, Math.Max(1, value));
        }

        private string _instruction = "Load pattern to see instructions here.";
        [DataMember]
        public string Instruction
        {
            get => _instruction;
            set => SetProperty(ref this._instruction, value);
        }

        private int _row = 1;
        [DataMember]
        public int Row
        {
            get => _row;
            set 
            { 
                if (SetProperty(ref this._row, Math.Max(1, value)))
                {
                    UpdateInstructionFromPattern();
                }
            }
        }

        private int _stitch = 0;
        [DataMember]
        public int Stitch
        {
            get => _stitch;
            set => SetProperty(ref this._stitch, Math.Max(0, value));
        }

        private string _filePath = "";
        [DataMember]
        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref this._filePath, value ?? "");
        }

        [DataMember]
        public AsyncCommand AddOrRemoveIncreaseCommand { get; }

        [DataMember]
        public AsyncCommand AddStitchesCommand { get; }

        [DataMember]
        public AsyncCommand NextRowCommand { get; }

        [DataMember]
        public AsyncCommand LoadPatternCommand { get; }
    }
}