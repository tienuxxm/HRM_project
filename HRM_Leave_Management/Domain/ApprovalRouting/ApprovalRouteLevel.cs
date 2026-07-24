using Domain.Abstractions;

namespace Domain.ApprovalRouting;

public class ApprovalRouteLevel : Entity<ApprovalRouteLevelId>
{
    private readonly List<ApprovalRouteLevelAssignment> _assignments = new();

    private ApprovalRouteLevel(
        ApprovalRouteLevelId id,
        ApprovalRoutePolicyId policyId,
        string levelName,
        int levelRank,
        bool canApproveLeave,
        bool isActive)
        : base(id)
    {
        PolicyId = policyId;
        LevelName = levelName;
        LevelRank = levelRank;
        CanApproveLeave = canApproveLeave;
        IsActive = isActive;
    }

    private ApprovalRouteLevel()
    {
    }

    public ApprovalRoutePolicyId PolicyId { get; private set; } = null!;
    public ApprovalRoutePolicy? Policy { get; private set; }
    public string LevelName { get; private set; } = null!;
    public int LevelRank { get; private set; }
    public bool CanApproveLeave { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<ApprovalRouteLevelAssignment> Assignments => _assignments.AsReadOnly();

    public static ApprovalRouteLevel Create(
        ApprovalRoutePolicyId policyId,
        string levelName,
        int levelRank,
        bool canApproveLeave = true)
    {
        if (policyId == null)
            throw new ArgumentNullException(nameof(policyId));

        if (string.IsNullOrWhiteSpace(levelName))
            throw new ArgumentException("Level name cannot be empty.", nameof(levelName));

        if (levelRank < 1)
            throw new ArgumentOutOfRangeException(nameof(levelRank), "LevelRank must be >= 1.");

        return new ApprovalRouteLevel(
            ApprovalRouteLevelId.New(),
            policyId,
            levelName.Trim(),
            levelRank,
            canApproveLeave,
            isActive: true);
    }

    public void Update(string levelName, int levelRank, bool canApproveLeave, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(levelName))
            throw new ArgumentException("Level name cannot be empty.", nameof(levelName));

        if (levelRank < 1)
            throw new ArgumentOutOfRangeException(nameof(levelRank), "LevelRank must be >= 1.");

        LevelName = levelName.Trim();
        LevelRank = levelRank;
        CanApproveLeave = canApproveLeave;
        IsActive = isActive;
    }

    public void AddAssignment(ApprovalRouteLevelAssignment assignment)
    {
        if (assignment == null)
            throw new ArgumentNullException(nameof(assignment));

        _assignments.Add(assignment);
    }
}
