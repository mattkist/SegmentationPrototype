namespace Segmentation.Application.Exceptions;

public sealed class SegmentationConfigurationValidationException : Exception
{
    public SegmentationConfigurationValidationException(
        string message,
        int sumOfKpiMaxScores,
        int maximumScore,
        IReadOnlyList<string>? errors = null)
        : base(message)
    {
        SumOfKpiMaxScores = sumOfKpiMaxScores;
        MaximumScore = maximumScore;
        Errors = errors ?? [];
    }

    public int SumOfKpiMaxScores { get; }
    public int MaximumScore { get; }
    public IReadOnlyList<string> Errors { get; }
}
