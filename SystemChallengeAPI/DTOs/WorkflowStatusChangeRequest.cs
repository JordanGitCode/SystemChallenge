namespace SystemChallengeAPI.DTOs
{
    public class WorkflowStatusChangeRequest
    {
        public Guid productId {  get; set; }
        public Guid versionId { get; set; }
        public string? DecisionReason { get; set; }
    }
}
