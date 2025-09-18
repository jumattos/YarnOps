using Microsoft.VisualStudio.Extensibility.UI;
using System.Runtime.Serialization;
using System.Text;
using System.Collections.Generic;

namespace VisualStudio.Extension
{
    [DataContract]
    internal class TrackerToolWindowData : NotifyPropertyChangedObject
    {
        private string[] _patternLines = Array.Empty<string>();
        private (string instruction, int stitchCount)[] _patternData = Array.Empty<(string, int)>();

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
                UpdateStitchProgress();
                return Task.CompletedTask;
            });

            NextRowCommand = new AsyncCommand((parameter, clientContext, cancellationToken) =>
            {
                Row++;
                Stitch = 0; // Reset stitch count for new row
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
                    
                    // Parse CSV pattern data
                    ParsePatternData();
                    
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

        private void ParsePatternData()
        {
            var patternData = new List<(string instruction, int stitchCount)>();
            
            foreach (string line in _patternLines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                
                var parts = ParseCsvLine(line);
                if (parts.Count >= 2)
                {
                    string instruction = parts[0].Trim();
                    
                    // Remove surrounding quotes from instruction if present
                    if (instruction.StartsWith("\"") && instruction.EndsWith("\"") && instruction.Length > 1)
                    {
                        instruction = instruction.Substring(1, instruction.Length - 2);
                    }
                    
                    // Try to parse the stitch count from the second column
                    string stitchCountStr = parts[1].Trim();
                    if (int.TryParse(stitchCountStr, out int stitchCount))
                    {
                        patternData.Add((instruction, stitchCount));
                    }
                    else
                    {
                        // If parsing fails, use 0 as default
                        patternData.Add((instruction, 0));
                    }
                }
                else if (parts.Count == 1)
                {
                    // If line has only one column, treat as instruction only
                    string instruction = parts[0].Trim();
                    if (instruction.StartsWith("\"") && instruction.EndsWith("\"") && instruction.Length > 1)
                    {
                        instruction = instruction.Substring(1, instruction.Length - 2);
                    }
                    patternData.Add((instruction, 0));
                }
            }
            
            _patternData = patternData.ToArray();
        }

        private List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            var currentField = new StringBuilder();
            bool inQuotes = false;
            
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // Handle escaped quotes (double quotes within quoted field)
                        currentField.Append('"');
                        i++; // Skip the next quote
                    }
                    else
                    {
                        // Toggle quote state
                        inQuotes = !inQuotes;
                        currentField.Append(c); // Keep the quote in the field
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    // Field separator found outside quotes
                    result.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    // Regular character
                    currentField.Append(c);
                }
            }
            
            // Add the last field
            result.Add(currentField.ToString());
            
            return result;
        }

        private void UpdateInstructionFromPattern()
        {
            if (_patternData.Length == 0)
            {
                Instruction = "Load pattern to see instructions here.";
                TotalStitchesInRow = 0;
                UpdateStitchProgress();
                return;
            }

            // Get the instruction for the current row (1-based indexing)
            int rowIndex = Row - 1;
            if (rowIndex >= 0 && rowIndex < _patternData.Length)
            {
                var (instruction, stitchCount) = _patternData[rowIndex];
                Instruction = instruction;
                TotalStitchesInRow = stitchCount;
                UpdateStitchProgress();
            }
            else if (rowIndex >= _patternData.Length)
            {
                Instruction = $"Pattern complete! (Only {_patternData.Length} rows in pattern)";
                TotalStitchesInRow = 0;
                UpdateStitchProgress();
            }
            else
            {
                Instruction = "Load pattern to see instructions here.";
                TotalStitchesInRow = 0;
                UpdateStitchProgress();
            }
        }

        private void UpdateStitchProgress()
        {
            if (TotalStitchesInRow > 0)
            {
                RemainingStitches = Math.Max(0, TotalStitchesInRow - Stitch);
                ProgressPercentage = TotalStitchesInRow > 0 ? (double)Stitch / TotalStitchesInRow * 100 : 0;
            }
            else
            {
                RemainingStitches = 0;
                ProgressPercentage = 0;
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
            set 
            { 
                // Ensure stitch count doesn't exceed total stitches in row and isn't negative
                int clampedValue = Math.Max(0, value);
                if (TotalStitchesInRow > 0)
                {
                    clampedValue = Math.Min(clampedValue, TotalStitchesInRow);
                }
                
                if (SetProperty(ref this._stitch, clampedValue))
                {
                    UpdateStitchProgress();
                }
            }
        }

        private string _filePath = "";
        [DataMember]
        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref this._filePath, value ?? "");
        }

        private int _totalStitchesInRow = 0;
        [DataMember]
        public int TotalStitchesInRow
        {
            get => _totalStitchesInRow;
            set => SetProperty(ref this._totalStitchesInRow, value);
        }

        private int _remainingStitches = 0;
        [DataMember]
        public int RemainingStitches
        {
            get => _remainingStitches;
            set => SetProperty(ref this._remainingStitches, value);
        }

        private double _progressPercentage = 0;
        [DataMember]
        public double ProgressPercentage
        {
            get => _progressPercentage;
            set => SetProperty(ref this._progressPercentage, value);
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