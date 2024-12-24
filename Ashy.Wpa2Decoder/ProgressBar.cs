using Ashy.Wpa2Decoder.Library;

namespace Ashy.Wpa2Decoder;

public class ProgressBar : IProgressBar
{
    public class RoundInfo
    {
        public bool IsEmpty => TotalRounds == 0 || StepTitles.Length == 0;
        public int TotalRounds { get; private set; }
        public int CurrentRound { get; private set; }
        public int CurrentStep { get; private set; }

        // List of step titles for each round
        public string[] StepTitles { get; private set; }
        public ConsoleColor StepHighlightColor { get; set; } // Color for highlighting the current step
        
        public static RoundInfo Empty => new RoundInfo(0, []);

        public RoundInfo(int totalRounds, IEnumerable<string> stepTitles)
        {
            if (totalRounds < 0)
                throw new ArgumentOutOfRangeException(nameof(totalRounds), "Total rounds must be greater than zero.");

            var steps = stepTitles as string[] ?? stepTitles.ToArray();
            if (stepTitles == null)
                throw new ArgumentException("Step titles cannot be null.", nameof(stepTitles));

            TotalRounds = totalRounds;
            CurrentRound = 1; // Default to the first round
            CurrentStep = 1; // Default to the first step
            StepTitles = steps.ToArray();
        }

        // Set the current round and step manually
        public void SetRoundAndStep(int totalRounds, int round, int step)
        {
            if (round < 1 || round > TotalRounds)
                throw new ArgumentOutOfRangeException(nameof(round),
                    "Round must be between 1 and the total number of rounds.");
            if (step < 1 || step > StepTitles.Length)
                throw new ArgumentOutOfRangeException(nameof(step),
                    "Step must be between 1 and the total number of steps per round.");

            TotalRounds = totalRounds;
            CurrentRound = round;
            CurrentStep = step;
        }

        public int WriteLine()
        {
            if(IsEmpty)
            {
                return 0;
            };
            
            // 1. Write the round info "Round X of Y"
            Console.Write("Round ");
            Console.ForegroundColor = StepHighlightColor;
            Console.Write(CurrentRound);
            Console.ResetColor();
            Console.Write(" of ");
            Console.ForegroundColor = StepHighlightColor;
            Console.Write(TotalRounds);
            Console.ResetColor();
            Console.Write(": ");

            // 2. Write the steps, highlighting the current step
            for (int i = 0; i < StepTitles.Length; i++)
            {
                // For the current step, apply the highlight color
                if (i == CurrentStep - 1)
                {
                    Console.ForegroundColor = StepHighlightColor;
                    Console.Write($"[{StepTitles[i]}]");
                    Console.ResetColor();
                }
                else
                {
                    Console.Write(StepTitles[i]);
                }

                // If it's not the last step, add the separator "--"
                if (i < StepTitles.Length - 1)
                {
                    var separator = i == CurrentStep - 2 || i == CurrentStep - 1 ? "-" : "--";
                    Console.Write(separator);
                }
            }

            Console.WriteLine();
            return 1;
        }
    }
    
    public long TotalTicks { get; set; }
    private long _currentTick = 0;
    private readonly DateTime _startTime;
    private readonly int _barWidth;
    private bool _disposed = false;
    private readonly ConsoleColor _progressBarColor;
    private readonly int _progressBarLine;
    private readonly RoundInfo _roundInformation;

    public ProgressBar(long totalTicks, ConsoleColor progressBarColor = ConsoleColor.Yellow)
    :this (totalTicks, RoundInfo.Empty, progressBarColor)
    {
        if (totalTicks <= 0)
            throw new ArgumentOutOfRangeException(nameof(totalTicks), "Total ticks must be greater than zero.");
    }

    public ProgressBar(RoundInfo roundInformation, ConsoleColor progressBarColor = ConsoleColor.Yellow)
    : this(0, roundInformation, progressBarColor)
    {
    }

    private ProgressBar(long totalTicks, RoundInfo roundInformation, ConsoleColor progressBarColor = ConsoleColor.Yellow)
    {

        TotalTicks = totalTicks;
        _barWidth = Console.WindowWidth - 40; // Adjust space for the timer and additional info on the right
        _startTime = DateTime.Now;
        _progressBarColor = progressBarColor;
        _progressBarLine = Console.CursorTop;
        _roundInformation = roundInformation;
        _roundInformation.StepHighlightColor = progressBarColor;
    }
    
    public void SetRoundAndStep(int totalRounds, int round, int step)
    {
        _roundInformation.SetRoundAndStep(totalRounds, round, step);
    }

    public void Report(long currentTicks, string additionalInfo)
    {

        if (currentTicks < 0 || currentTicks > TotalTicks)
            throw new ArgumentOutOfRangeException(nameof(currentTicks), "Ticks must be between 0 and totalTicks.");
        
        Console.SetCursorPosition(0, _progressBarLine);
        var linesWritten = _roundInformation.WriteLine();

        _currentTick = currentTicks;

        // Calculate the progress bar's width and percentage
        int progressWidth = (int)((_currentTick * _barWidth) / TotalTicks);
        int percentage = (int)((_currentTick * 100) / TotalTicks);
        
        Console.ForegroundColor = _progressBarColor;
        string progressBar = new string('#', progressWidth) + new string('-', _barWidth - progressWidth);
        Console.Write($"[{progressBar}] {percentage}%");
        int cursorLeft = Console.CursorLeft;
        string clearString = new string(' ', Console.WindowWidth - Console.CursorLeft);
        Console.Write(clearString);
        Console.CursorLeft = cursorLeft;

        // Add additional info to the right of the progress bar
        if (!string.IsNullOrEmpty(additionalInfo))
        {
            // Calculate the remaining space after the progress bar and percentage
            int spaceRemaining = Console.WindowWidth - 2 - _barWidth - 7; // 7 is for the " 100%" text

            if (spaceRemaining > 0)
            {
                Console.SetCursorPosition(_barWidth + 8, _progressBarLine + linesWritten); // After progress bar and percentage
                Console.Write(additionalInfo.PadRight(spaceRemaining));
            }
        }
        
        // Display the timer
        var elapsedTime = DateTime.Now - _startTime;
        string timerText = $"{elapsedTime.Hours:D2}:{elapsedTime.Minutes:D2}:{elapsedTime.Seconds:D2}";

        // Set the cursor to the last line and position the timer at the far right
        Console.SetCursorPosition(Console.WindowWidth - timerText.Length - 1, _progressBarLine + linesWritten);
        Console.ForegroundColor = _progressBarColor;  // Timer color
        Console.Write(timerText);
        Console.ResetColor();
        Console.Out.Flush();
    }

    public void Complete()
    {
        // Finalize the progress bar when it's complete
        Report(TotalTicks, "Completed");
        Console.WriteLine();  // Move to the next line after the progress bar is complete
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Complete();
            _disposed = true;
        }
    }

    ~ProgressBar()
    {
        Dispose();
    }
}

