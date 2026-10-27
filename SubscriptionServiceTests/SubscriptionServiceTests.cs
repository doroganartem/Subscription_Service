using Moq;
using Subscription_Service.Models;
using Subscription_Service.Services;
using Subscription_Service.Services.Interfaces;

namespace SubscriptionServiceTests
{
    public class SubscriptionServiceTests
    {
        private readonly Mock<IMemberRepository> _repo = new();
        private readonly Mock<IPaymentService> _payment = new();
        private readonly Mock<INotificationService> _notify = new();
        private readonly SubscriptionService _service;

        public SubscriptionServiceTests()
        {
            _service = new SubscriptionService(_repo.Object, _payment.Object, _notify.Object);
        }

        /// <summary>
        /// Перевіряє викидання винятку ArgumentException, коли користувача не знайдено
        /// </summary>
        [Fact]
        public void RenewSubscription_WhenMemberNotFound_ThrowsArgumentException()
        {
            // Arrange
            _repo.Setup(r => r.GetById(It.IsAny<int>())).Returns((Member?)null);

            // Act & Assert: 
            Assert.Throws<ArgumentException>(() => 
                _service.RenewSubscription(999, 100m, 30));
        }

        /// <summary>
        /// Успішне продовження підписки 
        /// </summary>
        [Fact]
        public void RenewSubscription_WhenSuccessful_UpdatesMemberAndNotifies()
        {
            // Arrange
            int memberId = 3;
            decimal amount = 200m;
            int days = 30;
            var member = new Member { Id = memberId, IsActive = false };

            _repo.Setup(r => r.GetById(memberId)).Returns(member);
            _payment.Setup(p => p.VerifyPayment(memberId, amount)).Returns(true);

            // Act
            var result = _service.RenewSubscription(memberId, amount, days);

            // Assert
            Assert.True(result);
            _repo.Verify(r => r.Update(It.Is<Member>(m => m.IsActive == true)), Times.Exactly(1));
            _notify.Verify(n => n.SendNotification("Subscription renewed!", memberId), Times.AtLeastOnce);
        }

        /// <summary>
        /// Перевірка деактивації, коли у репозиторії немає користувачів
        /// </summary>
        [Fact]
        public void DeactivateExpiredMembers_WhenNoMembers_DoesNotCallUpdate()
        {
            // Arrange
            var emptyList = new List<Member>();
            _repo.Setup(r => r.GetAll()).Returns(emptyList);

            // Act
            _service.DeactivateExpiredMembers();

            // Assert
            Assert.Empty(emptyList);
            _repo.Verify(r => r.Update(It.IsAny<Member>()), Times.Never);
        }

    }

}
