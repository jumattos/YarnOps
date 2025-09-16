using Microsoft.VisualStudio.Extensibility.UI;
using System.Runtime.Serialization;

namespace VisualStudio.Extension
{
    [DataContract]
    internal class TrackerToolWindowData : NotifyPropertyChangedObject
    {
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
                return Task.CompletedTask;
            });
        }

        private int _increase = 1;
        [DataMember]
        public int Increase
        {
            get => _increase;
            set => SetProperty(ref this._increase, value);
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
            set => SetProperty(ref this._row, value);
        }

        private int _stitch = 0;
        [DataMember]
        public int Stitch
        {
            get => _stitch;
            set => SetProperty(ref this._stitch, value);
        }

        [DataMember]
        public AsyncCommand AddOrRemoveIncreaseCommand { get; }

        [DataMember]
        public AsyncCommand AddStitchesCommand { get; }

        [DataMember]
        public AsyncCommand NextRowCommand { get; }
    }
}
