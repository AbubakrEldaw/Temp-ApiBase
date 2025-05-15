using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class User
    {
        public User()
        {
            PaymentSessions = new HashSet<PaymentSession>();
            RefreshTokens = new HashSet<RefreshToken>();
            TransactionCreatedByNavigations = new HashSet<Transaction>();
            TransactionModifiedByNavigations = new HashSet<Transaction>();
            TransactionPaidByNavigations = new HashSet<Transaction>();
            UserLinkLinkedUsers = new HashSet<UserLink>();
            UserLinkUsers = new HashSet<UserLink>();
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string EmployeeId { get; set; }
        public bool IsActive { get; set; }
        public string CompanyId { get; set; }
        public string PhoneNumber { get; set; }
        public bool? IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string Otp { get; set; }
        public DateTime? OtpSentAt { get; set; }
        public int OtpResendCount { get; set; }
        public bool? IsAdmin { get; set; }

        public virtual ICollection<PaymentSession> PaymentSessions { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
        public virtual ICollection<Transaction> TransactionCreatedByNavigations { get; set; }
        public virtual ICollection<Transaction> TransactionModifiedByNavigations { get; set; }
        public virtual ICollection<Transaction> TransactionPaidByNavigations { get; set; }
        public virtual ICollection<UserLink> UserLinkLinkedUsers { get; set; }
        public virtual ICollection<UserLink> UserLinkUsers { get; set; }
    }
}
