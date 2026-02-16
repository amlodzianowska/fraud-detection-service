using FraudDetection.Core.Entities;

namespace FraudDetection.Api.Models;

public class ReviewOrderRequestDto
{
    public FraudDecision Decision { get; set; }
    public string? Reason { get; set; }
}