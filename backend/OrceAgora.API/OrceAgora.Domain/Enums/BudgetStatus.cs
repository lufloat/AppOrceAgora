namespace OrceAgora.Domain.Enums;

public enum BudgetStatus
{
    Draft,
    Sent,
    Viewed,
    Approved,
    Rejected,
    Done
}

public enum PlanType { Basic, Pro }

public enum DiscountType { Fixed, Percent }

public enum PaymentStatus { Pending, Paid, Overdue, Cancelled }